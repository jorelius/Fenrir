using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Fenrir.Core;
using Fenrir.Core.Extensions;
using Fenrir.Core.Generators;
using Fenrir.Core.Jobs;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using PowerArgs;

namespace Fenrir.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            CliArgs parsed = null;
            try
            {
                Console.WriteLine();
                Args.InvokeAction<CliArgs>(args);
            }
            catch (ArgException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<CliArgs>());
            }

            // exit if help is requested
            if (parsed == null || parsed.Help)
            {
                return;
            }
        }
    }
}
