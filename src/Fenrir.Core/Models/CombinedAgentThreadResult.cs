using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Fenrir.Core.Models
{
    public class CombinedAgentThreadResult
    {
        public Dictionary<int, Second> Seconds { get; private set; }
        public List<List<float>> ResponseTimes { get; private set; }
        public List<List<int>> StatusCodes { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public CombinedAgentThreadResult(IEnumerable<AgentThreadResult> results, TimeSpan elapsed)
        {
            Seconds = new Dictionary<int, Second>();
            ResponseTimes = new List<List<float>>();
            StatusCodes = new List<List<int>>();
            Elapsed = elapsed;

            foreach (var result in results)
            {
                foreach (var second in result.Seconds)
                {
                    ResponseTimes.Add(second.Value.ResponseTimes);
                    second.Value.ClearResponseTimes();

                    StatusCodes.Add(second.Value.StatusCodes);
                    second.Value.ClearStatusCodes();
                    
                    if (Seconds.ContainsKey(second.Key))
                        Seconds[second.Key].AddMerged(second.Value);
                    else
                        Seconds.Add(second.Key, second.Value);
                }
            }
        }
    }
}