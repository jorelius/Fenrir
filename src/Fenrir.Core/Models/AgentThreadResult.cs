using System;
using System.Collections.Generic;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Models
{
    public class AgentThreadResult
    {
        /// <summary>
        /// Request Stats 
        /// </summary>
        public RequestStats Stats { get; }

        /// <summary>
        /// Request processed
        /// </summary>
        public Request Request { get; private set; }

        public AgentThreadResult(Request request)
        {
            Stats = new RequestStats();
            Request = request;
        }

        public void Add(long bytes, float responsetime, DateTime startTime, int statusCode)
        {
            Stats.Add(bytes, responsetime, startTime, statusCode);
        }

        public void AddError(float responsetime, DateTime startTime, int statusCode)
        {
            Stats.AddError(responsetime, startTime, statusCode);
        }

        public void AddResult(Result result)
        {
            if (Request?.Metadata != null)
            {
                Request.Metadata.Result = result;
            }
        }
    }
}