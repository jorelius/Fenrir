using System;
using System.Collections.Generic;
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
        internal const string GradesResultString = @"
Grades
    Passed:     {0}
    Failed:     {1}
    Undefined:  {2}
";

        internal const string StartSimpleWithDurationString = @"
Running {0}s test with {2} threads @ {1}";


        internal const string StartRequestString = @"
Running request test with {1} threads @ {0}";

    }
}
