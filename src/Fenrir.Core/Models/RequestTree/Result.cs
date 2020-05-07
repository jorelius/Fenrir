namespace Fenrir.Core.Models.RequestTree
{
    public class Result
    {
        public string Url { get; set; }
        public int Code { get; set; }
        public Payload Payload { get; set; }
    }
}