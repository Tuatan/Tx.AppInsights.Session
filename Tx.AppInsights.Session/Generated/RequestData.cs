namespace Tx.ApplicationInsights.Session.TelemetryType
{
    public class RequestData
    {
        public int Ver { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string Duration { get; set; }
        public bool Success { get; set; }
        public string ResponseCode { get; set; }
        public string Url { get; set; }
        public string HttpMethod { get; set; }
        public Properties Properties { get; set; }
    }
}