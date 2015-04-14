namespace Tx.ApplicationInsights.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json.Linq;

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

            return new PayloadData
                       {
                           PayloadJson = payloadJson,
                           Timestamp = timestamp,
                           Type = type,
                       };
        }
    }
}