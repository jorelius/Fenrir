using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Fenrir.Core.Models
{
    public class CombinedAgentThreadResult
    {
        public List<RequestStats> RequestStats { get; private set; }
        public List<float> ResponseTimes { get; private set; }
        public List<int> StatusCodes { get; private set; }

        public int Count { get; private set; }
        public int Errors { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public CombinedAgentThreadResult(IEnumerable<AgentThreadResult> results, TimeSpan elapsed)
        {
            RequestStats = new List<RequestStats>();
            ResponseTimes = new List<float>();
            StatusCodes = new List<int>();
            Elapsed = elapsed;

            foreach (var result in results)
            {
                ResponseTimes.Add(result.Stats.ResponseTime);
                StatusCodes.Add(result.Stats.StatusCode);
                RequestStats.Add(result.Stats);

                if (result.Stats.isError)
                {
                    Errors++;
                }

                Count++;
            }
        }
    }
}