namespace DbTest.Benchmarks.Metrics.Common;

public class SpeedMetrics
{
    protected SpeedMetrics(TimeSpan duration, decimal items)
    {
        if (duration > TimeSpan.Zero && items > 0)
        {
            decimal ticks = duration.Ticks;
            var ticksPerItem = ticks / items;
            decimal ticksPerSecond = TimeSpan.TicksPerSecond;
            RatePerSecond = ticksPerSecond / ticksPerItem;
            var millionItemsTicks = (long)(ticksPerItem * 1_000_000m);
            MillionItemsDuration = TimeSpan.FromTicks(millionItemsTicks);
        }
    }

    public decimal RatePerSecond { get; }
    public TimeSpan MillionItemsDuration { get; }
}