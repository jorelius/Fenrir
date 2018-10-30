using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Extensions
{
    internal static class RequestExtensions
    {
        public static HttpRequestMessage ToHttpRequestMessage(this Request request)
        {
            var method = (HttpMethod)Enum.Parse(typeof(HttpMethod),request.Method, true);
            var message =  new HttpRequestMessage(method, request.Url); 

            if (request?.Payload?.Headers != null && request.Payload.Headers.Count > 0)
            {
                foreach(var header in request.Payload.Headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            if (request?.Payload?.Body != null)
            {
                var data = Encoding.UTF8.GetBytes(request.Payload.Body.ToString());
                message.Content = new ByteArrayContent(data);
            }

            return message;
        }
    }
}