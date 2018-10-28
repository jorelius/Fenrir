using System.Collections.Generic;

namespace Fenrir.Core.Models.RequestTree
{
    public class Payload
    {
        public Dictionary<string, string> Headers { get; set; }
        public dynamic Body { get; set; }
    }
}