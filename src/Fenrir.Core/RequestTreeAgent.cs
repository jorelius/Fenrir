using System;
using System.Threading;
using System.Threading.Tasks;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core
{
    public class RequestTreeAgent
    {
        private readonly JsonHttpRequestTree _requestTree;

        public RequestTreeAgent(JsonHttpRequestTree requestTree)
        {
            _requestTree = requestTree;
        }
        
        public Task<AgentResult> Run(int threads, CancellationToken cancellationToken)
        {
            // traverse the tree 
            

            throw new NotImplementedException();
        }
    }
}