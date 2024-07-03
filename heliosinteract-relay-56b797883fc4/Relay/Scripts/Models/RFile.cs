namespace Helios.Relay
{   
    public class RFile : RModel
    {
        public override string endpoint => "files";
        public override string logType => "ReachFile";
        public RFileUploadOptions uploadOptions;

        public string path { get; set; }
        public string name { get; set; }
        public string mimeType { get; set; }
        public long length { get; set; }

        public string access { get; set; }
        public string url { get; set; }
        public string activationId { get; set; }
    }
}
