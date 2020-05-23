using System;
using System.Linq;
using Fenrir.Core.Generators;

namespace Fenrir.Cli.Usecases
{
    /// <summary>
    /// Load Request Generator from plugin directory and 
    /// apply plugin arguments
    /// </summary>
    public class LoadGenerator
    {
        public IRequestGenerator Execute(GeneratorArgs args, string pluginDirectory)
        {
            var loader = new RequestGeneratorPluginLoader(pluginDirectory);
            IRequestGenerator requestGenerator = loader.Load().First(g => g.Name.Equals(args.Name, StringComparison.InvariantCultureIgnoreCase));

            // add options
            if (args.Arguments != null && args.Arguments.Count > 0)
            {
                for (int i = 0; i < args.Arguments.Count; i++)
                {
                    string argument = null;
                    string value = null;
                    if (args.Arguments[i].StartsWith("#"))
                    {
                        argument = args.Arguments[i].TrimStart('#');
                        value = args.Arguments[i + 1];
                    }

                    int index = -1;
                    if (!string.IsNullOrWhiteSpace(argument)
                        && (index = requestGenerator.Options.FindLastIndex(o => o.Description.Key.Equals(argument))) > -1)
                    {
                        requestGenerator.Options[index].Value = value;
                    }
                }
            }

            return requestGenerator;
        }
    }
}