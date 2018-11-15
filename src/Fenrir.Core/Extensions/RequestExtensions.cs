using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Fenrir.Core.Models.RequestTree;

[assembly: InternalsVisibleTo("Fenrir.Core.Tests")]
namespace Fenrir.Core.Extensions
{
    public static class RequestExtensions
    {
        public static HttpRequestMessage ToHttpRequestMessage(this Request request)
        {
            HttpRequestMessage message =  new HttpRequestMessage(new HttpMethod(request.Method), request.Url);

            if (request?.Payload?.Body != null)
            {
                dynamic data = Encoding.UTF8.GetBytes(request.Payload.Body.ToString());
                message.Content = new ByteArrayContent(data);
            }

            if (request?.Payload?.Headers != null && request.Payload.Headers.Count > 0)
            {
                foreach(System.Collections.Generic.KeyValuePair<string, string> header in request.Payload.Headers)
                {
                    if (header.Key.StartsWith("content", StringComparison.InvariantCultureIgnoreCase))
                    {
                        message?.Content?.Headers.Add(header.Key, header.Value);
                    }
                    else
                    {
                        message.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            return message;
        }
    }
}