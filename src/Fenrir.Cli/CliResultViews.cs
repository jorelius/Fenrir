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
            Console.WriteLine(CliResultViews.StatsResultString,
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
            Console.WriteLine(CliResultViews.GradesResultString,
                            grades.Passed,
                            grades.Failed,
                            grades.Undefined);
        }

        internal const string StartSimpleWithDurationString = @"
Running {0}s test with {2} threads @ {1}";


        internal const string StartRequestString = @"
Running request test with {1} threads @ {0}";

    }
}
