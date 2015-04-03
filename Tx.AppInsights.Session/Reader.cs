namespace Tx.ApplicationInsights.Session
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    internal static class Reader
    {
        public static string GetContent(Stream stream, IDictionary<string, string> headers)
        {
            string content;

            if (!stream.CanRead)
            {
                return string.Empty;
            }

            if (headers != null &&
                headers.ContainsKey("Content-Encoding") &&
                !string.IsNullOrWhiteSpace( headers["Content-Encoding"]) &&
                string.Equals("gzip", headers["Content-Encoding"],
                    StringComparison.InvariantCultureIgnoreCase))
            {
                content = Decompress(stream);
            }
            else
            {
                using (var sr = new StreamReader(stream))
                {
                    content = sr.ReadToEnd();
                }
            }

            return content;
        }

        private static string Decompress(Stream stream)
        {
            using (var compressedzipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                var outputStream = new MemoryStream();
                var block = new byte[1024];
                while (true)
                {
                    int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    outputStream.Write(block, 0, bytesRead);
                }
                compressedzipStream.Close();
                return Encoding.UTF8.GetString(outputStream.ToArray());
            }
        }
    }
}
