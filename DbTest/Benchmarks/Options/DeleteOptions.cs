using System.Data;

namespace DbTest.Benchmarks.Options;

public class DeleteOptions
{
    public DeleteOptions(
        long minItemsToDelete, 
        long maxItemsToDelete, 
        IsolationLevel isolationLevel,
        TimeSpan pause)
    {
        MinItemsToDelete = minItemsToDelete;
        MaxItemsToDelete = maxItemsToDelete;
        IsolationLevel = isolationLevel;
        Pause = pause;
    }

    public long MinItemsToDelete { get; }
    public long MaxItemsToDelete { get; }
    public IsolationLevel IsolationLevel { get; }
    public TimeSpan Pause { get; }
}