using System.Diagnostics;
using System.Diagnostics.Metrics;


namespace SigNozTest
{
    public class Instrumentation : IDisposable
    {
        internal const string ActivitySourceName = "Examples.AspNetCore";
        internal const string MeterName = "Examples.AspNetCore";
        private readonly Meter meter;

        public Instrumentation()
        {
            string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
            ActivitySource = new ActivitySource(ActivitySourceName, version);
            meter = new Meter(MeterName, version);
            FreezingDaysCounter = meter.CreateCounter<long>("weather.days.freezing", "The number of days where the temperature is below freezing");
        }

        public ActivitySource ActivitySource { get; }

        public Counter<long> FreezingDaysCounter { get; }



        public void Dispose()
        {
            ActivitySource.Dispose();
            meter.Dispose();
        }
    }
}
