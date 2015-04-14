namespace Tx.ApplicationInsights.Session
{
    using System;

    public class PayloadData
    {
        public string PayloadJson { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Type { get; set; }
    }
}