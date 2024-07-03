using System;

namespace Helios.Relay
{
    [Flags]
    public enum RelayService
    {
        None        = 0,
        Reach       = 1 << 0,
        Twilio      = 1 << 1,
        Dispatch    = 1 << 2,
        GoogleUA    = 1 << 3,
        CsvExport   = 1 << 4,
        Keen        = 1 << 5,
        Polygon     = 1 << 6,
        Patron      = 1 << 7,
        Eshots      = 1 << 8,
        Dummy       = 1 << 30
    }
}