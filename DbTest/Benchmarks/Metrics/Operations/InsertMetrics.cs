using DbTest.Benchmarks.Metrics.Common;

namespace DbTest.Benchmarks.Metrics.Operations;

public class InsertMetrics : SpeedMetrics
{
    public InsertMetrics(
        DateTimeOffset firstMeasure,
        DateTimeOffset measuredAt,
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        TimeSpan duration,
        int actualInsertedItems) : base(duration, actualInsertedItems)
    {
        MeasuredAt = measuredAt;
        TimeSinceFirstMeasure = measuredAt - firstMeasure;
        TimeSinceFirstMeasureUs = Convert.ToInt64(TimeSinceFirstMeasure.TotalMicroseconds);
        SleepInterval = sleepInterval;
        AlreadyWrittenItems = alreadyWrittenItems;
        Duration = duration;
        DurationUs = Convert.ToInt64(duration.TotalMicroseconds);
        ActualInsertedItems = actualInsertedItems;
    }

    public DateTimeOffset MeasuredAt { get; }
    public TimeSpan TimeSinceFirstMeasure { get; }
    public long TimeSinceFirstMeasureUs { get; }
    public TimeSpan SleepInterval { get; }
    public long AlreadyWrittenItems { get; }
    public TimeSpan Duration { get; }
    public long DurationUs { get; }
    public int ActualInsertedItems { get; }
}