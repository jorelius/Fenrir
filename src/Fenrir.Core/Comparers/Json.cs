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
    public class Json : ComparerBase
    {
        public override ComparerResult CalculateGradeBody(dynamic expected, dynamic actual)
        {
            var jdp = new JsonDiffPatch();
            var left = JToken.Parse(expected.ToString());
            var right = JToken.Parse(actual.ToString());

            JToken patch = jdp.Diff(left, right);

            if (patch != null && patch.HasValues)
            {
                return new ComparerResult { Result = false, Cause = patch.ToString() };
            }

            return new ComparerResult { Result = true, Cause = "Json Matches" };
        }
    }
}
