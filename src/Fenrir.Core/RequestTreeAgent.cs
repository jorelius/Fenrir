using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fenrir.Core.Jobs;
using Fenrir.Core.Extensions;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;
using System.Threading.Tasks.Dataflow;
using System.Net;
using Fenrir.Core.Comparers;

namespace Fenrir.Core
{
    public class RequestTreeAgent
    {
        private readonly HttpClient _client;
        private readonly HttpRequestTree _requestTree;
        private readonly ResultComparerFactory _comparerFactory;

        public RequestTreeAgent(HttpRequestTree requestTree, ResultComparerFactory comparerFactory = null)
        {
            ServicePointManager.ServerCertificateValidationCallback = (message, certificate2, arg3, arg4) => true;
            _client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            });

            _requestTree = requestTree;
            _comparerFactory = comparerFactory;
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
                if (cancellationToken.IsCancellationRequested)
                {
                    break; 
                }

                // manage the number of threads with TPL Dataflow
                var throttler = new TransformBlock<IAgentJob, AgentThreadResult>(
                    async job => await job.DoWork().ConfigureAwait(false),
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = threads, CancellationToken = cancellationToken }
                );

                var buffer = new BufferBlock<AgentThreadResult>(); 
                throttler.LinkTo(buffer);

                var jobSet = requests.Select(r => {
                    return new HttpClientAgentJob(_client, r);
                }); 

                foreach(var job in jobSet)
                {
                    throttler.Post(job); 
                }

                throttler.Complete(); 
                await throttler.Completion.ConfigureAwait(false); 

                IList<AgentThreadResult> processed; 
                buffer.TryReceiveAll(out processed); 

                results.AddRange(processed);
            }

            // compile results
            var combinedThreadResult = new CombinedAgentThreadResult(results, sw.Elapsed);
            var statsResult = new AgentStats(threads);
            statsResult.Process(combinedThreadResult);

            var gradsResult = new AgentRequestGrade(_comparerFactory);
            gradsResult.Process(results); 

            return new AgentResult { Stats = statsResult, Grades = gradsResult };
        }

        /// <summary>
        /// Flatten request tree such that requests at the same level can run in parallel but 
        /// sequenced requests (a.k.a. pre requests) run before their parent
        /// </summary>
        /// <param name="requests">current set of request to process</param>
        /// <param name="ParentId">Id of parent request if any</param>
        private IEnumerable<Request[]> Flatten(IEnumerable<Request> requests, string ParentId = null)
        {
            var curRequestRun = new List<Request>();
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
                    // add current request run to result and 
                    // create new run
                    if (curRequestRun.Count > 0)
                    {
                        yield return curRequestRun.ToArray();
                        curRequestRun = new List<Request>();
                    }

                    // add higher levels in the request tree
                    foreach(Request[] resultSet in Flatten(request.Pre, request.Metadata.Id))
                    {
                        yield return resultSet;
                    }
                }

                curRequestRun.Add(request);
            }

            // add request run if any
            if (curRequestRun.Count > 0)
            {
                yield return curRequestRun.ToArray();
            }
        }
    }
}