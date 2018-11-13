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
        public async Task CountLoadTest()
        {
            var request = new Request() { Method = "Get", Url = @"http://localhost:5000/api/Test/TestGet?numberOfResponses=20" };

            var job = new HttpClientAgentJob(new System.Net.Http.HttpClient(), request.ToHttpRequestMessage()); 
            Agent agent = new Agent(job, request); 

            var stats = await agent.Run(10, new CancellationToken());

            Assert.True(stats != null);
            Assert.True(stats.StatusCodes[200] == 10);
        }

        [Fact]
        public async Task DurationLoadTest()
        {
            var request = new Request() { Method = "Get", Url = @"http://localhost:5000/api/Test/TestGet?numberOfResponses=20" };

            var job = new HttpClientAgentJob(new System.Net.Http.HttpClient(), request.ToHttpRequestMessage()); 
            Agent agent = new Agent(job, request); 

            var stats = await agent.Run(2, TimeSpan.FromSeconds(2), new CancellationToken());

            Assert.True(stats != null);
            Assert.True(stats.Count > 1);
            Assert.True(stats.StatusCodes[200] > 0);
            Assert.True(stats.Elapsed > TimeSpan.FromSeconds(.5));
        }
    }
}
