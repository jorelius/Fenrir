using PowerArgs;
using System;

namespace Fenrir.Cli
{
    [TabCompletion]
    public class SimpleArgs
    {
        [ArgRequired, ArgDescription("load test url"), ArgShortcut("u"), ArgPosition(1)]
        public Uri Url { get; set; }

        [ArgDescription("number of parallel threads to use"), ArgShortcut("t"), DefaultValue(1), ArgRange(1, int.MaxValue)]
        public int Concurrency { get; set; }

        [ArgDescription("number requests to make"), ArgShortcut("c"), DefaultValue(-1), ArgRange(1, int.MaxValue)]
        public int Count { get; set; }

        [ArgDescription("length of time to run load test"), ArgShortcut("d"), DefaultValue("00:00:30"), ArgIgnore]
        public TimeSpan Duration { get; set; }


    }
}