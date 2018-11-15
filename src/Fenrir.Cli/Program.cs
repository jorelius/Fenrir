using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fenrir.Core;
using Fenrir.Core.Extensions;
using Fenrir.Core.Jobs;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;
using Microsoft.Extensions.CommandLineUtils;

namespace Fenrir.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Command("simple", config => {
                config.Description = "run simple load test agent";
                config.HelpOption("-? | -h | --help");
                var threadsOp = config.Option("-t", "number of parallel threads to use", CommandOptionType.SingleValue);
                var durationOp = config.Option("-d", "length of time", CommandOptionType.SingleValue);
                var url = config.Argument("url", "load test url", false);
                config.OnExecute(async () => {
                    Uri uri = null;
                    if (string.IsNullOrWhiteSpace(url.Value) || !Uri.TryCreate(url.Value, UriKind.Absolute, out uri))
                    {
                        Console.WriteLine($"{url.Value} is not a valid url");
                        return 1;
                    }

                    int threads = 1;
                    if (threadsOp.HasValue() && !int.TryParse(threadsOp.Value(), out threads))
                    {
                        Console.WriteLine($"{threadsOp.Value()} is not a valid for number of threads");
                        return 1;
                    }

                    TimeSpan duration = TimeSpan.FromSeconds(30);
                    if (durationOp.HasValue() && !TimeSpan.TryParse(durationOp.Value(), out duration))
                    {
                        Console.WriteLine($"{durationOp.Value()} is not a valid timespan");
                        return 1;
                    }

                    Console.WriteLine(CliResultViews.StartRunWithDurationString, duration.TotalSeconds, uri, threads);

                    var stats = await RunSimpleLoadTestAsync(uri, threads, duration);

                    Console.WriteLine(CliResultViews.ResultString,
                        stats.Count,
                        stats.Elapsed.TotalSeconds,
                        stats.RequestsPerSecond,
                        stats.Bandwidth,
                        stats.Errors,
                        stats.Median,
                        stats.StdDev,
                        stats.Min,
                        stats.Max,
                        GetAsciiHistogram(stats));
                    
                    return 0; 
                });
            });
            app.Command("request", config => {
                config.Description = "run request agent";
            });
            app.HelpOption("-? | -h | --help");
            app.Execute(args);
        }

        private static string GetAsciiHistogram(AgentStats stats)
        {
            if (stats.Histogram.Length == 0)
                return string.Empty;

            const string filled = "█";
            const string empty = " ";
            var histogramText = new string[7];
            var max = stats.Histogram.Max();

            foreach (var t in stats.Histogram)
            {
                for (var j = 0; j < histogramText.Length; j++)
                {
                    histogramText[j] += t > max / histogramText.Length * (histogramText.Length - j - 1) ? filled : empty;
                }
            }

            var text = string.Join("\r\n", histogramText);
            var minText = string.Format("{0:0.000} ms ", stats.Min);
            var maxText = string.Format(" {0:0.000} ms", stats.Max);
            text += "\r\n" + minText + new string('=', stats.Histogram.Length - minText.Length - maxText.Length) + maxText;
            return text;
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
    }
}
