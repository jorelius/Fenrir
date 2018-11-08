namespace Fenrir.Core.Models.RequestTree
{
    public class Metadata
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public Result Result { get; internal set; }
    }
}