using System;
using System.Linq;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Comparers
{
    public class Exact : ComparerBase
    {
        public override bool CalculateGradeBody(dynamic expected, dynamic actual)
        {
            return String.Equals(expected, actual);
        }
    }
}