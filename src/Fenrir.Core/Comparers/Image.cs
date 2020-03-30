using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Comparers
{
    public class Image : ComparerBase
    {
        public override ComparerResult CalculateGradeBody(dynamic expected, dynamic actual)
        {
            if (expected == actual)
            {
                return new ComparerResult { Result = true, Cause = "Image bytes[] match" };
            }

            return new ComparerResult { Result = false, Cause = "Image bytes[] do not match" };
        }
    }
}