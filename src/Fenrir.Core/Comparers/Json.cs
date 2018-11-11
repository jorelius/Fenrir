using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

namespace Fenrir.Core.Comparers
{
    public class Json : IResultComparer
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

            var jdp = new JsonDiffPatch();
            var left = JToken.Parse(expected.Payload.Body.ToString());
            var right = JToken.Parse(actual.Payload.Body.ToString());

            JToken patch = jdp.Diff(left, right);

            if (patch != null && patch.HasValues)
            {
                return false;
            }

            return true;
        }
    }
}
