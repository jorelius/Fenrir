using System;
using System.Threading.Tasks;
using Fenrir.Core.Models;

namespace Fenrir.Core
{
    public interface  IAgentJob
    {
        Task<IAgentJob> Init(int index, AgentThreadResult agentThreadResult);
        Task DoWork();
        AgentThreadResult GetResults();
    }
}
