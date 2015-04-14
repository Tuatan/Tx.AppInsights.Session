namespace Tx.ApplicationInsights.Session
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    internal static class AppInsightsListener
    {
        public static IObservable<PayloadData> Capture(string uri)
        {
            return Observable.Create<PayloadData>(
                observer =>
                {
                    var disposable = new CompositeDisposable();

                    var listener = new HttpListenerObservable(uri);

                    listener.Start();

                    disposable.Add(listener
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

                    disposable.Add(listener);

                    return disposable;
                });
        }

        //private static IDisposable ParseIncommingRequests(IObservable<string> source, IObserver<PayloadData> sink)
        //{
        //    return source
        //        .SelectMany(
        //            payloadJson =>
        //            {
        //                try
        //                {
        //                    return PayloadParser.ParseNew(payloadJson);
        //                }
        //                catch (Exception e)
        //                {
        //                    // Add EventSource based tracing
        //                    return Enumerable.Empty<PayloadData>();
        //                }
        //            })
        //        .Where(payloadData => payloadData != null)
        //        .Subscribe(sink);
        //}

        //private static IDisposable Resubmit(IObservable<string> source, IObserver<PayloadData> sink)
        //{
        //    var disposable = new CompositeDisposable();

        //    var subscription = source
        //        .Subscribe(i => { });

        //    disposable.Add(subscription);

        //    return disposable;
        //}

        //public static IObservable<PayloadData> Capture2(
        //    string uri, 
        //    Func<IObservable<string>, IObserver<PayloadData>, IDisposable>[] subscribers)
        //{
        //    return Observable.Create<PayloadData>(
        //        observer =>
        //        {
        //            var disposable = new CompositeDisposable();

        //            var listener = new HttpListenerObservable(uri);

        //            disposable.Add(listener);

        //            var subject = new Subject<string>();

        //            listener.Start();

        //            var subscription = listener
        //                .Select(i => i.RequestContent)
        //                .Where(i => !string.IsNullOrEmpty(i))
        //                .SubscribeSafe(subject);

        //            disposable.Add(subscription);

        //            foreach (var subscriber in subscribers)
        //            {
        //                disposable.Add(subscriber(subject, observer));
        //            }

        //            disposable.Add(subject);

        //            return disposable;
        //        });
        //}


        //private static void Empty(string item)
        //{
        //}
    }
}
