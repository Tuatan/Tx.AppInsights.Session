namespace Tx.ApplicationInsights.Session.TelemetryType
{
    using System.Collections.Generic;

    public class ExceptionEventData
    {
        public int Ver { get; set; }
        public string HandledAt { get; set; }
        public Properties Properties { get; set; }
        public List<ExceptionData> Exceptions { get; set; }
        public string SeverityLevel { get; set; }
    }
}