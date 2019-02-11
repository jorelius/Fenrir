using Fenrir.Core.Generators;
using Fenrir.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fenrir.Cli
{
    internal static class CliResultViews
    {
        internal const string StatsResultString = @"
{0} requests in {1:0.##}s
    Requests/sec:   {2:0}
    Bandwidth:      {3:0} mbit
    Errors:         {4:0}

Latency
    Median:         {5:0.000} ms
    StdDev:         {6:0.000} ms
    Min:            {7:0.000} ms
    Max:            {8:0.000} ms

{9}
";

        internal static void DrawStats(AgentStats stats)
        {
            Console.WriteLine(StatsResultString,
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
        }

        internal static void DrawStatusCodes(AgentStats stats)
        {
            var codes = stats.StatusCodes.OrderByDescending((kv) => kv.Value);

            Console.WriteLine();
            Console.WriteLine("Http Codes");

            int numCodes = 0; 
            foreach(var code in codes)
            {
                // return top 3
                if (numCodes >= 3) break; 
                
                Console.WriteLine("    {0}s:           {1}", code.Key, code.Value);
                numCodes ++; 
            }

            if (codes.Count() > 3)
            {
                var remainingCodes = 
                    codes
                        .Skip(3)
                        .Select(kv => kv.Value)
                        .Aggregate((total, next) => total + next);


                Console.WriteLine("    other:           {1}", remainingCodes);
            }
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


        internal const string GradesResultString = @"
Grades
    Passed:     {0}
    Failed:     {1}
    Undefined:  {2}
";

        internal static void DrawGrades(AgentRequestGrade grades)
        {
            Console.WriteLine(GradesResultString,
                            grades.Passed,
                            grades.Failed,
                            grades.Undefined);
        }

        internal const string StartSimpleWithDurationString = @"
Running {0}s test with {2} threads @ {1}";


        internal const string StartRequestString = @"
Running request test with {1} threads @ {0}";


        internal const string GeneratorResultString = @"
    Name:           {0}
    Description:    {1}
    Options:
";

        internal const string GeneratorOptionResultString = @"
        Name:           {0}
        Description:    {1}
        DefaultValue:   {2}
        IsRequired:     {3}
";

        internal static void DrawGenerator(IRequestGenerator generator)
        {
            Console.WriteLine(GeneratorResultString, 
                generator.Name, 
                generator.Description); 

            foreach (var option in generator.Options)
            {
                Console.WriteLine(GeneratorOptionResultString, 
                    option.Description.Key, 
                    option.Description.Description, 
                    option.Description.DefaultValue, 
                    option.Description.IsRequired);
            }
        }

    }
}
