using Fenrir.Core.Comparers;
using Fenrir.Core.Models.RequestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fenrir.Core.Models
{
    public class AgentRequestGrade
    {
        public IEnumerable<Request> Requests; 

        public void Process(IEnumerable<AgentThreadResult> results)
        {
            var comparer = new Json();
            foreach(var result in results)
            {
                Grade grade = null;
                if (result.Request.ExpectedResult != null)
                {
                    grade = comparer.Compare(result.Request.ExpectedResult, result.Request.Metadata.Result);
                }

                result.Request.Metadata.Grade = grade; 
            }

            // only add root level request with no parents
            var rootRequests = results.Where(r => string.IsNullOrWhiteSpace(r.Request.Metadata.ParentId)).Select(r => r.Request);
            Requests = rootRequests; 
        }

    }
}
