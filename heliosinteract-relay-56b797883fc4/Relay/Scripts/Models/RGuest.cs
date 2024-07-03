namespace Helios.Relay
{
    using System;

    public class RGuest : RModel
    {
        public override string endpoint => "guests";
        public override string logType => "ReachGuest";
        public string name { get; set; }
        public string email { get; set; }
        public string badge { get; set; }
        public int age { get; set; }
        public DateTime birthdate { get; set; }
        public string gender { get; set; }
        public string[] phones { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postal { get; set; }
        public string country { get; set; }
        public bool marketing { get; set; }
    }
}