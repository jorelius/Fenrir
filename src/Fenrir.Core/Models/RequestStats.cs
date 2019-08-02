using System.Collections.Generic;

namespace Fenrir.Core.Models
{
    public class RequestStats
    {
        public long Bytes { get; private set; }
        public bool isError { get; private set; }
        public float ResponseTime { get; private set; } = 0;
        public int StatusCode { get; private set; } = -1;


        public void Add(long bytes, float responseTime, int statusCode)
        {
            Bytes = bytes;
            ResponseTime = responseTime;
            StatusCode = statusCode;
        }

        public void AddError(float responseTime, int statusCode)
        {
            isError = true;
            ResponseTime = responseTime;
            StatusCode = statusCode;
        }
    }
}