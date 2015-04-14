namespace Tx.ApplicationInsights.Session
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;

    internal class HttpListenerObservable : IObservable<HttpRequestData>, IDisposable
    {
        private readonly HttpListener listener;
        private IObservable<HttpRequestData> stream;

        public HttpListenerObservable(string url)
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(url);
        }

        public void Start()
        {
            if (this.stream != null)
            {
                this.Stop();
            }

            if (!this.listener.IsListening)
            {
                this.listener.Start();
            }

            this.stream = Observable
                .Create<HttpRequestData>((Func<IObserver<HttpRequestData>, IDisposable>)this.CreateStream)
                .Repeat()
                .Publish()
                .RefCount();
        }

        public void Stop()
        {
            this.Dispose();
        }

        public IDisposable Subscribe(IObserver<HttpRequestData> observer)
        {
            if (this.stream == null)
            {
                throw new InvalidOperationException("Call HttpListenerObservable.Start before subscribing to the stream");
            }

            return this.stream
                .SubscribeSafe(observer);
        }

        public void Dispose()
        {
            if (this.listener != null && this.listener.IsListening)
            {
                this.listener.Stop();
                this.listener.Close();

                this.stream = null;
            }
        }

        private IDisposable CreateStream(IObserver<HttpRequestData> observer)
        {
            return Task.Factory.FromAsync(
                (Func<AsyncCallback, object, IAsyncResult>)this.listener.BeginGetContext,
                (Func<IAsyncResult, HttpListenerContext>)this.listener.EndGetContext,
                null)
                .ToObservable()
                .Select(ToHttpSession)
                .SubscribeSafe(observer);
        }

        private static HttpRequestData ToHttpSession(HttpListenerContext context)
        {
            var result = new HttpRequestData();

            try
            {
                var request = context.Request;

                result.RequestHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var item in request.Headers.AllKeys)
                {
                    result.RequestHeaders[item] = request.Headers[item];
                }

                result.RequestContent = Reader.GetContent(context.Request.InputStream, result.RequestHeaders);
            }
            catch (Exception e)
            {
                // Add EventSource based tracing
            }
            finally 
            {
                context.Response.Close();
            }

            return result;
        }
    }
}
