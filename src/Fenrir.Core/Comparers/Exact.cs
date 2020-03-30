using System;
using System.Linq;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Comparers
{
    public class Exact : ComparerBase
    {
        public override ComparerResult CalculateGradeBody(dynamic expected, dynamic actual)
        {
            if (String.Equals(expected, actual))
            {
                return new ComparerResult { Result = true, Cause = "matches exactly" };
            }

            return new ComparerResult { Result = false, Cause = "does not match exactly" };
        }
    }
}