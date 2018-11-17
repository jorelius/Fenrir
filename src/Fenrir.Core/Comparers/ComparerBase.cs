using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Comparers
{
    public abstract class ComparerBase : IResultComparer
    {
        public Grade Compare(Result expected, Result actual)
        {
            var pass = CalculateGrade(expected, actual);
            return new Grade() { Passed = pass, Comment = pass ? "All Good" : "No Good" };
        }

        public bool CalculateGrade(Result expected, Result actual)
        {
            if (expected.Code != actual.Code)
            {
                return false;
            }

            if (expected?.Payload?.Headers != null &&
                !expected.Payload.Headers
                .All(h => actual.Payload.Headers.ContainsKey(h.Key)
                    && string.Equals(actual.Payload.Headers[h.Key], h.Value, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }

            if(expected?.Payload?.Body != null && 
                !CalculateGradeBody(expected.Payload.Body, actual.Payload.Body))
            {
                return false;
            }

            return true;
        }

        public abstract bool CalculateGradeBody(dynamic expected, dynamic actual);
    }
}
