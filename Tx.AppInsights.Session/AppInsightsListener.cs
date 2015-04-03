namespace Tx.ApplicationInsights.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

    using Newtonsoft.Json.Linq;

    internal static class AppInsightsListener
    {
        public static IObservable<PayloadData> Capture(string uri)
        {
            return Observable.Create<PayloadData>(
                observer =>
                    {
                        var subscription = new CompositeDisposable();

                        var listener = new HttpListenerObservable(uri);

                        listener.Start();

                        subscription.Add(listener
                            .Select(i => i.RequestContent)
                            .Where(i => !string.IsNullOrEmpty(i))
                            .SelectMany(
                                i =>
                                    {
                                        try
                                        {
                                            return PayloadParser.ParseNew(i);
                                        }
                                        catch (Exception e)
                                        {
                                            return Enumerable.Empty<PayloadData>();
                                        }
                                    })
                            .Where(i => i != null)
                            .Subscribe(observer));

                        subscription.Add(listener);

                        return subscription;
                    });
        }
    }


    //public static class Reader
    //{
    //    public static IEnumerable<T> Read<T>(IEnumerable<string> input)
    //    {
    //        var TypeMap = new PartitionableTypeMap();

    //        var typeKey = TypeMap.GetTypeKey(typeof(T));

    //        var transform = TypeMap.GetTransform(typeof(T));

    //        if (string.IsNullOrEmpty(typeKey) || transform == null)
    //        {
    //            throw new NotSupportedException(typeof(T).FullName + " is unsupported type.");
    //        }

    //        return input
    //            .Select(i => PayloadParser.Parse(i))
    //            .Where(i => TypeMap.Comparer.Equals(TypeMap.GetInputKey(i), typeKey))
    //            .Select(i => (T)transform(i));
    //    }
    //}

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
            Envelope<T> result;

            try
            {
                var jsonObject = JObject.Parse(envelope.PayloadJson);

                result = this.ParseEnvelope<T>(envelope, jsonObject);

                result.Data = jsonObject.SelectToken("data.baseData").ToObject<T>();
            }
            catch (Exception e)
            {                
                throw e;
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
                key = (envelope) => null;
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
            if (evt.Type == "Exception")
            {
                return "Exception";
            }

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

    public class PayloadData
    {
        public string PayloadJson { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Type { get; set; }
    }

    public static class PayloadParser
    {
        public static PayloadData Parse(string payloadJson)
        {
            var jsonObject = JObject.Parse(payloadJson);

            var name = ((string)jsonObject.SelectToken("name"));

            var index = name.LastIndexOf('.');

            var timestamp = ((DateTimeOffset)jsonObject.SelectToken("time"));

            var type = index == -1
                    ? name
                    : name.Substring(index + 1);
            Console.WriteLine(type);

            return new PayloadData
            {
                PayloadJson = payloadJson,
                Timestamp = timestamp,
                Type = type,
            };
        }

        public static IEnumerable<PayloadData> ParseNew(string payloadJson)
        {
            if (payloadJson.StartsWith("["))
            {
                return JObject.Parse(payloadJson)
                    .Children()
                    .Select(i => ParseSingle(i, i.ToString()));
            }
            else
            {
                return new[]
                {
                    ParseSingle(JObject.Parse(payloadJson), payloadJson)
                };
            }
        }

        public static PayloadData ParseSingle(JToken jsonObject, string payloadJson)
        {
            var name = ((string)jsonObject.SelectToken("name"));

            var index = name.LastIndexOf('.');

            var timestamp = ((DateTimeOffset)jsonObject.SelectToken("time"));

            var type = index == -1
                    ? name
                    : name.Substring(index + 1);
            Console.WriteLine(type);

            return new PayloadData
            {
                PayloadJson = payloadJson,
                Timestamp = timestamp,
                Type = type,
            };
        }
    }

    public class Envelope<T>
    {
        public IDictionary<string, string> Tags;

        public DateTimeOffset Time;

        public T Data;
    }

    public class Properties
    {
        public string DeveloperMode { get; set; }
    }

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

    public class SessionStateData
    {
        public int Ver { get; set; }
        public string State { get; set; }
    }

    public class PerformanceCounterData
    {
        public int Ver { get; set; }
        public string CategoryName { get; set; }
        public string CounterName { get; set; }
        public string InstanceName { get; set; }
        public double Value { get; set; }
    }

    public class ParsedStack
    {
        public int Level { get; set; }
        public string Method { get; set; }
        public string Assembly { get; set; }
        public string FileName { get; set; }
        public int Line { get; set; }
    }

    public class ExceptionData
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string Message { get; set; }
        public bool HasFullStack { get; set; }
        public List<ParsedStack> ParsedStack { get; set; }
    }

    public class ExceptionEventData
    {
        public int Ver { get; set; }
        public string HandledAt { get; set; }
        public Properties Properties { get; set; }
        public List<ExceptionData> Exceptions { get; set; }
        public string SeverityLevel { get; set; }
    }
}
