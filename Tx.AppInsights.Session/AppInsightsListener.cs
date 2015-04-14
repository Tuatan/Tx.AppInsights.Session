namespace Tx.ApplicationInsights.Session
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

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
                                            // Add EventSource based tracing
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
}
