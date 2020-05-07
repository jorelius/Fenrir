using System.Collections.Generic;

namespace Fenrir.Core.Models.RequestTree
{
    public class Metadata
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public Result Result { get; internal set; }
        public Grade Grade { get; internal set; }

        public Dictionary<string, string> Additional { get; private set; } = new Dictionary<string, string>();
    }
}