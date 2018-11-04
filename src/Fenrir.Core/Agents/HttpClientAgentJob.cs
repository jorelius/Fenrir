using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Fenrir.Core.Models;

namespace Fenrir.Core.Agents
{
    public class HttpClientAgentJob : IAgentJob
    {
        private readonly int _index;
        private readonly HttpRequestMessage _request;
        private readonly Stopwatch _stopwatch;
        private readonly Stopwatch _localStopwatch;
        private readonly AgentThreadResult _agentThreadResult;
        private readonly HttpClient _httpClient;

        public HttpClientAgentJob(HttpClient httpClient, HttpRequestMessage request)
        {
            _httpClient = httpClient;
            _request = request;
        }

        public HttpClientAgentJob(int index, HttpClient httpClient, HttpRequestMessage request, AgentThreadResult agentThreadResult)
        {
            _index = index;
            _httpClient = httpClient;
            _request = request;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _localStopwatch = new Stopwatch();
            _agentThreadResult = agentThreadResult;
        }

        public async Task<AgentThreadResult> DoWork()
        {
            _localStopwatch.Restart();

            using (var response = await _httpClient.SendAsync(_request))
            {
                var contentStream = await response.Content.ReadAsStreamAsync();
                var length = contentStream.Length + response.Headers.ToString().Length;
                var responseTime = (float)_localStopwatch.ElapsedTicks / Stopwatch.Frequency * 1000;

                var code = (int)response.StatusCode;
                if ((int)response.StatusCode < 400)
                    _agentThreadResult.Add((int)_stopwatch.ElapsedMilliseconds, length, responseTime, _index < 10, code);
                else
                    _agentThreadResult.AddError((int)_stopwatch.ElapsedMilliseconds, responseTime, _index < 10, code);
            }

            return _agentThreadResult;
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