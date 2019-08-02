using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fenrir.Core.Models;

namespace Fenrir.Core
{
    public interface  IAgentJob
    {
        Task<AgentThreadResult> DoWork();
    }
}
