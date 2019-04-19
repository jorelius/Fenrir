using Fenrir.Core.Generators;
using PowerArgs;
using System.Collections.Generic;

namespace Fenrir.Cli
{
    internal class GeneratorNameCompletionSource : SimpleTabCompletionSource
    {
        public GeneratorNameCompletionSource() : base(GeneratorNames())
        {
            this.MinCharsBeforeCyclingBegins = 0;
        }

        private static IEnumerable<string> GeneratorNames()
        {
            var loader = new RequestGeneratorPluginLoader(CliArgs.PluginDir());
            foreach (var generator in loader.Load())
            {
                yield return $"\"{generator.Name}\"";
            }
        }
    }
}