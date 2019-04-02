using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Fenrir.Core.Models.RequestTree;
using Newtonsoft.Json;

namespace Fenrir.Core.Generators
{
    public class SimpleLoadTestGenerator : IRequestGenerator
    {
        public Guid Id => Guid.Parse("158add24-8371-4ff7-999c-e9622df6bf9d");

        public string Name => "Simple Load Test Generator";

        public string Description => "Supports Simple load test scenarios";

        public List<Option> Options { get; set; } = new List<Option> 
        {
            new Option(new OptionDescription("Count", "-1", "request count. If -1 then there is no count limit")),
            new Option(new OptionDescription("Duration", "-1", "duration in second. If -1 then there is no time limit")),
            new Option(new OptionDescription("Url", "", "Single url to load test")),
            new Option(new OptionDescription("Path", "", "Path to request json")),
        };

        private HttpClient client { get; set; }

        public SimpleLoadTestGenerator()
        {
            client = new HttpClient(); 
        }

        public IEnumerable<Request> Run()
        {
            Option countOption = null;
            int count;
            if (!int.TryParse((countOption = Options.FindLast(o => string.Equals(o.Description.Key, "Count"))).Value, out count))
            {
                throw new ArgumentException($"plugin parameter Count is not a valid value: {countOption?.Value}");
            }

            if (count < -1)
            {
                throw new ArgumentException($"plugin parameter Count is not a value grater than or equal to -1: {countOption?.Value}");
            }

            Option durationOption = null;
            int duration;
            if (!int.TryParse((durationOption = Options.FindLast(o => string.Equals(o.Description.Key, "Duration"))).Value, out duration))
            {
                throw new ArgumentException($"plugin parameter Duration is not a valid value: {durationOption?.Value}");
            }

            if (duration < -1)
            {
                throw new ArgumentException($"plugin parameter Duration is not a value grater than or equal to -1: {durationOption?.Value}");
            }

            List<Request> srcRequests = null;

            Option urlOption = null;
            Uri uri;
            if ( Uri.TryCreate((urlOption = Options.FindLast(o => string.Equals(o.Description.Key, "Url"))).Value, UriKind.Absolute, out uri) ) 
            {
                srcRequests = new List<Request>()
                {
                    new Request
                    {
                        Url = uri.AbsoluteUri,
                        Method = "Get"
                    }
                };
            }

            Option pathOption = null;
            string path = (pathOption = Options.FindLast(o => string.Equals(o.Description.Key, "Path"))).Value;

            if ( string.IsNullOrWhiteSpace(path) && File.Exists(path) )
            {
                string json = File.ReadAllText(path);
                JsonHttpRequestTree requestTree = JsonConvert.DeserializeObject<JsonHttpRequestTree>(json);
                srcRequests = requestTree.Requests.ToList();
            }

            if (srcRequests == null)
            {
                throw new ArgumentException($"Uri or Path to request file must be supplied");
            }

            DateTime start = new DateTime(DateTime.Now.Ticks); 
            int i = 0;
            while ( 
                (count == -1 || i < count) && 
                (duration == -1 || (DateTime.Now - start).Seconds <= duration ) )
            {
                if (count != -1)
                {
                    i++;
                }

                yield return ProvideRequest(srcRequests, i);
            }
        }

        private Request ProvideRequest(List<Request> srcRequests, int index)
        {
            return srcRequests[index % srcRequests.Count];
        }
    }
}