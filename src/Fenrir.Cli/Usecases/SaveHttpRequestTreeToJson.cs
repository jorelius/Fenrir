using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Cli.Usecases
{
    public class SaveHttpRequestTreeToJson
    {
        public async Task Execute(HttpRequestTree requestTree, string outputFile)
        {
            using (var stream = new FileStream(outputFile, FileMode.CreateNew))
            {
                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true
                };

                await JsonSerializer.SerializeAsync<HttpRequestTree>(stream, requestTree, options);
            }
        }
    }
}