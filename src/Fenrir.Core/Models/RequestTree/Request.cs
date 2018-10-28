using System.Collections.Generic;

namespace Fenrir.Core.Models.RequestTree
{
    public class Request
    {
        public Request[] Pre { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public Payload Payload { get; set; }
        public Result ExpectedResult { get; set; }
    }
}