namespace DbTest.Benchmarks.Metrics.Operations;

public class VacuumMetrics
{
    public VacuumMetrics(
        DateTimeOffset firstMeasure,
        DateTimeOffset measuredAt,
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        long alreadyWrittenItemsBeforeVacuum,
        TimeSpan duration)
    {
        MeasuredAt = measuredAt;
        TimeSinceFirstMeasure = measuredAt - firstMeasure;
        TimeSinceFirstMeasureUs = Convert.ToInt64(TimeSinceFirstMeasure.TotalMicroseconds);
        SleepInterval = sleepInterval;
        AlreadyWrittenItems = alreadyWrittenItems;
        AlreadyWrittenItemsBeforeVacuum = alreadyWrittenItemsBeforeVacuum;
        Duration = duration;
        DurationUs = Convert.ToInt64(duration.TotalMicroseconds);
    }

    public DateTimeOffset MeasuredAt { get; }
    public TimeSpan TimeSinceFirstMeasure { get; }
    public long TimeSinceFirstMeasureUs { get; }
    public TimeSpan SleepInterval { get; }
    public long AlreadyWrittenItems { get; }
    public long AlreadyWrittenItemsBeforeVacuum { get; }
    public TimeSpan Duration { get; }
    public long DurationUs { get; }
}