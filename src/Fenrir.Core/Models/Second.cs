using System.Collections.Generic;

namespace Fenrir.Core.Models
{
    public class Second
    {
        public long Count { get; set; }
        public long Bytes { get; private set; }
        public long Errors { get; private set; }
        public int Elapsed { get; }
        public List<float> ResponseTimes { get; private set; }
        public List<int> StatusCodes { get; private set; }

        public Second(int elapsed)
        {
            Elapsed = elapsed;
            ResponseTimes = new List<float>();
            StatusCodes = new List<int>();
        }

        internal void ClearResponseTimes()
        {
            ResponseTimes = new List<float>();
        }

        internal void ClearStatusCodes()
        {
            StatusCodes = new List<int>();
        }

        public void Add(long bytes, float responseTime, bool trackResponseTime, int statusCode)
        {
            Count++;
            Bytes += bytes;

            if (trackResponseTime)
                ResponseTimes.Add(responseTime);

            StatusCodes.Add(statusCode);
        }

        public void AddError(float responseTime, bool trackResponseTime, int statusCode)
        {
            Count++;
            Errors++;

            if (trackResponseTime)
                ResponseTimes.Add(responseTime);

            StatusCodes.Add(statusCode);
        }

        public void AddMerged(Second second)
        {
            Count += second.Count;
            Bytes += second.Bytes;
            Errors += second.Errors;
        }
}
}