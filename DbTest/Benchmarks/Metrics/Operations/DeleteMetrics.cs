using DbTest.Benchmarks.Metrics.Common;

namespace DbTest.Benchmarks.Metrics.Operations;

public class DeleteMetrics : SpeedMetrics
{
    public DeleteMetrics(
        DateTimeOffset firstMeasure,
        DateTimeOffset measuredAt,
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        TimeSpan duration,
        long itemsRequiredToDeleteCount,
        int actualDeletedItems) : base(duration, actualDeletedItems)
    {
        MeasuredAt = measuredAt;
        TimeSinceFirstMeasure = measuredAt - firstMeasure;
        TimeSinceFirstMeasureUs = Convert.ToInt64(TimeSinceFirstMeasure.TotalMicroseconds);
        SleepInterval = sleepInterval;
        AlreadyWrittenItems = alreadyWrittenItems;
        Duration = duration;
        DurationUs = Convert.ToInt64(duration.TotalMicroseconds);
        ItemsRequiredToDeleteCount = itemsRequiredToDeleteCount;
        ActualDeletedItems = actualDeletedItems;
    }

    public DateTimeOffset MeasuredAt { get; }
    public TimeSpan TimeSinceFirstMeasure { get; }
    public long TimeSinceFirstMeasureUs { get; }
    public TimeSpan SleepInterval { get; }
    public long AlreadyWrittenItems { get; }
    public TimeSpan Duration { get; }
    public long DurationUs { get; }
    public long ItemsRequiredToDeleteCount { get; }
    public int ActualDeletedItems { get; }
}