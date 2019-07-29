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
        private readonly int _index;
        private readonly HttpRequestMessage _request;
        private readonly Stopwatch _stopwatch;
        private readonly Stopwatch _localStopwatch;
        private readonly AgentThreadResult _agentThreadResult;
        private readonly HttpClient _httpClient;


        public HttpClientAgentJob(HttpClient httpClient)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _localStopwatch = new Stopwatch();
            _httpClient = httpClient;
        }

        public HttpClientAgentJob(HttpClient httpClient, HttpRequestMessage request) : this(httpClient)
        {
            _request = request;
        }

        public HttpClientAgentJob(int index, HttpClient httpClient, HttpRequestMessage request, AgentThreadResult agentThreadResult) : this(httpClient, request)
        {
            _index = index;          
            _agentThreadResult = agentThreadResult;
        }

        public async Task<AgentThreadResult> DoWork()
        {
            _localStopwatch.Restart();
            var responseTime = (float)_localStopwatch.ElapsedTicks / Stopwatch.Frequency * 1000;
            try 
            {
                using (var response = await _httpClient.SendAsync(_request).ConfigureAwait(false))
                {
                    var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var length = contentStream.Length + response.Headers.ToString().Length;
                    
                    _agentThreadResult.AddResult(await response.ToResult().ConfigureAwait(false));

                    var code = (int)response.StatusCode;
                    if ((int)response.StatusCode < 400)
                        _agentThreadResult.Add((int)_stopwatch.ElapsedMilliseconds, length, responseTime, _index < 10, code);
                    else
                        _agentThreadResult.AddError((int)_stopwatch.ElapsedMilliseconds, responseTime, _index < 10, code);
                }
            } 
            catch (HttpRequestException e) // server may fail to respond because of load 
            {
                _agentThreadResult.AddResult(new Result { Code = -1 });
                _agentThreadResult.AddError((int)_stopwatch.ElapsedMilliseconds, responseTime, _index < 10, -1);
            }

            return _agentThreadResult;
        }

        public Task<IAgentJob> InitAsync(int index, HttpRequestMessage request, AgentThreadResult agentThreadResult)
        {
            return Task.FromResult<IAgentJob>(new HttpClientAgentJob(index, _httpClient, request, agentThreadResult));
        }

        public Task<IAgentJob> InitAsync(int index, AgentThreadResult agentThreadResult)
        {
            return Task.FromResult<IAgentJob>(new HttpClientAgentJob(index, _httpClient, _request, agentThreadResult));
        }

        public IAgentJob Init(int index, AgentThreadResult agentThreadResult)
        {
            return new HttpClientAgentJob(index, _httpClient, _request, agentThreadResult);
        }
    }
}