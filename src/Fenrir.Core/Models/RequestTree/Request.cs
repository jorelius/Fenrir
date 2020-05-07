using System.Collections.Generic;

namespace Fenrir.Core.Models.RequestTree
{
    public class Request
    {
        /// <summary>
        /// Requests that should be sequenced
        /// before this one
        /// </summary>
        public Request[] Pre { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public Payload Payload { get; set; }

        /// <summary>
        /// Expected Result for request
        /// </summary>
        public Result ExpectedResult { get; set; }

        /// <summary>
        /// Metadata for request
        /// </summary>
        public Metadata Metadata { get; set; }
    }
}