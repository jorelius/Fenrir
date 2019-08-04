using System;
using System.Collections.Generic;

namespace Fenrir.Core.Models
{
    public class RequestStats
    {
        public long Bytes { get; private set; }
        public bool isError { get; private set; }
        public float ResponseTime { get; private set; } = 0;
        public int StatusCode { get; private set; } = -1;

        public DateTime StartTime { get; private set; } = DateTime.Now;


        public void Add(long bytes, float responseTime, DateTime startTime, int statusCode)
        {
            Bytes = bytes;
            ResponseTime = responseTime;
            StartTime = startTime;
            StatusCode = statusCode;
        }

        public void AddError(float responseTime, DateTime startTime, int statusCode)
        {
            isError = true;
            ResponseTime = responseTime;
            StartTime = startTime;
            StatusCode = statusCode;
        }
    }
}