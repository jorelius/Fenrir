using System.IO;
using System.Text.Json;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Cli.Usecases
{
    public class LoadHttpRequestTreeFromJson
    {
        /// <summary>
        /// Build HttpRequestTree from json file
        /// </summary>
        /// <param name="path"></param>
        public HttpRequestTree Execute(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<HttpRequestTree>(json);
        }
    }
}