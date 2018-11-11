using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fenrir.Core.Models;

namespace Fenrir.Core
{
    public interface  IAgentJob
    {
        Task<IAgentJob> InitAsync(int agentIndex, HttpRequestMessage httpRequestMessage, AgentThreadResult agentThreadResult);
        Task<IAgentJob> InitAsync(int index, AgentThreadResult agentThreadResult);
        Task<AgentThreadResult> DoWork();
        
    }
}
