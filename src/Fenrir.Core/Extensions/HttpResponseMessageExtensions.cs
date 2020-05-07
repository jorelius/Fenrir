using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public async static Task<Result> ToResult(this HttpResponseMessage response)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            foreach(var header in response.Headers)
            {
                headers.Add(header.Key, HeaderValueToString(header.Value));
            }

            foreach (var header in response.Content.Headers)
            {
                headers.Add(header.Key, HeaderValueToString(header.Value));
            }

            string body = null;
            if (ShouldEncodeAsBase64(response.Content.Headers.ContentType.MediaType))
            {
                byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                body = Convert.ToBase64String(bytes);
            }
            else
            {
                body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return new Result
            {
                Code = (int)response.StatusCode,
                Payload = new Payload
                {
                    Headers = headers,
                    Body = body
                }
            };
        }

        private static string HeaderValueToString(IEnumerable<string> value)
        {
            string result = null; 
            foreach(var v in value)
            {
                if (result == null)
                {
                    result = v;
                    continue; 
                }

                result = "; " + v; 
            }

            return result;
        }


        private static bool ShouldEncodeAsBase64(string mediaType)
        {
            var sanatized = mediaType.ToLowerInvariant();
            if(sanatized.Contains("json"))
            {
                return false;
            }

            if (sanatized.Contains("xml"))
            {
                return false;
            }

            if (sanatized.Contains("text"))
            {
                return false;
            }

            return true;
        }
    }
}