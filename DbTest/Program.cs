using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DbTest.Benchmarks.Options;
using DbTest.Pg;
using Npgsql;

namespace DbTest;

public static class Program
{
    public static void Main(string[] args)
    {
        var idGenerator = IdGenerators.GuidNewGuid;
        
        var reportLocation = Path.Combine(
            new FileInfo(typeof(Program).Assembly.Location).Directory?.FullName ?? "",
            "report.json");
        var cts = new CancellationTokenSource();
        var duration = GetDuration(args);
        var pgOptions = CreatePgOptions(idGenerator, "testguid");
        var bench = new PgBenchmark(pgOptions, cts);
        Console.WriteLine($"Start benchmark: {DateTimeOffset.UtcNow:O}. Duration: {duration:g}");
        Console.WriteLine("-----------------------");
        var sw = Stopwatch.StartNew();
        bench.Start();
        var end = DateTimeOffset.UtcNow.Add(duration);

        while (DateTimeOffset.UtcNow < end)
        {
            Console.WriteLine($"Elapsed: {sw.Elapsed:g} / {duration:g}");
            Console.WriteLine(bench.GetMetricsSnapshot().FormatLast());
            Console.WriteLine("-----------------------");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        sw.Stop();
        Console.WriteLine($"Stopping benchmark: {DateTimeOffset.UtcNow:O}");
        bench.Stop();
        Console.WriteLine("-----------------------");
        Console.WriteLine($"Benchmark stopped: {DateTimeOffset.UtcNow:O}");
        Console.WriteLine("-----------------------");
        var lastSnapshot = bench.GetMetricsSnapshot();
        var json = JsonSerializer.Serialize(lastSnapshot,
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true
            });
        File.WriteAllText(reportLocation, json, Encoding.UTF8);
        Console.WriteLine($"Report location: {reportLocation}");
        Console.WriteLine("-----------------------");
    }

    private static TimeSpan GetDuration(string[]? args)
    {
        if (args?.Length > 0
            && TimeSpan.TryParse(args[0], out var possibleDuration)
            && possibleDuration > TimeSpan.FromSeconds(1))
        {
            return possibleDuration;
        }

        return TimeSpan.FromMinutes(2);
    }

    private static PgBenchmarkOptions CreatePgOptions(IdGenerator idGenerator, string databaseName)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Database = databaseName,
            Host = "localhost",
            Username = "postgres",
            Password = "postgres",
            Port = 5432,
            MinPoolSize = 0,
            MaxPoolSize = 20,
            ConnectionLifetime = 0,
            ConnectionIdleLifetime = 86400,
            CommandTimeout = 86400,
            CancellationTimeout = 0
        };
        var connectionString = connectionStringBuilder.ConnectionString;
        var options = new PgBenchmarkOptions(
            CreateInsertOptions(idGenerator),
            CreateDeleteOptions(),
            CreateSelectOptions(),
            CreateUpdateOptions(),
            CreateVacuumOptions(),
            connectionString);
        return options;
    }

    private static InsertOptions CreateInsertOptions(IdGenerator idGenerator)
    {
        const int minBatchSize = 25_000;
        const int maxBatchSize = 25_000;
        var sleepInterval = TimeSpan.FromMilliseconds(1);
        var insertsStartedAt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var insertedItemStep = TimeSpan.FromMilliseconds(100);

        return new InsertOptions(
            idGenerator,
            minBatchSize,
            maxBatchSize,
            insertsStartedAt,
            insertedItemStep,
            IsolationLevel.ReadCommitted,
            sleepInterval);
    }

    private static SelectOptions CreateSelectOptions()
    {
        const int minItemsToSelect = 1_000;
        const int maxItemsToSelect = 1_000;
        var sleepInterval = TimeSpan.FromMilliseconds(1);

        return new SelectOptions(
            minItemsToSelect,
            maxItemsToSelect,
            IsolationLevel.ReadCommitted,
            sleepInterval);
    }

    private static DeleteOptions CreateDeleteOptions()
    {
        const int minItemsToDelete = 5_000;
        const int maxItemsToDelete = 5_000;
        var sleepInterval = TimeSpan.FromSeconds(1);

        return new DeleteOptions(
            minItemsToDelete,
            maxItemsToDelete,
            IsolationLevel.ReadCommitted,
            sleepInterval);
    }

    private static UpdateOptions CreateUpdateOptions()
    {
        const int minItemsToUpdate = 1_000;
        const int maxItemsToUpdate = 1_000;
        const int minItemsToSkip = 0;
        var sleepInterval = TimeSpan.FromSeconds(5);
        return new UpdateOptions(
            minItemsToUpdate,
            maxItemsToUpdate,
            minItemsToSkip,
            static alreadyWritten =>
            {
                var maxSkip = alreadyWritten - maxItemsToUpdate;
                if (maxSkip > 0)
                {
                    return maxSkip;
                }

                return 0;
            },
            IsolationLevel.ReadCommitted,
            sleepInterval);
    }

    private static VacuumOptions CreateVacuumOptions()
    {
        var sleepInterval = TimeSpan.FromSeconds(30);
        return new VacuumOptions(sleepInterval);
    }
}