using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core.Comparers
{
    public class Image : IResultComparer
    {
        public Grade Compare(Result expected, Result actual)
        {
            var pass = expected.Payload.Body == actual.Payload.Body;
            return new Grade() { Passed = pass, Comment = pass ? "All Good" : "No Good" };
        }
    }
}