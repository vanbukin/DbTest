using System.Data;

namespace DbTest.Benchmarks.Options;

public class InsertOptions
{
    public InsertOptions(
        IdGenerator idGenerator,
        int minBatchSize, 
        int maxBatchSize, 
        DateTime startDate,
        TimeSpan step,
        IsolationLevel isolationLevel,
        TimeSpan pause)
    {
        IdGenerator = idGenerator;
        MinBatchSize = minBatchSize;
        MaxBatchSize = maxBatchSize;
        StartDate = startDate;
        Step = step;
        IsolationLevel = isolationLevel;
        Pause = pause;
    }

    public IdGenerator IdGenerator { get; }
    public int MinBatchSize { get; }
    public int MaxBatchSize { get; }
    public DateTime StartDate { get; }
    public TimeSpan Step { get; }
    public IsolationLevel IsolationLevel { get; }
    public TimeSpan Pause { get; }
}