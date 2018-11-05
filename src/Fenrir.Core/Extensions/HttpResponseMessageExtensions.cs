using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Extensions
{
    internal static class HttpResponseMessageExtensions
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

            return new Result
            {
                Code = (int)response.StatusCode,
                Payload = new Payload
                {
                    Headers = headers,
                    Body = await response.Content.ReadAsStringAsync()
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
    }
}