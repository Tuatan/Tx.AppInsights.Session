<Query Kind="Statements">
  <NuGetReference>Tx.ApplicationInsights.Session</NuGetReference>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>Tx.ApplicationInsights.Session</Namespace>
  <Namespace>Tx.ApplicationInsights.Session.TelemetryType</Namespace>
</Query>

var playback = new Playback();

playback.AddApplicationInsightsSession(@"http://localhost:12345/");

playback
	.GetObservable<Envelope<RequestData>>()
	.Select(i => i.Data)
	.Select(i => new { i.Name, i.ResponseCode, i.Duration })
	.Dump();

playback.Start();