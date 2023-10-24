using DbTest.Benchmarks.Metrics.Operations;

namespace DbTest.Benchmarks.Metrics;

public class BenchmarkMetrics
{
    private SelectMetrics[] _selectMetrics = Array.Empty<SelectMetrics>();
    private InsertMetrics[] _insertMetrics = Array.Empty<InsertMetrics>();
    private UpdateMetrics[] _updateMetrics = Array.Empty<UpdateMetrics>();
    private DeleteMetrics[] _deleteMetrics = Array.Empty<DeleteMetrics>();
    private VacuumMetrics[] _vacuumMetrics = Array.Empty<VacuumMetrics>();

    public void AddSelect(
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        TimeSpan duration,
        long itemsRequiredToSelectCount,
        long actualSelectedItems)
    {
        var measuredAt = DateTimeOffset.UtcNow;
        var currentMetrics = _selectMetrics;
        var firstMeasureDate = measuredAt;
        if (currentMetrics.Length > 0)
        {
            firstMeasureDate = currentMetrics.Min(x => x.MeasuredAt);
        }

        var selectMetrics = new SelectMetrics(
            firstMeasureDate,
            measuredAt,
            sleepInterval,
            alreadyWrittenItems,
            duration,
            itemsRequiredToSelectCount,
            actualSelectedItems);
        var newMetrics = new SelectMetrics[currentMetrics.Length + 1];
        Array.Copy(currentMetrics, newMetrics, currentMetrics.Length);
        newMetrics[currentMetrics.Length] = selectMetrics;
        Interlocked.Exchange(ref _selectMetrics, newMetrics);
    }

    public void AddInsert(
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        TimeSpan duration,
        int actualInsertedItems)
    {
        var measuredAt = DateTimeOffset.UtcNow;
        var currentMetrics = _insertMetrics;
        var firstMeasureDate = measuredAt;
        if (currentMetrics.Length > 0)
        {
            firstMeasureDate = currentMetrics.Min(x => x.MeasuredAt);
        }

        var insertMetrics = new InsertMetrics(
            firstMeasureDate,
            measuredAt,
            sleepInterval,
            alreadyWrittenItems,
            duration,
            actualInsertedItems);
        var newMetrics = new InsertMetrics[currentMetrics.Length + 1];
        Array.Copy(currentMetrics, newMetrics, currentMetrics.Length);
        newMetrics[currentMetrics.Length] = insertMetrics;
        Interlocked.Exchange(ref _insertMetrics, newMetrics);
    }

    public void AddUpdate(
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        TimeSpan duration,
        long offset,
        long itemsRequiredToUpdateCount,
        int actualUpdatedItems)
    {
        var measuredAt = DateTimeOffset.UtcNow;
        var currentMetrics = _updateMetrics;
        var firstMeasureDate = measuredAt;
        if (currentMetrics.Length > 0)
        {
            firstMeasureDate = currentMetrics.Min(x => x.MeasuredAt);
        }

        var updateMetrics = new UpdateMetrics(
            firstMeasureDate,
            measuredAt,
            sleepInterval,
            alreadyWrittenItems,
            duration,
            offset,
            itemsRequiredToUpdateCount,
            actualUpdatedItems);
        var newMetrics = new UpdateMetrics[currentMetrics.Length + 1];
        Array.Copy(currentMetrics, newMetrics, currentMetrics.Length);
        newMetrics[currentMetrics.Length] = updateMetrics;
        Interlocked.Exchange(ref _updateMetrics, newMetrics);
    }

    public void AddDelete(
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        TimeSpan duration,
        long itemsRequiredToDeleteCount,
        int actualDeletedItems)
    {
        var measuredAt = DateTimeOffset.UtcNow;
        var currentMetrics = _deleteMetrics;
        var firstMeasureDate = measuredAt;
        if (currentMetrics.Length > 0)
        {
            firstMeasureDate = currentMetrics.Min(x => x.MeasuredAt);
        }

        var deleteMetrics = new DeleteMetrics(
            firstMeasureDate,
            measuredAt,
            sleepInterval,
            alreadyWrittenItems,
            duration,
            itemsRequiredToDeleteCount,
            actualDeletedItems);
        var newMetrics = new DeleteMetrics[currentMetrics.Length + 1];
        Array.Copy(currentMetrics, newMetrics, currentMetrics.Length);
        newMetrics[currentMetrics.Length] = deleteMetrics;
        Interlocked.Exchange(ref _deleteMetrics, newMetrics);
    }

    public void AddVacuum(
        TimeSpan sleepInterval,
        long alreadyWrittenItems,
        long alreadyWrittenItemsBeforeVacuum,
        TimeSpan duration)
    {
        var measuredAt = DateTimeOffset.UtcNow;
        var currentMetrics = _vacuumMetrics;
        var firstMeasureDate = measuredAt;
        if (currentMetrics.Length > 0)
        {
            firstMeasureDate = currentMetrics.Min(x => x.MeasuredAt);
        }
        var vacuumMetrics = new VacuumMetrics(
            firstMeasureDate,
            measuredAt,
            sleepInterval,
            alreadyWrittenItems,
            alreadyWrittenItemsBeforeVacuum,
            duration);
        var newMetrics = new VacuumMetrics[currentMetrics.Length + 1];
        Array.Copy(currentMetrics, newMetrics, currentMetrics.Length);
        newMetrics[currentMetrics.Length] = vacuumMetrics;
        Interlocked.Exchange(ref _vacuumMetrics, newMetrics);
    }

    public MetricsSnapshot Snapshot()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var selectMetrics = _selectMetrics;
        var insertMetrics = _insertMetrics;
        var updateMetrics = _updateMetrics;
        var deleteMetrics = _deleteMetrics;
        var vacuumMetrics = _vacuumMetrics;

        var filterSelect = selectMetrics
            .Where(x => x.MeasuredAt <= createdAt)
            .OrderBy(x => x.MeasuredAt)
            .ToArray();
        var filterInsert = insertMetrics
            .Where(x => x.MeasuredAt <= createdAt)
            .OrderBy(x => x.MeasuredAt)
            .ToArray();
        var filterUpdate = updateMetrics
            .Where(x => x.MeasuredAt <= createdAt)
            .OrderBy(x => x.MeasuredAt)
            .ToArray();
        var filterDelete = deleteMetrics
            .Where(x => x.MeasuredAt <= createdAt)
            .OrderBy(x => x.MeasuredAt)
            .ToArray();
        var filterVacuum = vacuumMetrics
            .Where(x => x.MeasuredAt <= createdAt)
            .OrderBy(x => x.MeasuredAt)
            .ToArray();
        var snapshot = new MetricsSnapshot(
            createdAt,
            filterSelect,
            filterInsert,
            filterUpdate,
            filterDelete,
            filterVacuum);
        return snapshot;
    }
}