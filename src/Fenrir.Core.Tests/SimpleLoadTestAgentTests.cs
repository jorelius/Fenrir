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
    public class SimpleLoadTestAgentTests : IClassFixture<WebServiceFixture>
    {
        WebServiceFixture fixture;

        public SimpleLoadTestAgentTests(WebServiceFixture webServiceFixture)
        {
            fixture = webServiceFixture;
        }

        [Fact]
        public async Task CountLoadTest()
        {
            var url = @"http://localhost:5000/api/Test/TestGet?numberOfResponses=20";

            var generator = new SimpleLoadTestGenerator();

            var countOption = generator.Options.First(o => o.Description.Key == "Count");
            countOption.Value = "10";

            var urlOption = generator.Options.First(o => o.Description.Key == "Url");
            urlOption.Value = url;

            var requests = generator.Run().ToList();
            
            Assert.Equal(10, requests.Count());
            Assert.True(requests[5].Url == url);
        }

        [Fact]
        public async Task DurationLoadTest()
        {
            var url = @"http://localhost:5000/api/Test/TestGet?numberOfResponses=20";

            var generator = new SimpleLoadTestGenerator();

            var durationOption = generator.Options.First(o => o.Description.Key == "Duration");
            durationOption.Value = "3"; // 3 seconds

            var urlOption = generator.Options.First(o => o.Description.Key == "Url");
            urlOption.Value = url;

            var requests = generator.Run();

            DateTime start = new DateTime(DateTime.Now.Ticks);

            foreach(var request in requests)
            {
                Assert.Equal(url, request.Url);
            }

            int duration = (DateTime.Now - start).Seconds;

            Assert.InRange(duration, 2, 4);
        }
    }
}
