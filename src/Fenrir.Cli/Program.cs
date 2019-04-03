using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fenrir.Core;
using Fenrir.Core.Extensions;
using Fenrir.Core.Generators;
using Fenrir.Core.Jobs;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Fenrir.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "fenrir",
                Description = "Service testing tool that compares and load tests microservices."
            };

            app.OnExecute(() => {
                app.ShowHelp();
                return 1;
            });
            
            app.HelpOption("-? | -h | --help", true);

            app.Command("simple", config => {
                config.Description = "run simple load test agent";
                config.HelpOption("-? | -h | --help");

                var threadsOp = config.Option<int>("-t", "number of parallel threads to use", CommandOptionType.SingleValue);
                var durationOp = config.Option("-d", "length of time", CommandOptionType.SingleValue);
                var url = config.Argument("url", "load test url", false);

                config.OnExecute(async () => {
                    Uri uri = null;
                    if (string.IsNullOrWhiteSpace(url.Value) || !Uri.TryCreate(url.Value, UriKind.Absolute, out uri))
                    {
                        Console.WriteLine($"{url.Value} is not a valid url");
                        return 1;
                    }

                    int threads = threadsOp.HasValue() ? threadsOp.ParsedValue : 1;
                    TimeSpan duration = durationOp.HasValue() ? TimeSpan.Parse(durationOp.Value()) : TimeSpan.FromSeconds(30);

                    Console.WriteLine(CliResultViews.StartSimpleWithDurationString, duration.TotalSeconds, uri, threads);

                    var requestResult = await RunSimpleLoadTestAsync(uri, threads, duration);

                    CliResultViews.DrawStatusCodes(requestResult.Stats);
                    CliResultViews.DrawStats(requestResult.Stats);

                    return 0; 
                });
            }, false);

            app.Command("request", config => {
                config.Description = "run request agent";
                config.HelpOption("-? | -h | --help");

                var threadsOp = config.Option<int>("-t", "number of parallel threads to use", CommandOptionType.SingleValue);
                var requestFileOp = config.Option("-f", "path to request tree json file", CommandOptionType.SingleValue);
                var outputFileOp = config.Option("-o", "path to output json file", CommandOptionType.SingleValue);
                var generatorArg = config.Argument("generator", "generator name and options", true);

                config.OnExecute(async () => {

                    int threads = threadsOp.HasValue() ? threadsOp.ParsedValue : 1;

                    JsonHttpRequestTree requestTree = null; 
                    if (requestFileOp.HasValue() && File.Exists(requestFileOp.Value()))
                    {
                        string json = File.ReadAllText(requestFileOp.Value());
                        requestTree = JsonConvert.DeserializeObject<JsonHttpRequestTree>(json);
                    }

                    string generatorName = null;
                    if (generatorArg.Values != null && generatorArg.Values.Count > 0)
                    {
                        generatorName = generatorArg.Values[0];

                        var loader = new RequestGeneratorPluginLoader(PluginDir());
                        var requestGenerator = loader.Load().First(g => g.Name.Equals(generatorName, StringComparison.InvariantCultureIgnoreCase));
                                                
                        // add options
                        for (int i = 0; i < generatorArg.Values.Count; i++)
                        {
                            string argument = null;
                            string value = null;
                            if(generatorArg.Values[i].StartsWith("#"))
                            {
                                argument = generatorArg.Values[i].TrimStart('#');
                                value = generatorArg.Values[i + 1];
                            }

                            int index = -1;
                            if(!string.IsNullOrWhiteSpace(argument) 
                                && (index = requestGenerator.Options.FindLastIndex(o => o.Description.Key.Equals(argument))) > -1)
                            {
                                requestGenerator.Options[index].Value = value; 
                            }
                        }

                        var requests = requestGenerator.Run();

                        requestTree = new JsonHttpRequestTree() { Requests = requests, Description = requestGenerator.Name };
                    }

                    if (requestTree == null)
                    {
                        throw new ArgumentException("generator or json file must be supplied");
                    }

                    // Draw run header
                    Console.WriteLine(CliResultViews.StartRequestString, 
                        !string.IsNullOrWhiteSpace(generatorName) 
                            ? generatorName 
                            : requestFileOp.Value(), 
                        threads);

                    var requestResult = await RunRequestComparisonAsync(threads, requestTree);
                    var grades = requestResult.Grades;
                    var stats = requestResult.Stats;

                    // Draw Stats
                    CliResultViews.DrawGrades(grades);
                    CliResultViews.DrawStatusCodes(stats);
                    CliResultViews.DrawStats(stats);

                    var time = DateTime.UtcNow.ToString("yyyyMMddTHHmmss");
                    var result = new JsonHttpRequestTree()
                    {
                        Requests = requestResult.Grades.Requests,
                        Description = $"{requestTree.Description} : {time}"
                    };

                    string outputFile = outputFileOp.HasValue() 
                        ? outputFileOp.Value() 
                        : $"output-{time}.json";

                    // Draw output file path
                    Console.WriteLine("Result path: {0}", outputFile); 

                    File.WriteAllText(outputFile, 
                        JsonConvert.SerializeObject(
                            result, 
                            new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }));

                    return 0;
                });
            }, false);

            app.Command("generators", config => {
                config.Description = "Generator plugings";
                config.HelpOption("-? | -h | --help");
                
                config.OnExecute(async () => {
                    var loader = new RequestGeneratorPluginLoader(PluginDir());
                    foreach(var generator in loader.Load())
                    {
                        CliResultViews.DrawGenerator(generator); 
                    }

                    return 0;
                });
            }, false);

            try 
            {
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute: {0}", ex.Message);
            }
        }

        private static async Task<AgentResult> RunSimpleLoadTestAsync(Uri uri, int threads, TimeSpan duration)
        {
            var generator = new SimpleLoadTestGenerator(); 
            
            var durationOption = generator.Options.First(o => o.Description.Key == "Duration");
            durationOption.Value = duration.Seconds.ToString();

            var urlOption = generator.Options.First(o => o.Description.Key == "Url");
            urlOption.Value = uri.AbsoluteUri;

            var tree = new JsonHttpRequestTree()
            {
                Requests = generator.Run()
            };

            return await RunRequestComparisonAsync(threads, tree);
        }

        private static async Task<AgentResult> RunRequestComparisonAsync(int threads, JsonHttpRequestTree requestTree)
        {
            var agent = new RequestTreeAgent(requestTree);
            return await agent.Run(threads, new System.Threading.CancellationToken());
        }

        public static string PluginDir()
        {
            var path = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fenrir/plugins"));
                   
            if (!Directory.Exists(path))
            {
                throw new ArgumentException($"plugin path ( {path} ) does not exist");
            }
            
            return path;
        }
    }
}
