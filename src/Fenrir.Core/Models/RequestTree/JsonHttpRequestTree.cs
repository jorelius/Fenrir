using System.Collections.Generic;

namespace Fenrir.Core.Models.RequestTree
{
    public class JsonHttpRequestTree
    {
        public string Description { get; set; }
        public IEnumerable<Request> Requests { get; set; }
    }
}