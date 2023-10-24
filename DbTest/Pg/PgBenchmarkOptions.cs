using System.Data;
using DbTest.Benchmarks.Options;
using Npgsql;

namespace DbTest.Pg;

public class PgBenchmarkOptions : BenchmarkOptions<NpgsqlConnection, NpgsqlTransaction>
{
    public PgBenchmarkOptions(
        InsertOptions insert,
        DeleteOptions delete,
        SelectOptions select,
        UpdateOptions update,
        VacuumOptions vacuum,
        string connectionString)
        : base(
            insert,
            delete,
            select,
            update,
            vacuum,
            () => CreateConnection(connectionString),
            CreateTransaction)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        DatabaseName = builder.Database!;
        ConnectionString = connectionString;
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    private static NpgsqlConnection CreateConnection(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    private static NpgsqlTransaction CreateTransaction(NpgsqlConnection connection, IsolationLevel isolationLevel)
    {
        return connection.BeginTransaction(isolationLevel);
    }
}