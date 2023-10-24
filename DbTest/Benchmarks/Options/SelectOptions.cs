using System.Data;

namespace DbTest.Benchmarks.Options;

public class SelectOptions
{
    public SelectOptions(
        long minItemsToSelect,
        long maxItemsToSelect,
        IsolationLevel isolationLevel,
        TimeSpan pause)
    {
        MinItemsToSelect = minItemsToSelect;
        MaxItemsToSelect = maxItemsToSelect;
        IsolationLevel = isolationLevel;
        Pause = pause;
    }

    public long MinItemsToSelect { get; }
    public long MaxItemsToSelect { get; }
    public IsolationLevel IsolationLevel { get; }
    public TimeSpan Pause { get; }
}