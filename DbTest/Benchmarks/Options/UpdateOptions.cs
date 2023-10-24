using System.Data;

namespace DbTest.Benchmarks.Options;

public class UpdateOptions
{
    public UpdateOptions(
        long minItemsToUpdate,
        long maxItemsToUpdates,
        long minItemsToSkip,
        MaxItemsToSkip maxItemsToSkip, 
        IsolationLevel isolationLevel,
        TimeSpan pause)
    {
        MinItemsToUpdate = minItemsToUpdate;
        MaxItemsToUpdates = maxItemsToUpdates;
        MinItemsToSkip = minItemsToSkip;
        MaxItemsToSkip = maxItemsToSkip;
        IsolationLevel = isolationLevel;
        Pause = pause;
    }

    public long MinItemsToUpdate { get; }
    public long MaxItemsToUpdates { get; }
    public long MinItemsToSkip { get; }
    public MaxItemsToSkip MaxItemsToSkip { get; }
    public IsolationLevel IsolationLevel { get; }
    public TimeSpan Pause { get; }
}