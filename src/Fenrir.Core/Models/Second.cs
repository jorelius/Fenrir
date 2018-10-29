using System.Collections.Generic;

namespace Fenrir.Core.Models
{
    public class Second
    {
        public long Count { get; set; }
        public long Bytes { get; private set; }
        public long Errors { get; private set; }
        public int Elapsed { get; }
        public List<(int StatusCode, float ResponseTime)> ResponseTimes { get; private set; }

        public int StatusCode { get; set; }

        public Second(int elapsed)
        {
            Elapsed = elapsed;
            ResponseTimes = new List<(int StatusCode, float ResponseTime)>();
        }

        internal void ClearResponseTimes()
        {
            ResponseTimes = new List<(int StatusCode, float ResponseTime)>();
        }

        public void Add(long bytes, float responseTime, bool trackResponseTime, int statusCode)
        {
            Count++;
            Bytes += bytes;

            if (trackResponseTime)
                ResponseTimes.Add((statusCode, responseTime));

            StatusCode = statusCode;
        }

        public void AddError(float responseTime, bool trackResponseTime, int statusCode)
        {
            Count++;
            Errors++;

            if (trackResponseTime)
                ResponseTimes.Add((statusCode, responseTime));
        }

        public void AddMerged(Second second)
        {
            Count += second.Count;
            Bytes += second.Bytes;
            Errors += second.Errors;
        }
}
}