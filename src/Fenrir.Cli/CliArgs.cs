﻿using CsvHelper;
using CsvHelper.Configuration;
using Fenrir.Core;
using Fenrir.Core.Generators;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;
using PowerArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fenrir.Cli
{
    [TabCompletion]
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    [ArgDescription("Service testing tool for load testing, environment comparison, response testing, and integration testing microservices.")]
    [ArgExample("fenrir request -f \"requestjsonfile.json\" -t 100", "", Title = "file request tree example")]
    [ArgExample("fenrir generator \"Generator Plugin Name\" -a \"#pluginOption\" \"pluginOptionValue\" -t 100", "user created generators with options", Title = "Generator plugin example")]
    [ArgExample("fenrir simple -url \"https://example.domain/resource/1\" -t 100 -c 10000", "", Title = "simple load test example")]
    public class CliArgs
    {
        [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
        public bool Help { get; set; }

        [ArgActionMethod, ArgDescription("Run simple load"), ArgShortcut("s")]
        public async Task Simple(SimpleArgs args)
        {
            Console.WriteLine(CliResultViews.StartSimpleWithDurationString, args.Duration.TotalSeconds, args.Url, args.Concurrency);

            var requestResult = await RunSimpleLoadTestAsync(args.Url, args.Concurrency, args.Duration, args.Count);

            CliResultViews.DrawStatusCodes(requestResult.Stats);
            CliResultViews.DrawStats(requestResult.Stats);
        }

        [ArgActionMethod, ArgDescription("List generator plugins"), ArgShortcut("l")]
        public void List(ListArgs args)
        {
            var loader = new RequestGeneratorPluginLoader(PluginDir());
            foreach (var generator in loader.Load())
            {
                CliResultViews.DrawGenerator(generator);
            }
        }

        [ArgActionMethod, ArgDescription("Generate requests using request plugin"), ArgShortcut("g")]
        public async Task Generator(GeneratorArgs args)
        {
            var loader = new RequestGeneratorPluginLoader(PluginDir());
            var requestGenerator = loader.Load().First(g => g.Name.Equals(args.Name, StringComparison.InvariantCultureIgnoreCase));

            // add options
            if (args.Arguments != null && args.Arguments.Count > 0)
            {
                for (int i = 0; i < args.Arguments.Count; i++)
                {
                    string argument = null;
                    string value = null;
                    if (args.Arguments[i].StartsWith("#"))
                    {
                        argument = args.Arguments[i].TrimStart('#');
                        value = args.Arguments[i + 1];
                    }

                    int index = -1;
                    if (!string.IsNullOrWhiteSpace(argument)
                        && (index = requestGenerator.Options.FindLastIndex(o => o.Description.Key.Equals(argument))) > -1)
                    {
                        requestGenerator.Options[index].Value = value;
                    }
                }
            }

            var requests = requestGenerator.Run();

            HttpRequestTree requestTree = new HttpRequestTree() { Requests = requests, Description = requestGenerator.Name };
            await RunRequestTreeAgent(requestTree, args.Concurrency, requestGenerator.Name, args.OutputFilePath);
        }

        [ArgActionMethod, ArgDescription("Generate requests from file or pipe"), ArgShortcut("r")]
        public async Task Request(RequestArgs args)
        {
            HttpRequestTree requestTree = null;

            // check if we have piped 
            // input
            if (Console.IsInputRedirected)
            {
                using (Stream pipeStream = Console.OpenStandardInput())
                {
                    requestTree = ReadTsv(pipeStream);
                }
            }

            // load from file
            if (requestTree == null && !string.IsNullOrWhiteSpace(args.RequestFilePath))
            {
                try
                {
                    // try to load json
                    requestTree = ReadJson(args.RequestFilePath);
                }
                catch
                {
                    // try to load tsv
                    requestTree = ReadTsv(args.RequestFilePath);
                }
            }

            string requestSource = !string.IsNullOrWhiteSpace(args.RequestFilePath)
                ? args.RequestFilePath
                : "Pipe";

            await RunRequestTreeAgent(requestTree, args.Concurrency, requestSource, args.OutputFilePath);
        }

        #region "static helper methods"
        private static async Task RunRequestTreeAgent(HttpRequestTree requestTree, int Concurrency, string requestSource, string outputFilePath)
        {
            // Draw run header
            Console.WriteLine(CliResultViews.StartRequestString, requestSource, Concurrency);

            var requestResult = await RunRequestComparisonAsync(Concurrency, requestTree);
            var grades = requestResult.Grades;
            var stats = requestResult.Stats;

            // Draw Stats
            CliResultViews.DrawGrades(grades);
            CliResultViews.DrawStatusCodes(stats);
            CliResultViews.DrawStats(stats);

            var time = DateTime.UtcNow.ToString("yyyyMMddTHHmmss");
            var result = new HttpRequestTree()
            {
                Requests = requestResult.Grades.Requests,
                Description = $"{requestTree.Description} : {time}"
            };

            string outputFile = !string.IsNullOrWhiteSpace(outputFilePath)
                ? outputFilePath
                : $"output-{time}.json";

            // Draw output file path
            Console.WriteLine("Result path: {0}", outputFile);

            using (var stream = new FileStream(outputFile, FileMode.CreateNew))
            {
                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true
                };

                await JsonSerializer.SerializeAsync<HttpRequestTree>(stream, result, options);
            }
        }

        private static async Task<AgentResult> RunSimpleLoadTestAsync(Uri uri, int threads, TimeSpan duration, int count)
        {
            var generator = new SimpleLoadTestGenerator();

            var durationOption = generator.Options.First(o => o.Description.Key == "Duration");
            durationOption.Value = duration.Seconds.ToString();

            var countOption = generator.Options.First(o => o.Description.Key == "Count");
            countOption.Value = count.ToString();

            var urlOption = generator.Options.First(o => o.Description.Key == "Url");
            urlOption.Value = uri.AbsoluteUri;

            var tree = new HttpRequestTree()
            {
                Requests = generator.Run()
            };

            return await RunRequestComparisonAsync(threads, tree);
        }

        private static async Task<AgentResult> RunRequestComparisonAsync(int threads, HttpRequestTree requestTree)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            var token = source.Token;

            Console.CancelKeyPress += delegate {
                source.Cancel();
            };

            var agent = new RequestTreeAgent(requestTree);
            return await agent.Run(threads, token);
        }

        /// <summary>
        /// Creates Plugin directory if none exists
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static string PluginDir(string basePath = null)
        {
            basePath = basePath ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.GetFullPath(Path.Combine(basePath, ".fenrir/plugins"));

            if (!Directory.Exists(path))
            {
                // create directory
                try
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"Creating plugin directory: {path}");
                } 
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"Failed to create plugin directory: {path}");
                }
            }

            return path;
        }

        /// <summary>
        /// Build HttpRequestTree from json file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static HttpRequestTree ReadJson(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<HttpRequestTree>(json);
        }

        /// <summary>
        /// Build HttpRequestTree from tsv file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static HttpRequestTree ReadTsv(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return ReadTsv(stream);
            }
        }

        public static HttpRequestTree ReadTsv(Stream TsvStream)
        {
            var config = new Configuration
            {
                Delimiter = "\t",
                IgnoreBlankLines = true
            };

            using (var reader = new StreamReader(TsvStream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<RequestMap>();
                var records = csv.GetRecords<Request>();
                return new HttpRequestTree
                {
                    Requests = records.ToList()
                };
            }
        }
        #endregion "static helper methods"
    }
}
