using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fenrir.Core.Generators
{
    public class RequestGeneratorPluginLoader
    {
        private string PluginDirectory { get; set; }

        public RequestGeneratorPluginLoader(string pluginDir = null)
        {
            if (string.IsNullOrEmpty(pluginDir))
            {
                pluginDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins");
            }

            PluginDirectory = pluginDir;
        }

        public IEnumerable<IRequestGenerator> Load()
        {
            var generators = new List<IRequestGenerator>();
            generators.AddRange(LoadGeneratorPlugins());
            return generators;
        }

        private IEnumerable<IRequestGenerator> LoadGeneratorPlugins()
        {
            var generators = new List<IRequestGenerator>();

            if (!Directory.Exists(PluginDirectory))
            {
                Directory.CreateDirectory(PluginDirectory);
            }

            List<string> plugins = new List<string>();

            var pluginDirs = Directory.EnumerateDirectories(PluginDirectory).ToList();
            pluginDirs.Add(PluginDirectory); 
            foreach (var dir in pluginDirs)
            {
                var files = Directory.EnumerateFiles(dir);
                plugins.AddRange(files.Where(f =>
                {
                    var file = new FileInfo(f);
                    if (file.Extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }

                    return false;
                }));
            }

            //loads a new dll to the current AppDomain.
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var plugin in plugins)
            {
                var a = Assembly.LoadFrom(plugin);
                assemblies.Add(a);
            }

            foreach (Assembly a in assemblies)
            {
                try
                {
                    foreach (Type t in a.GetTypes())
                    {
                        if (t?.GetInterface(typeof(IRequestGenerator).FullName) != null)
                        {
                            IRequestGenerator pluginClass = Activator.CreateInstance(t) as IRequestGenerator;
                            generators.Add(pluginClass);
                        }
                    }
                }
                catch (Exception e)
                {
                    // can I get a logging framework? 
                }
            }

            return generators;
        }
    }
}
