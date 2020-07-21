using Fenrir.Core.Comparers;
using Fenrir.Core.Models.RequestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Fenrir.Core.Models
{
    public class AgentRequestGrade
    {
        public IEnumerable<Request> Requests;

        public int Passed { get; private set; }
        public int Failed { get; private set; }
        public int Undefined { get; private set; }

        public void Process(IEnumerable<AgentThreadResult> results)
        {
            var comparerFactory = new ResultComparerFactory(); 
            foreach(var result in results)
            {
                Grade grade = null;
                if (result.Request.ExpectedResult != null)
                {
                    string contentType;
                    
                    if (!result.Request.Metadata.Result.Payload.Headers.TryGetValue("Content-Type", out contentType))
                    {
                        contentType = "text/plain";
                    } 

                    var header = MediaTypeHeaderValue.Parse(contentType); 
                    IResultComparer comparer = comparerFactory.GetByContentType(header.MediaType);

                    grade = comparer.Compare(result.Request.ExpectedResult, result.Request.Metadata.Result);
                }

                result.Request.Metadata.Grade = grade;

                if (grade == null)
                {
                    Undefined++;
                }
                else if (grade.Passed)
                {
                    Passed++;
                }
                else 
                {
                    Failed++;
                }
            }

            // only add root level request with no parents
            var rootRequests = results.Where(r => string.IsNullOrWhiteSpace(r.Request.Metadata.ParentId)).Select(r => r.Request);
            Requests = rootRequests; 
        }

    }
}
