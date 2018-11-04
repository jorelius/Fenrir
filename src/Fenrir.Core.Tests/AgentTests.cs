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

namespace Fenrir.Core.Tests
{
    public class AgentTests : IClassFixture<WebServiceFixture>
    {
        WebServiceFixture fixture;

        public AgentTests(WebServiceFixture webServiceFixture)
        {
            fixture = webServiceFixture;
        }

        [Fact]
        public async Task TestDefaultResponce()
        {
            var resourceStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Fenrir.Core.Tests.Resources.test-get-20.json");

            JsonHttpRequestTree requestTree;
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                requestTree = JsonConvert.DeserializeObject<JsonHttpRequestTree>(reader.ReadToEnd());
            }

            RequestTreeAgent agent = new RequestTreeAgent(requestTree);
            var result = await agent.Run(1);

            Assert.True(result != null);
            Assert.True(result.StatusCodes[200] == 1);
        }

        [Fact]
        public async Task TestPreRequests()
        {
            var resourceStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Fenrir.Core.Tests.Resources.test-pre-then-get.json");

            JsonHttpRequestTree requestTree;
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                requestTree = JsonConvert.DeserializeObject<JsonHttpRequestTree>(reader.ReadToEnd());
            }

            RequestTreeAgent agent = new RequestTreeAgent(requestTree);
            var result = await agent.Run(1);

            Assert.True(result != null);
            Assert.True(result.StatusCodes[201] == 1);
            Assert.True(result.StatusCodes[200] == 1);
        }
    }
}
