namespace System.Reactive
{
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    using Tx.ApplicationInsights.Session;

    public static class PlaybackExtensions
    {
        public static void AddApplicationInsightsSession(
            this IPlaybackConfiguration playback,
            string uri)
        {
            if (playback == null)
            {
                throw new ArgumentNullException("playback");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                throw new ArgumentException("uri parameter is not valid Uri");
            }

            playback.AddInput(
                () => AppInsightsListener.Capture(uri),
                typeof(PartitionableTypeMap));
        }

        private static void AddApplicationInsightsSession(
            this IPlaybackConfiguration playback,
            string uri,
            string routeUri)
        {
            if (playback == null)
            {
                throw new ArgumentNullException("playback");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (routeUri == null)
            {
                throw new ArgumentNullException("routeUri");
            }

            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                throw new ArgumentException("uri parameter is not valid Uri");
            }

            if (!Uri.IsWellFormedUriString(routeUri, UriKind.Absolute))
            {
                throw new ArgumentException("routeUri parameter is not valid Uri");
            }

            playback.AddInput(
                () => CreateObservable(uri, routeUri),
                typeof(PartitionableTypeMap));
        }

        private static IObservable<PayloadData> CreateObservable(string uri, string routeUri)
        {
            //return AppInsightsListener.Capture(uri);

            return Observable.Create<PayloadData>(
                observer =>
                {
                    var disposable = new CompositeDisposable();

                    var source = AppInsightsListener.Capture(uri);

                    var subject = new Subject<PayloadData>();

                    disposable.Add(subject);

                    disposable.Add(source.Subscribe(subject));

                    disposable.Add(subject.Subscribe(observer));

                    disposable.Add(subject.Subscribe());

                    return disposable;
                });
        }

        public static void AddApplicationInsightsSession(
            this IPlaybackConfiguration playback,
            string uri,
            IObserver<string> additionalObserver)
        {
            if (playback == null)
            {
                throw new ArgumentNullException("playback");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (additionalObserver == null)
            {
                throw new ArgumentNullException("additionalObserver");
            }

            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                throw new ArgumentException("uri parameter is not valid Uri");
            }

            playback.AddInput(
                () => CreateObservable2(uri, additionalObserver),
                typeof(PartitionableTypeMap));
        }

        public static void AddApplicationInsightsSession(
            this IPlaybackConfiguration playback,
            string uri,
            Action<string> additionalHandler)
        {
            if (playback == null)
            {
                throw new ArgumentNullException("playback");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (additionalHandler == null)
            {
                throw new ArgumentNullException("additionalHandler");
            }

            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                throw new ArgumentException("uri parameter is not valid Uri");
            }

            playback.AddApplicationInsightsSession(
                uri, 
                Observer.Create(additionalHandler));
        }

        private static IObservable<PayloadData> CreateObservable2(string uri, IObserver<string> additionalObserver)
        {
            return Observable.Create<PayloadData>(
                observer =>
                {
                    var disposable = new CompositeDisposable();

                    var source = AppInsightsListener.Capture(uri);

                    var subject = new Subject<PayloadData>();

                    disposable.Add(subject);

                    disposable.Add(source.SubscribeSafe(subject));

                    disposable.Add(subject.SubscribeSafe(observer));

                    disposable.Add(subject
                        .Select(i => i.PayloadJson)
                        .SubscribeSafe(additionalObserver));

                    return disposable;
                });
        }

    }
}
