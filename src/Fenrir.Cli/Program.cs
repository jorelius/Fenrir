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
            
            app.HelpOption("-? | -h | --help");

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

                    var stats = await RunSimpleLoadTestAsync(uri, threads, duration);

                    CliResultViews.DrawStats(stats);

                    return 0; 
                });
            }, false);

            app.Command("request", config => {
                config.Description = "run request agent";
                config.HelpOption("-? | -h | --help");

                var threadsOp = config.Option<int>("-t", "number of parallel threads to use", CommandOptionType.SingleValue);
                var requestFileOp = config.Option("-f", "path to request tree json file", CommandOptionType.SingleValue);
                var generatorOp = config.Option("-g", "generator name", CommandOptionType.SingleValue);
                var extraArg = config.Argument("-e", "extra options", true); 

                config.OnExecute(async () => {

                    int threads = threadsOp.HasValue() ? threadsOp.ParsedValue : 1;

                    JsonHttpRequestTree requestTree = null; 
                    if (requestFileOp.HasValue() && File.Exists(requestFileOp.Value()))
                    {
                        string json = File.ReadAllText(requestFileOp.Value());
                        requestTree = JsonConvert.DeserializeObject<JsonHttpRequestTree>(json);
                    }

                    if (generatorOp.HasValue())
                    {
                        var loader = new RequestGeneratorPluginLoader();
                        var requestGenerator = loader.Load().First(g => g.Name.Equals(generatorOp.Value(), StringComparison.InvariantCultureIgnoreCase));
                        var pluginOptions = requestGenerator.Options.ToOptionsDictionary(); 

                        // add options
                        for(int i = 0; i < extraArg.Values.Count; i++)
                        {
                            string argument = null;
                            string value = null;
                            if(extraArg.Values[i].StartsWith("--"))
                            {
                                argument = extraArg.Values[i].TrimStart('-');
                                value = extraArg.Values[i + 1];
                            }

                            if(pluginOptions.ContainsKey(argument))
                            {
                                pluginOptions[argument].Value = value; 
                            }
                        }

                        var requests = requestGenerator.Run();

                        requestTree = new JsonHttpRequestTree() { Requests = requests, Description = requestGenerator.Name };
                    }

                    Console.WriteLine(CliResultViews.StartRequestString, generatorOp.HasValue() ? generatorOp.Value() : requestFileOp.Value(), threads);

                    var requestResult = await RunRequestComparisonAsync(threads, requestTree);
                    var grades = requestResult.Grades;
                    var stats = requestResult.Stats;

                    CliResultViews.DrawGrades(grades);
                    CliResultViews.DrawStats(stats);

                    var result = new JsonHttpRequestTree()
                    {
                        Requests = requestResult.Grades.Requests,
                        Description = $"{requestTree.Description} : {DateTime.UtcNow}"
                    };
                    
                    File.WriteAllText("output.json", JsonConvert.SerializeObject(result, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));

                    return 0;
                });
            }, false);

            app.Command("generators", config => {
                config.Description = "Generator plugings";
                config.HelpOption("-? | -h | --help");
                
                config.OnExecute(async () => {
                    var loader = new RequestGeneratorPluginLoader();
                    foreach(var generator in loader.Load())
                    {
                        CliResultViews.DrawGenerator(generator); 
                    }

                    return 0;
                });
            }, false);

            app.Execute(args);
        }

        

        private static async Task<AgentStats> RunSimpleLoadTestAsync(Uri uri, int threads, TimeSpan duration)
        {
            var request = new Request
            {
                Url = uri.AbsoluteUri,
                Method = "get"
            }; 

            IAgentJob job = new HttpClientAgentJob(new HttpClient(), request.ToHttpRequestMessage()); 
            var agent = new SimpleLoadTestAgent(job, request);

            return await agent.Run(threads, duration, new System.Threading.CancellationToken()); 
        }

        private static async Task<AgentResult> RunRequestComparisonAsync(int threads, JsonHttpRequestTree requestTree)
        {
            var agent = new RequestTreeAgent(requestTree);
            return await agent.Run(threads, new System.Threading.CancellationToken());
        }
    }
}
