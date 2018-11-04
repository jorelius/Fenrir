using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Diagnostics;

namespace Fenrir.Core.Tests
{
    public class WebServiceFixture
    {
        CancellationTokenSource TokenSource;
        Task WebhostTask;

        public WebServiceFixture()
        {
            TokenSource = new CancellationTokenSource();
            var Webhost = BuildWebHost(new string[0]);
            WebhostTask = Webhost.RunAsync(TokenSource.Token);

            Console.WriteLine("Webhost status: " + WebhostTask.Status);

            SpinWait.SpinUntil(() => { return WebhostTask.Status != TaskStatus.Running; });

            Console.WriteLine("Webhost status: " + WebhostTask.Status);
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<WebServiceFixtureStartup>()
                .Build();
        }
    }
}
