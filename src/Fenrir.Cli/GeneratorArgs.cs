﻿using PowerArgs;
using System.Collections.Generic;

namespace Fenrir.Cli
{
    public class GeneratorArgs
    {
        [ArgRequired, ArgDescription("generator name"), ArgPosition(1)]
        public string Name { get; set; }

        [ArgDescription("generator arguments"), ArgShortcut("a")]
        public List<string> Arguments { get; set; }

        [ArgDescription("number of parallel threads to use"), ArgShortcut("t"), DefaultValue(1), ArgRange(1, int.MaxValue)]
        public int Concurrency { get; set; }

        [ArgDescription("path to output file"), ArgShortcut("o")]
        public string OutputFilePath { get; set; }
    }
}