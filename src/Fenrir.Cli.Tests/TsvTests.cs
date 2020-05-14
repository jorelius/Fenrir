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

namespace Fenrir.Cli.Tests
{
    public class TsvTests
    {
        private const string url = "http://www.cnet.com";

        [Theory]
        [InlineData("full-header.tsv")]
        [InlineData("minimum-fields.tsv")]
        public void LoadTsvRequests(string resourceName)
        {
            var resourceStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Fenrir.Cli.Tests.Resources.{resourceName}");

            var requestTree = CliArgs.ReadTsv(resourceStream);

            Request request = requestTree.Requests.FirstOrDefault();

            Assert.Equal(url, request.Url);
        }

        [Fact]
        public void WriteTsvRequest()
        {
            var requests = new List<Request>
            {
                new Request
                {
                    Url = url,
                    Method = "get"
                }
            };

            CsvConfiguration config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = "\t",
                IgnoreBlankLines = true,
                
            };

            using (var writer = new StreamWriter($"{Path.GetTempPath()}/test.tsv"))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<RequestMap>();
                csv.WriteRecords(requests);
            }
        }
    }
}
