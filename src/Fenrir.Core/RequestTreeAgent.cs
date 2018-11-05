using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fenrir.Core.Agents;
using Fenrir.Core.Extensions;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core
{
    public class RequestTreeAgent
    {
        private HttpClient _client;
        private readonly JsonHttpRequestTree _requestTree;

        public RequestTreeAgent(JsonHttpRequestTree requestTree)
        {
            _client = new HttpClient();
            _requestTree = requestTree;
        }
        
        public Task<AgentResult> Run(int threads)
        {
            return Run(threads, new CancellationToken()); 
        }

        public async Task<AgentResult> Run(int threads, CancellationToken cancellationToken)
        {
            var flattenedTree = Flatten(_requestTree.Requests);
            var results = new List<AgentThreadResult>(); 

            var sw = new Stopwatch();
            sw.Start();
            
            // flatten the tree and "do the work"
            foreach (var requests in flattenedTree)
            {
                var jobSet = requests.Select(r => {
                    return new HttpClientAgentJob(0, _client, r.ToHttpRequestMessage(), new AgentThreadResult(r.Metadata.Id, r.Metadata.ParentId) {});
                });

                var agentThreadResults = await Task.WhenAll(jobSet.Select(job => job.DoWork()));
                
                results.AddRange(agentThreadResults);
            }

            // compile results
            var combinedThreadResult = new CombinedAgentThreadResult(results, sw.Elapsed);
            var AgentResult = new AgentResult(threads, combinedThreadResult.Elapsed);
            AgentResult.Process(combinedThreadResult);

            return AgentResult;
        }

        private List<Request[]> Flatten(Request[] requests, string ParentId = null)
        {
            var result = new List<Request[]>();
            foreach(var request in requests)
            {
                if (request.Metadata == null)
                {
                    request.Metadata = new Metadata();
                }

                // set id if non is passed in
                if (string.IsNullOrWhiteSpace(request.Metadata.Id))
                {
                    request.Metadata.Id = Guid.NewGuid().ToString();
                }

                if (!string.IsNullOrWhiteSpace(ParentId))
                {
                    request.Metadata.ParentId = ParentId;
                }

                if (request.Pre != null)
                {
                    result.AddRange(Flatten(request.Pre, request.Metadata.Id));
                }

                var single =  new Request[1] { request };
                result.Add(single); 
            }

            return result;
        }
    }
}