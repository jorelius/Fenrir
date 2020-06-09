using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Cli.Usecases
{
    public class LoadHttpRequestTreeFromListOfUrls
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

        public HttpRequestTree Execute(Stream UrlListStream)
        {
            return new HttpRequestTree
            {
                Requests = ReadUrls(UrlListStream)
            };
        }

        private IEnumerable<Request> ReadUrls(Stream UrlListStream)
        {
            using (var reader = new StreamReader(UrlListStream))
            {
                while(!reader.EndOfStream)
                {
                    yield return new Request
                    {
                        Url = reader.ReadLine(),
                        Method = "get"
                    };
                }
            }
        }
    }
}