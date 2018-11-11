using System;
using System.Linq;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Comparers
{
    public class Exact : IResultComparer
    {
        public Grade Compare(Result expected, Result actual)
        {
            var pass = CalculateGrade(expected, actual); 
            return new Grade() { Passed = pass, Comment = pass ? "All Good" : "No Good" };
        }

        private bool CalculateGrade(Result expected, Result actual)
        {
            if (expected.Code != actual.Code)
            {
                return false;
            }

            if (!expected.Payload.Headers
                .All(h => actual.Payload.Headers.ContainsKey(h.Key) 
                    && string.Equals(actual.Payload.Headers[h.Key], h.Value, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }

            if (!String.Equals(expected.Payload.Body, actual.Payload.Body))
            {
                return false; 
            }

            return true;
        }
    }
}