namespace Tx.ApplicationInsights.Session
{
    using System;
    using System.Collections.Generic;

    public class Envelope<T>
    {
        public IDictionary<string, string> Tags;

        public DateTimeOffset Time;

        public T Data;
    }
}