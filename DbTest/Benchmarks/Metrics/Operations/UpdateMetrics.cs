using DbTest.Benchmarks.Metrics.Common;

namespace DbTest.Benchmarks.Metrics.Operations;

public class UpdateMetrics : SpeedMetrics
{
    public UpdateMetrics(
        DateTimeOffset firstMeasure,
        DateTimeOffset measuredAt,
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        TimeSpan duration,
        long offset,
        long itemsRequiredToUpdateCount,
        int actualUpdatedItems) : base(duration, actualUpdatedItems)
    {
        MeasuredAt = measuredAt;
        TimeSinceFirstMeasure = measuredAt - firstMeasure;
        TimeSinceFirstMeasureUs = Convert.ToInt64(TimeSinceFirstMeasure.TotalMicroseconds);
        SleepInterval = sleepInterval;
        AlreadyWrittenItems = alreadyWrittenItems;
        Duration = duration;
        DurationUs = Convert.ToInt64(duration.TotalMicroseconds);
        Offset = offset;
        ItemsRequiredToUpdateCount = itemsRequiredToUpdateCount;
        ActualUpdatedItems = actualUpdatedItems;
    }

    public DateTimeOffset MeasuredAt { get; }
    public TimeSpan TimeSinceFirstMeasure { get; }
    public long TimeSinceFirstMeasureUs { get; }
    public TimeSpan SleepInterval { get; }
    public long AlreadyWrittenItems { get; }
    public TimeSpan Duration { get; }
    public long DurationUs { get; }
    public long Offset { get; }
    public long ItemsRequiredToUpdateCount { get; }
    public int ActualUpdatedItems { get; }
}