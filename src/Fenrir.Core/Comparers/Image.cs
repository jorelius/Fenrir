using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Comparers
{
    public class Image : ComparerBase
    {
        public override bool CalculateGradeBody(dynamic expected, dynamic actual)
        {
            return expected == actual;
        }
    }
}