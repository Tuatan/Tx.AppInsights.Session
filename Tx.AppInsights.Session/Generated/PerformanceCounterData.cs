namespace Tx.ApplicationInsights.Session.TelemetryType
{
    public class PerformanceCounterData
    {
        public int Ver { get; set; }
        public string CategoryName { get; set; }
        public string CounterName { get; set; }
        public string InstanceName { get; set; }
        public double Value { get; set; }
    }
}