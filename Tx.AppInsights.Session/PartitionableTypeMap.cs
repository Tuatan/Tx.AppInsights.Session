namespace Tx.ApplicationInsights.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;

    using Newtonsoft.Json.Linq;

    using Tx.ApplicationInsights.Session.TelemetryType;

    internal class PartitionableTypeMap : IPartitionableTypeMap<PayloadData, string>
    {
        private readonly Dictionary<Type, Func<PayloadData, object>> map = new Dictionary<Type, Func<PayloadData, object>>();
        private readonly Dictionary<Type, string> map2 = new Dictionary<Type, string>();

        public PartitionableTypeMap()
        {
            this.map2.Add(typeof(Envelope<RequestData>), "Request");
            this.map2.Add(typeof(Envelope<SessionStateData>), "SessionState");
            this.map2.Add(typeof(Envelope<PerformanceCounterData>), "PerformanceCounter");
            this.map2.Add(typeof(Envelope<ExceptionEventData>), "Exception");

            this.map.Add(typeof(Envelope<RequestData>), this.Parse<RequestData>);
            this.map.Add(typeof(Envelope<SessionStateData>), this.Parse<SessionStateData>);
            this.map.Add(typeof(Envelope<PerformanceCounterData>), this.Parse<PerformanceCounterData>);
            this.map.Add(typeof(Envelope<ExceptionEventData>), this.Parse<ExceptionEventData>);
        }

        public Envelope<T> ParseEnvelope<T>(PayloadData envelope, JObject jsonObject)
        {
            var result = new Envelope<T>
            {
                Time = envelope.Timestamp
            };

            result.Tags = jsonObject
                .SelectToken("tags")
                .Children()
                .Select(i =>
                    {
                        var index1 = i.Path.IndexOf('\'');
                        var index2 = i.Path.LastIndexOf('\'');

                        return new
                        {
                            Key = i.Path.Substring(index1 + 1, index2 - index1 - 1),
                            Value = i.Children().First().ToString(),
                        };
                    })
                .ToDictionary(i => i.Key, i => i.Value);

            return result;
        }

        public Envelope<RequestData> ParseRequest(PayloadData envelope)
        {
            var jsonObject = JObject.Parse(envelope.PayloadJson);

            var result = this.ParseEnvelope<RequestData>(envelope, jsonObject);

            result.Data = jsonObject.SelectToken("data.baseData").ToObject<RequestData>();

            return result;
        }

        public Envelope<T> Parse<T>(PayloadData envelope)
        {
            Envelope<T> result = null;

            try
            {
                var jsonObject = JObject.Parse(envelope.PayloadJson);

                result = this.ParseEnvelope<T>(envelope, jsonObject);

                result.Data = jsonObject.SelectToken("data.baseData").ToObject<T>();
            }
            catch (Exception e)
            {                
                // Add EventSource based tracing
            }

            return result;
        }

        internal Dictionary<Type, Func<PayloadData, object>> Deserializers
        {
            get
            {
                return this.map;
            }
        }

        public Func<PayloadData, object> GetTransform(Type outputType)
        {
            Func<PayloadData, object> key;

            if (!this.map.TryGetValue(outputType, out key))
            {
                key = envelope => null;
            }

            return key;
        }

        public Func<PayloadData, DateTimeOffset> TimeFunction
        {
            get
            {
                return e => e.Timestamp;
            }
        }

        public string GetTypeKey(Type outputType)
        {
            string key;

            this.map2.TryGetValue(outputType, out key);

            return key;
        }

        public string GetInputKey(PayloadData evt)
        {
            return evt.Type;
        }

        public IEqualityComparer<string> Comparer
        {
            get
            {
                return StringComparer.OrdinalIgnoreCase;
            }
        }
    }
}