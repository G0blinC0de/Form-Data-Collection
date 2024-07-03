namespace Helios.Relay
{
    public class RExperience : RModel
    {
        public override string endpoint => "experiences";
        public override string logType => "ReachExperience";
        public int duration { get; set; }
        public string type { get; set; }
        public string guestId { get; set; }
        public bool publish { get; set; }
        public int copyCount { get; set; }
        public string printerStatus { get; set; }
        public bool emailOptIn { get; set; }
        public string fileId { get; set; }
        public string activationId { get; set; }
        public RFile[] files { get; set; }
    }
}