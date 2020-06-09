using CsvHelper;
using CsvHelper.Configuration;
using Fenrir.Cli.Usecases;
using Fenrir.Core;
using Fenrir.Core.Generators;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;
using PowerArgs;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class Controller
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
            // Load request generator and apply plugin args
            IRequestGenerator requestGenerator = new LoadGenerator().Execute(args, PluginDir());
            IEnumerable<Request> requests = requestGenerator.Run();

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
                    try
                    {
                        // try load TSV
                        requestTree = new LoadHttpRequestTreeFromTsv().Execute(pipeStream);
                    }
                    catch
                    {
                        // try load Url list
                        requestTree = new LoadHttpRequestTreeFromListOfUrls().Execute(pipeStream);
                    }
                }
            }

            // load from file
            if (requestTree == null && !string.IsNullOrWhiteSpace(args.RequestFilePath))
            {
                try
                {
                    // try to load json
                    requestTree = new LoadHttpRequestTreeFromJson().Execute(args.RequestFilePath);
                }
                catch
                {
                    try
                    {
                        // try to load tsv
                        requestTree = new LoadHttpRequestTreeFromTsv().Execute(args.RequestFilePath);
                    }
                    catch
                    {
                        // try to load url list
                        requestTree = new LoadHttpRequestTreeFromListOfUrls().Execute(args.RequestFilePath);
                    }
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

            await new SaveHttpRequestTreeToJson().Execute(result, outputFile);
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
        #endregion "static helper methods"
    }
}
