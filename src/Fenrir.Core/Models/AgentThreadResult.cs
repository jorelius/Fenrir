using System;
using System.Collections.Generic;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Models
{
    public class AgentThreadResult
    {
        public Dictionary<int, Second> Seconds { get; }
        public Request Request { get; private set; }
        public string Id { get; } = null;
        public string ParentId { get; } = null;

        public AgentThreadResult()
        {
            Seconds = new Dictionary<int, Second>();
        }

        public AgentThreadResult(Request request) : this()
        {
            Request = request;
        }

        public void Add(int elapsed, long bytes, float responsetime, bool trackResponseTime, int statusCode)
        {
            GetItem(elapsed).Add(bytes, responsetime, trackResponseTime, statusCode);
        }

        public void AddError(int elapsed, float responsetime, bool trackResponseTime, int statusCode)
        {
            GetItem(elapsed).AddError(responsetime, trackResponseTime, statusCode);
        }

        private Second GetItem(int elapsed)
        {
            if (Seconds.ContainsKey(elapsed))
                return Seconds[elapsed];

            var second = new Second(elapsed);
            Seconds.Add(elapsed, second);
            return second;
        }

        public void AddResult(Result result)
        {
            Request.Metadata.Result = result;
        }
    }
}