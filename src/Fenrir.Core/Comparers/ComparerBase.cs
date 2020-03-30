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
            ComparerResult result = CalculateGrade(expected, actual);
            return new Grade() { Passed = result.Result, Comment = result.Cause };
        }

        public ComparerResult CalculateGrade(Result expected, Result actual)
        {
            if (expected.Code != actual.Code)
            {
                return new ComparerResult { Result = false, Cause = "Http Code Does not Match expected value" };
            }

            // validating against expected header values
            // can be disabled if expected header values are set to null
            if (expected?.Payload?.Headers != null &&
                !expected.Payload.Headers
                .All(h => actual.Payload.Headers.ContainsKey(h.Key)
                    && string.Equals(actual.Payload.Headers[h.Key], h.Value, StringComparison.InvariantCultureIgnoreCase)))
            {
                return new ComparerResult { Result = false, Cause = "Http result headers do not Match expected values" };;
            }

            ComparerResult result = null; 
            if(expected?.Payload?.Body != null && 
                !(result = CalculateGradeBody(expected.Payload.Body, actual.Payload.Body)).Result)
            {
                return result;
            }

            return new ComparerResult { Result = true, Cause = result?.Cause };
        }

        public abstract ComparerResult CalculateGradeBody(dynamic expected, dynamic actual);
    }
}
