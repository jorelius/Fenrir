using PowerArgs;

namespace Fenrir.Cli
{
    [TabCompletion]
    public class RequestArgs
    {
        [ArgDescription("path to request tree file"), ArgExistingFile, ArgShortcut("f")]
        public string RequestFilePath { get; set; }

        [ArgDescription("number of parallel threads to use"), ArgShortcut("t"), DefaultValue(1), ArgRange(1, int.MaxValue)]
        public int Concurrency { get; set; }

        [ArgDescription("path to output file"), ArgShortcut("o")]
        public string OutputFilePath { get; set; }
    }
}