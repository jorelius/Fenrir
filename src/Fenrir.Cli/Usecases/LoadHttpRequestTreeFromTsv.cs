using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Cli.Usecases
{
    public class LoadHttpRequestTreeFromTsv
    {
        /// <summary>
        /// Build HttpRequestTree from tsv file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public HttpRequestTree Execute(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return Execute(stream);
            }
        }

        public HttpRequestTree Execute(Stream TsvStream)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = "\t",
                IgnoreBlankLines = true
            };

            using (var reader = new StreamReader(TsvStream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<RequestMap>();
                var records = csv.GetRecords<Request>();
                return new HttpRequestTree
                {
                    Requests = records.ToList()
                };
            }
        }
    }
}