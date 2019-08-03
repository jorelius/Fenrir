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
        private readonly HttpRequestMessage _request;
        private readonly Stopwatch _localStopwatch;
        private readonly AgentThreadResult _agentThreadResult;
        private readonly HttpClient _httpClient;

        public HttpClientAgentJob(HttpClient httpClient, HttpRequestMessage request, AgentThreadResult agentThreadResult)
        {      
            _agentThreadResult = agentThreadResult;
            _request = request;
            _httpClient = httpClient;
            _localStopwatch = new Stopwatch();
        }

        public HttpClientAgentJob(HttpClient httpClient, Request request) : this(httpClient, request.ToHttpRequestMessage(), new AgentThreadResult(request))
        {

        }

        public async Task<AgentThreadResult> DoWork()
        {
            _localStopwatch.Restart();
            
            try 
            {
                using (var response = await _httpClient.SendAsync(_request).ConfigureAwait(false))
                {
                    var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var length = contentStream.Length + response.Headers.ToString().Length;
                    
                    _agentThreadResult.AddResult(await response.ToResult().ConfigureAwait(false));

                    var code = (int)response.StatusCode;
                    if ((int)response.StatusCode < 400)
                        _agentThreadResult.Add(length, _localStopwatch.ElapsedMilliseconds, code);
                    else
                        _agentThreadResult.AddError(_localStopwatch.ElapsedMilliseconds, code);
                }
            } 
            catch (HttpRequestException e) // server may fail to respond because of load 
            {
                _agentThreadResult.AddResult(new Result { Code = -1 });
                _agentThreadResult.AddError(_localStopwatch.ElapsedMilliseconds, -1);
            }

            return _agentThreadResult;
        }
    }
}