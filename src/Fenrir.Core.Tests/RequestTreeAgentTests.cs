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
    public class RequestTreeAgentTests : IClassFixture<WebServiceFixture>
    {
        WebServiceFixture fixture;

        public RequestTreeAgentTests(WebServiceFixture webServiceFixture)
        {
            fixture = webServiceFixture;
        }

        [Fact]
        public async Task OnlyGradeCode()
        {
            var resourceStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Fenrir.Core.Tests.Resources.test-get-code.json");

            HttpRequestTree requestTree;
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                requestTree = JsonConvert.DeserializeObject<HttpRequestTree>(reader.ReadToEnd());
            }

            RequestTreeAgent agent = new RequestTreeAgent(requestTree);
            var result = await agent.Run(1);

            Assert.True(result != null);
            Assert.True(result.Stats.StatusCodes[200] == 1);
            Assert.True(result.Grades.Requests.ToList().Count == 1);
        }

        [Fact]
        public async Task RequestTreeAgent()
        {
            var resourceStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Fenrir.Core.Tests.Resources.test-get-20.json");

            HttpRequestTree requestTree;
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                requestTree = JsonConvert.DeserializeObject<HttpRequestTree>(reader.ReadToEnd());
            }

            RequestTreeAgent agent = new RequestTreeAgent(requestTree);
            var result = await agent.Run(1);

            Assert.True(result != null);
            Assert.True(result.Stats.StatusCodes[200] == 1);
            Assert.True(result.Grades.Requests.ToList().Count == 1); 
        }

        [Fact]
        public async Task PreRequests()
        {
            var resourceStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Fenrir.Core.Tests.Resources.test-pre-then-get.json");

            HttpRequestTree requestTree;
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                requestTree = JsonConvert.DeserializeObject<HttpRequestTree>(reader.ReadToEnd());
            }

            RequestTreeAgent agent = new RequestTreeAgent(requestTree);
            var result = await agent.Run(1);

            Assert.True(result != null);
            Assert.True(result.Stats.StatusCodes[201] == 1);
            Assert.True(result.Stats.StatusCodes[200] == 1);
        }
    }
}
