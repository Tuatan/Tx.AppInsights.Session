namespace Tx.ApplicationInsights.Session
{
    using System.Collections.Generic;

    internal class HttpRequestData
    {
        public IDictionary<string, string> RequestHeaders { get; set; }

        public string RequestContent { get; set; }
    }
}
