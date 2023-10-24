using System.Data;
using System.Diagnostics;
using DbTest.Benchmarks.Metrics;
using DbTest.Benchmarks.Options;
using DbTest.Models;

namespace DbTest.Benchmarks;

public abstract class AbstractDbBenchmark<TOptions, TDbConnection, TDbTransaction>
    where TOptions : BenchmarkOptions<TDbConnection, TDbTransaction>
    where TDbConnection : IDbConnection
    where TDbTransaction : IDbTransaction
{
    protected readonly CancellationTokenSource Cts;
    protected readonly TOptions Options;
    protected readonly BenchmarkMetrics Metrics;

    protected AbstractDbBenchmark(TOptions options, CancellationTokenSource cts)
    {
        Options = options;
        Cts = cts;
        Metrics = new BenchmarkMetrics();
    }

    protected Thread InsertThread = null!;
    protected Thread SelectThread = null!;
    protected Thread UpdateThread = null!;
    protected Thread DeleteThread = null!;
    protected Thread VacuumThread = null!;
    protected long AlreadyWritten = 0L;

    public virtual void Start()
    {
        Prepare(Options, Cts.Token);
        InsertThread = new Thread(StartInsertLoop);
        SelectThread = new Thread(StartSelectLoop);
        UpdateThread = new Thread(StartUpdateLoop);
        DeleteThread = new Thread(StartDeleteLoop);
        VacuumThread = new Thread(StartVacuumLoop);
        InsertThread.Start();
        SelectThread.Start();
        UpdateThread.Start();
        DeleteThread.Start();
        VacuumThread.Start();
    }

    public virtual void Stop()
    {
        Cts.Cancel();
        InsertThread.Join();
        SelectThread.Join();
        UpdateThread.Join();
        DeleteThread.Join();
        VacuumThread.Join();
    }

    protected abstract void Prepare(
        TOptions options,
        CancellationToken cancellationToken);

    // INSERT
    protected virtual void StartInsertLoop()
    {
        var options = Options.Insert;
        var startDate = options.StartDate;
        while (!Cts.IsCancellationRequested)
        {
            var batchSize = Random.Shared.Next(options.MinBatchSize, options.MaxBatchSize);
            var itemsToInsert = TestRecord.Range(options.IdGenerator, batchSize, startDate, options.Step);
            startDate = itemsToInsert.Max(x => x.CreatedAt);
            using var connection = Options.ConnectionFactory();
            using var transaction = Options.TransactionFactory(connection, options.IsolationLevel);
            var sw = Stopwatch.StartNew();
            var insertedItems = Insert(connection, transaction, itemsToInsert, Cts.Token);
            transaction.Commit();
            sw.Stop();
            var currentAlreadyWritten = Interlocked.Add(ref AlreadyWritten, insertedItems);
            Metrics.AddInsert(
                options.Pause,
                currentAlreadyWritten,
                sw.Elapsed,
                insertedItems);
            if (options.Pause > TimeSpan.Zero)
            {
                Thread.Sleep(options.Pause);
            }
        }
    }

    protected abstract int Insert(
        TDbConnection connection,
        TDbTransaction transaction,
        TestRecord[] records,
        CancellationToken cancellationToken);

    // SELECT
    protected virtual void StartSelectLoop()
    {
        var options = Options.Select;
        while (!Cts.IsCancellationRequested)
        {
            var itemsRequiredToSelectCount = Random.Shared.NextInt64(
                options.MinItemsToSelect,
                options.MaxItemsToSelect);
            using var connection = Options.ConnectionFactory();
            using var transaction = Options.TransactionFactory(connection, options.IsolationLevel);
            var sw = Stopwatch.StartNew();
            var readItems = Select(connection, transaction, itemsRequiredToSelectCount, Cts.Token);
            transaction.Commit();
            sw.Stop();
            var currentAlreadyWritten = Interlocked.Read(ref AlreadyWritten);
            Metrics.AddSelect(
                options.Pause,
                currentAlreadyWritten,
                sw.Elapsed,
                itemsRequiredToSelectCount,
                readItems);
            if (options.Pause > TimeSpan.Zero)
            {
                Thread.Sleep(options.Pause);
            }
        }
    }

    protected abstract long Select(
        TDbConnection connection,
        TDbTransaction transaction,
        long itemsRequiredToSelectCount,
        CancellationToken cancellationToken);

    // UPDATE
    protected virtual void StartUpdateLoop()
    {
        var options = Options.Update;
        while (!Cts.IsCancellationRequested)
        {
            var maxItemsToSkip = options.MaxItemsToSkip(Interlocked.Read(ref AlreadyWritten));
            var offset = Random.Shared.NextInt64(options.MinItemsToSkip, maxItemsToSkip);
            var itemsRequiredToUpdateCount = Random.Shared.NextInt64(
                options.MinItemsToUpdate,
                options.MaxItemsToUpdates);
            using var connection = Options.ConnectionFactory();
            using var transaction = Options.TransactionFactory(connection, options.IsolationLevel);
            var sw = Stopwatch.StartNew();
            var updatedItems = Update(connection, transaction, offset, itemsRequiredToUpdateCount, Cts.Token);
            transaction.Commit();
            sw.Stop();
            var currentAlreadyWritten = Interlocked.Read(ref AlreadyWritten);
            Metrics.AddUpdate(
                options.Pause,
                currentAlreadyWritten,
                sw.Elapsed,
                offset,
                itemsRequiredToUpdateCount,
                updatedItems);
            if (options.Pause > TimeSpan.Zero)
            {
                Thread.Sleep(options.Pause);
            }
        }
    }

    protected abstract int Update(
        TDbConnection connection,
        TDbTransaction transaction,
        long offset,
        long itemsRequiredToUpdateCount,
        CancellationToken cancellationToken);

    // DELETE
    protected virtual void StartDeleteLoop()
    {
        var options = Options.Delete;
        while (!Cts.IsCancellationRequested)
        {
            var itemsRequiredToDeleteCount = Random.Shared.NextInt64(
                options.MinItemsToDelete,
                options.MaxItemsToDelete);
            using var connection = Options.ConnectionFactory();
            using var transaction = Options.TransactionFactory(connection, options.IsolationLevel);
            var sw = Stopwatch.StartNew();
            var deletedItems = Delete(connection, transaction, itemsRequiredToDeleteCount, Cts.Token);
            transaction.Commit();
            sw.Stop();
            var currentAlreadyWritten = Interlocked.Read(ref AlreadyWritten);
            Metrics.AddDelete(
                options.Pause,
                currentAlreadyWritten,
                sw.Elapsed,
                itemsRequiredToDeleteCount,
                deletedItems
            );
            if (options.Pause > TimeSpan.Zero)
            {
                Thread.Sleep(options.Pause);
            }
        }
    }

    protected abstract int Delete(
        TDbConnection connection,
        TDbTransaction transaction,
        long itemsRequiredToDeleteCount,
        CancellationToken cancellationToken);

    // VACUUM
    protected virtual void StartVacuumLoop()
    {
        var options = Options.Vacuum;
        while (!Cts.IsCancellationRequested)
        {
            using var connection = Options.ConnectionFactory();
            var alreadyWrittenItemsBeforeVacuum = Interlocked.Read(ref AlreadyWritten);
            var sw = Stopwatch.StartNew();
            Vacuum(connection, Cts.Token);
            sw.Stop();
            var currentAlreadyWritten = Interlocked.Read(ref AlreadyWritten);
            Metrics.AddVacuum(
                options.Pause,
                currentAlreadyWritten,
                alreadyWrittenItemsBeforeVacuum,
                sw.Elapsed);
            if (options.Pause > TimeSpan.Zero)
            {
                Thread.Sleep(options.Pause);
            }
        }
    }

    protected abstract void Vacuum(
        TDbConnection connection,
        CancellationToken cancellationToken);

    public virtual MetricsSnapshot GetMetricsSnapshot()
    {
        return Metrics.Snapshot();
    }
}