using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Fenrir.Core.Extensions;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;


namespace Fenrir.Core.Jobs
{
    public class HttpClientAgentJob : IAgentJob
    {
        private readonly Request _request;
        private readonly Stopwatch _localStopwatch;
        private readonly HttpClient _httpClient;


        public HttpClientAgentJob(HttpClient httpClient, Request request) 
        {
            _request = request;
            _httpClient = httpClient;
            _localStopwatch = new Stopwatch();
        }

        public async Task<AgentThreadResult> DoWork()
        {
            AgentThreadResult threadResult = new AgentThreadResult(_request);
            HttpRequestMessage httpRequest = _request.ToHttpRequestMessage();

            DateTime startTime = DateTime.Now.ToLocalTime();
            _localStopwatch.Restart();
            
            try 
            {
                using (var response = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false))
                {
                    var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var length = contentStream.Length + response.Headers.ToString().Length;

                    threadResult.AddResult(await response.ToResult().ConfigureAwait(false));

                    var code = (int)response.StatusCode;
                    if ((int)response.StatusCode < 400)
                        threadResult.Add(length, _localStopwatch.ElapsedMilliseconds, startTime, code);
                    else
                        threadResult.AddError(_localStopwatch.ElapsedMilliseconds, startTime, code);
                }
            } 
            catch (HttpRequestException e) // server may fail to respond because of load 
            {
                threadResult.AddResult(new Result { Code = -1 });
                threadResult.AddError(_localStopwatch.ElapsedMilliseconds, startTime, -1);
            }
            catch (Exception e) // anything else
            {
                threadResult.AddResult(new Result { Code = -2 });
                threadResult.AddError(_localStopwatch.ElapsedMilliseconds, startTime, -2);
            }

            return threadResult;
        }
    }
}