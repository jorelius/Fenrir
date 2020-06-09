using CsvHelper;
using CsvHelper.Configuration;
using Fenrir.Core.Models.RequestTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using Xunit;
using Fenrir.Cli.Usecases;

namespace Fenrir.Cli.Tests
{
    public class LoadFromUrlListTests
    {
        private const string url = "http://www.cnet.com";

        [Theory]
        [InlineData("url-list.txt")]
        public void LoadTsvRequests(string resourceName)
        {
            var resourceStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Fenrir.Cli.Tests.Resources.{resourceName}");

            var requestTree = new LoadHttpRequestTreeFromListOfUrls().Execute(resourceStream);

            Request request = requestTree.Requests.FirstOrDefault();

            Assert.Equal(url, request.Url);
        }
    }
}
