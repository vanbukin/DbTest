using DbTest.Benchmarks.Metrics.Common;

namespace DbTest.Benchmarks.Metrics.Operations;

public class SelectMetrics : SpeedMetrics
{
    public SelectMetrics(
        DateTimeOffset firstMeasure,
        DateTimeOffset measuredAt,
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        TimeSpan duration,
        long itemsRequiredToSelectCount,
        long actualSelectedItems) : base(duration, actualSelectedItems)
    {
        MeasuredAt = measuredAt;
        TimeSinceFirstMeasure = measuredAt - firstMeasure;
        TimeSinceFirstMeasureUs = Convert.ToInt64(TimeSinceFirstMeasure.TotalMicroseconds);
        SleepInterval = sleepInterval;
        AlreadyWrittenItems = alreadyWrittenItems;
        Duration = duration;
        DurationUs = Convert.ToInt64(duration.TotalMicroseconds);
        ItemsRequiredToSelectCount = itemsRequiredToSelectCount;
        ActualSelectedItems = actualSelectedItems;
    }

    public DateTimeOffset MeasuredAt { get; }
    public TimeSpan TimeSinceFirstMeasure { get; }
    public long TimeSinceFirstMeasureUs { get; }
    public TimeSpan SleepInterval { get; }
    public long AlreadyWrittenItems { get; }
    public TimeSpan Duration { get; }
    public long DurationUs { get; }
    public long ItemsRequiredToSelectCount { get; }
    public long ActualSelectedItems { get; }
}