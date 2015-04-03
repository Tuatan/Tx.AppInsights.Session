namespace System.Reactive
{
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
    }
}
