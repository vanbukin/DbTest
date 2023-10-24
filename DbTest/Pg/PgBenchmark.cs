using DbTest.Benchmarks;
using DbTest.Models;
using Npgsql;
using NpgsqlTypes;

namespace DbTest.Pg;

public class PgBenchmark : AbstractDbBenchmark<PgBenchmarkOptions, NpgsqlConnection, NpgsqlTransaction>
{
    private const string TableName = "testrecords";

    public PgBenchmark(PgBenchmarkOptions options, CancellationTokenSource cts) : base(options, cts)
    {
    }

    protected override void Prepare(PgBenchmarkOptions options, CancellationToken cancellationToken)
    {
        var nonDbConnectionString = new NpgsqlConnectionStringBuilder(Options.ConnectionString)
        {
            Database = null
        };
        var prepareConnectionString = nonDbConnectionString.ConnectionString;
        using var prepareConnection = new NpgsqlConnection(prepareConnectionString);
        prepareConnection.Open();
        if (!IsDbExists(prepareConnection))
        {
            CreateDatabase(prepareConnection);
        }
        else
        {
            DropDatabase(prepareConnection);
            CreateDatabase(prepareConnection);
        }

        using var databaseConnection = new NpgsqlConnection(Options.ConnectionString);
        databaseConnection.Open();
        CreateTable(databaseConnection);
        CreateAscIndex(databaseConnection);
        CreateDescIndex(databaseConnection);
    }

    private bool IsDbExists(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName;";
        cmd.Parameters.AddWithValue("@dbName", Options.DatabaseName);
        var scalar = cmd.ExecuteScalar();
        if (scalar is 1)
        {
            return true;
        }

        return false;
    }

    private void CreateDatabase(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE {Options.DatabaseName};";
        cmd.ExecuteNonQuery();
    }

    private void DropDatabase(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"DROP DATABASE {Options.DatabaseName};";
        cmd.ExecuteNonQuery();
    }

    private void CreateTable(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            $"CREATE TABLE IF NOT EXISTS {TableName} (Id uuid not null primary key, CreatedAt timestamp with time zone not null, Payload varchar(200) not null);";
        cmd.ExecuteNonQuery();
    }

    private void CreateAscIndex(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            $"CREATE INDEX IF NOT EXISTS ix_{TableName}_CreatedAt_ASC ON {TableName} (CreatedAt ASC);";
        cmd.ExecuteNonQuery();
    }

    private void CreateDescIndex(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            $"CREATE INDEX IF NOT EXISTS ix_{TableName}_CreatedAt_DESC ON {TableName} (CreatedAt DESC);";
        cmd.ExecuteNonQuery();
    }

    protected override int Insert(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        TestRecord[] records,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return 0;
        }

        using var writer =
            connection.BeginBinaryImport($"COPY {TableName} (Id, CreatedAt, Payload) FROM STDIN (FORMAT BINARY)");
        var written = 0;
        foreach (var record in records)
        {
            writer.StartRow();
            writer.Write(record.Id, NpgsqlDbType.Uuid);
            writer.Write(record.CreatedAt, NpgsqlDbType.TimestampTz);
            writer.Write(record.Payload, NpgsqlDbType.Varchar);
            written++;
        }

        writer.Complete();
        return written;
    }

    protected override long Select(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long itemsRequiredToSelectCount,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return 0;
        }

        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText =
            $"SELECT Id, CreatedAt, Payload FROM {TableName} post ORDER BY CreatedAt DESC LIMIT {itemsRequiredToSelectCount}";
        using var reader = cmd.ExecuteReader();
        long itemsRead = 0L;
        while (reader.Read())
        {
            _ = reader.GetGuid(0);
            _ = reader.GetDateTime(1);
            _ = reader.GetString(2);
            itemsRead++;
        }

        return itemsRead;
    }

    protected override int Update(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long offset,
        long itemsRequiredToUpdateCount,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return 0;
        }

        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText =
            $"UPDATE {TableName} SET Payload = 'DEADBEEF' FROM (SELECT Id FROM {TableName} ORDER BY CreatedAt ASC LIMIT {itemsRequiredToUpdateCount} OFFSET {offset}) t WHERE {TableName}.Id = t.Id;";
        return cmd.ExecuteNonQuery();
    }

    protected override int Delete(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        long itemsRequiredToDeleteCount,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return 0;
        }

        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText =
            $"DELETE FROM {TableName} WHERE Id IN (SELECT Id FROM {TableName} ORDER BY CreatedAt ASC LIMIT {itemsRequiredToDeleteCount});";
        return cmd.ExecuteNonQuery();
    }

    protected override void Vacuum(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            $"VACUUM (ANALYZE) {TableName};";
        cmd.ExecuteNonQuery();
    }
}