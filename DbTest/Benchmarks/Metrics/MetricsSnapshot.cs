using System.Text;
using DbTest.Benchmarks.Metrics.Operations;

namespace DbTest.Benchmarks.Metrics;

public class MetricsSnapshot
{
    public MetricsSnapshot(
        DateTimeOffset createdAt,
        SelectMetrics[] selectMetrics,
        InsertMetrics[] insertMetrics,
        UpdateMetrics[] updateMetrics,
        DeleteMetrics[] deleteMetrics,
        VacuumMetrics[] vacuumMetrics)
    {
        CreatedAt = createdAt;
        SelectMetrics = selectMetrics;
        InsertMetrics = insertMetrics;
        UpdateMetrics = updateMetrics;
        DeleteMetrics = deleteMetrics;
        VacuumMetrics = vacuumMetrics;
    }

    public DateTimeOffset CreatedAt { get; }
    public SelectMetrics[] SelectMetrics { get; }
    public InsertMetrics[] InsertMetrics { get; }
    public UpdateMetrics[] UpdateMetrics { get; }
    public DeleteMetrics[] DeleteMetrics { get; }
    public VacuumMetrics[] VacuumMetrics { get; }

    public string FormatLast()
    {
        var insert = InsertMetrics.LastOrDefault();
        var select = SelectMetrics.LastOrDefault();
        var update = UpdateMetrics.LastOrDefault();
        var delete = DeleteMetrics.LastOrDefault();
        var vacuum = VacuumMetrics.LastOrDefault();
        var builder = new StringBuilder(1024);
        builder.AppendLine($"Date: {CreatedAt.ToString("O")}");
        builder.AppendLine(
            $"Insert[{InsertMetrics.Length}] - Duration: {insert?.Duration.ToString("g")}, Rate: {insert?.RatePerSecond.ToString("F2")}, Total: {insert?.AlreadyWrittenItems}, Inserted: {insert?.ActualInsertedItems}");
        builder.AppendLine(
            $"Select[{SelectMetrics.Length}] - Duration: {select?.Duration.ToString("g")}, Rate: {select?.RatePerSecond.ToString("F2")}, Total: {select?.AlreadyWrittenItems}, Selected: {select?.ActualSelectedItems}");
        builder.AppendLine(
            $"Update[{UpdateMetrics.Length}] - Duration: {update?.Duration.ToString("g")}, Rate: {update?.RatePerSecond.ToString("F2")}, Total: {update?.AlreadyWrittenItems}, Selected: {update?.ActualUpdatedItems}");
        builder.AppendLine(
            $"Delete[{DeleteMetrics.Length}] - Duration: {delete?.Duration.ToString("g")}, Rate: {delete?.RatePerSecond.ToString("F2")}, Total: {delete?.AlreadyWrittenItems}, Selected: {delete?.ActualDeletedItems}");
        builder.AppendLine(
            $"Vacuum[{VacuumMetrics.Length}] - Duration: {vacuum?.Duration.ToString("g")}");
        return builder.ToString();
    }
}