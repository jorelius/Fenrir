using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Fenrir.Core.Models.RequestTree;
using System.Threading.Tasks;
using System.Threading;
using Fenrir.Core.Jobs;
using Fenrir.Core.Extensions;
using Fenrir.Core.Generators;

namespace Fenrir.Core.Tests
{
    public class RequestGeneratorPluginLoaderTests
    {
        [Fact]
        public async Task LoadPluginTest()
        {
            var loader = new RequestGeneratorPluginLoader(Path.GetDirectoryName(Assembly.GetAssembly(typeof(RequestGeneratorPluginLoaderTests)).Location));
            var generators = loader.Load(); 

            Assert.True(generators != null);
            Assert.True(generators.Count() > 0);

            var generator = generators.First();
            generator.Options[0].Value = "42";

            Assert.Equal("42", generator.Options[0].Value); 
        }
    }
}
