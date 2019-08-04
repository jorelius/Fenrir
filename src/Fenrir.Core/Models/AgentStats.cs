using System;
using System.Collections.Generic;
using System.Linq;
using Fenrir.Core.Extensions;

namespace Fenrir.Core.Models
{
    public class AgentStats
    {
        public AgentStats(int threads)
        {
            Threads = threads;
            Histogram = new int[0];
            StatusCodes = new Dictionary<int, int>();
        }

        public int Threads { get; }
        public TimeSpan Elapsed { get; private set; }

        public long Count { get; private set; }
        public long Errors { get; private set; }
        public double RequestsPerSecond { get; private set; }
        public double BytesPrSecond { get; private set; }
        
        public Dictionary<int, int> StatusCodes { get; }

        public double Median { get; private set; }
        public double StdDev { get; private set; }
        public double Min { get; private set; }
        public double Max { get; private set; }
        public int[] Histogram { get; private set; }

        public DateTime FirstRequestTime { get; private set; }
        public DateTime LastRequestTime { get; private set; }
        public int[] TimeSeries { get; private set; }

        public double Bandwidth => Math.Round(BytesPrSecond * 8 / 1024 / 1024, MidpointRounding.AwayFromZero);

        public void Process(CombinedAgentThreadResult atResult)
        {
            Elapsed = atResult.Elapsed;
            var items = atResult.RequestStats.DefaultIfEmpty(new RequestStats()).ToList();
            Count = atResult.Count;
            Errors = atResult.Errors;
            RequestsPerSecond = Count / (Elapsed.TotalMilliseconds / 1000);
            BytesPrSecond = items.Sum(s => s.Bytes) / (Elapsed.TotalMilliseconds / 1000);

            foreach (var statusCode in atResult.StatusCodes)
            {
               if (StatusCodes.ContainsKey(statusCode))
                   StatusCodes[statusCode] += 1;
               else
                   StatusCodes.Add(statusCode, 1);
            }

            var responseTimes = atResult.ResponseTimes.ToArray();
            
            if (!responseTimes.Any())
                return;

            Array.Sort(responseTimes);
            Median = responseTimes.GetMedian();
            StdDev = responseTimes.GetStdDev();
            Min = responseTimes.First();
            Max = responseTimes.Last();
            Histogram = GenerateHistogram(responseTimes);

            atResult.StartTimes.Sort();
            FirstRequestTime = atResult.StartTimes.First();
            LastRequestTime = atResult.StartTimes.Last();
            TimeSeries = GenerateTimeSeries(atResult.StartTimes);
        }

        private int[] GenerateHistogram(float[] responeTimes)
        {
            var splits = 80;
            var result = new int[splits];

            if (responeTimes == null || responeTimes.Length < 2)
                return result;

            var max = responeTimes.Last();
            var min = responeTimes.First();
            var divider = (max - min) / splits;
            var step = min;
            var y = 0;

            for (var i = 0; i < splits; i++)
            {
                var count = 0;
                var stepMax = step + divider;

                if (i + 1 == splits)
                    stepMax = float.MaxValue;

                while (y < responeTimes.Length && responeTimes[y] < stepMax)
                {
                    y++;
                    count++;
                }

                result[i] = count;
                step += divider;
            }

            return result;
        }

        private int[] GenerateTimeSeries(List<DateTime> startTimes)
        {
            var splits = 80;
            var result = new int[splits];

            if (startTimes == null || startTimes.Count < 2)
                return result;

            var max = startTimes.Last();
            var min = startTimes.First();
            var divider = new TimeSpan((max - min).Ticks / splits);
            var step = min;
            var y = 0;

            for (var i = 0; i < splits; i++)
            {
                var count = 0;
                var stepMax = step + divider;

                if (i + 1 == splits)
                    stepMax = DateTime.Now;

                while (y < startTimes.Count && startTimes[y] < stepMax)
                {
                    y++;
                    count++;
                }

                result[i] = count;
                step += divider;
            }

            return result;
        }
    }
}