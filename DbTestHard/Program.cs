using System.Data;
using System.Diagnostics;
using System.Text;
using Dodo.Primitives;
using Npgsql;

namespace DbTestHard;

public static class Program
{
    private static readonly string ConnectionString = new NpgsqlConnectionStringBuilder
    {
        Database = DatabaseName,
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
    }.ConnectionString;

    private const int ChunkSize = 25_000;
    private const int PerfTestCounts = 40;
    private const string TableName = "uuids";
    private const string DatabaseName = "harduuids";

    public static void Main()
    {
        RunMainTestDodoPrimitivesMySqlOptimized();
        //RunMainTestGuid();
        
    }

    public static void RunMainTestDodoPrimitivesMySqlOptimized()
    {
        Prepare();
        var inserts = new List<TimeSpan>();
        var reads = new List<ReadDurations>();
        for (var i = 0; i < PerfTestCounts; i++)
        {
            var elapsed = PerfTestInsert("Dodo.Primitives MySqlOptimized", GetDodoUuids(), TableName, ChunkSize);
            Thread.Sleep(1000);
            var readDurations = ReadAll(TableName);
            inserts.Add(elapsed);
            reads.Add(readDurations);
            PrintResults(inserts, reads);
        }
    }

    public static void RunMainTestGuid()
    {
        Prepare();
        var inserts = new List<TimeSpan>();
        var reads = new List<ReadDurations>();
        for (var i = 0; i < PerfTestCounts; i++)
        {
            var elapsed = PerfTestInsert("Guid.NewGuid", GetGuids(), TableName, ChunkSize);
            Thread.Sleep(1000);
            var readDurations = ReadAll(TableName);
            inserts.Add(elapsed);
            reads.Add(readDurations);
            PrintResults(inserts, reads);
        }
    }
    

    private static void PrintResults(List<TimeSpan> insertDurations, List<ReadDurations> readDurations)
    {
        for (var i = 0; i < insertDurations.Count; i++)
        {
            var insertDuration = insertDurations[i];
            var readDuration = readDurations[i];
            Console.WriteLine(
                $"{i + 1} - Insert: {insertDuration:G}, Reader: {readDuration.ExecuteReaderDuration:G}, AllRecords: {readDuration.AllReadsDuration:G}");
        }
    }
    
    
    private static void Prepare()
    {
        var nonDbConnectionString = new NpgsqlConnectionStringBuilder(ConnectionString)
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

        using var databaseConnection = new NpgsqlConnection(ConnectionString);
        databaseConnection.Open();
        CreateTable(databaseConnection);
    }

    private static bool IsDbExists(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName;";
        cmd.Parameters.AddWithValue("@dbName", DatabaseName);
        var scalar = cmd.ExecuteScalar();
        if (scalar is 1)
        {
            return true;
        }

        return false;
    }

    private static void CreateDatabase(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE {DatabaseName};";
        cmd.ExecuteNonQuery();
    }

    private static void DropDatabase(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"DROP DATABASE {DatabaseName};";
        cmd.ExecuteNonQuery();
    }

    private static void CreateTable(NpgsqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            $"CREATE TABLE IF NOT EXISTS {TableName} (Id uuid not null primary key);";
        cmd.ExecuteNonQuery();
    }


    private static TimeSpan PerfTestInsert(string perfTestCase, byte[][] payload, string tableName, int chunkSize)
    {
        Console.WriteLine(perfTestCase);
        var totalInsert = 0;
        var uuids = payload.Chunk(chunkSize).ToArray();
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        var sw = Stopwatch.StartNew();
        foreach (var chunk in uuids)
        {
            using var cmd = connection.CreateCommand();
            var sb = new StringBuilder();
            sb.AppendLine($"INSERT INTO {tableName} (id) VALUES ");
            sb.AppendJoin(",", chunk.Select(x => $"(CAST(ENCODE('\\x{Convert.ToHexString(x)}', 'hex') AS UUID))"));
            sb.AppendLine(";");

            cmd.CommandText = sb.ToString();
            var inserted = cmd.ExecuteNonQuery();
            totalInsert += inserted;
            if (totalInsert % (chunkSize * 10) == 0)
            {
                Console.WriteLine($"Inserted: {totalInsert:N}");
            }
        }

        sw.Stop();
        return sw.Elapsed;
    }

    private static byte[][] GetDodoUuids()
    {
        return GetDodoUuidsEnumerable().ToArray();
    }
    
    private static byte[][] GetGuids()
    {
        return GetGuidsEnumerable().ToArray();
    }

    private static IEnumerable<byte[]> GetDodoUuidsEnumerable()
    {
        return Enumerable.Range(0, 10_000_000).Select(x =>
        {
            Uuid.NewMySqlOptimized();
            Uuid.NewMySqlOptimized();
            Uuid.NewMySqlOptimized();
            Uuid.NewMySqlOptimized();
            Uuid.NewMySqlOptimized();
            return Uuid.NewMySqlOptimized().ToByteArray();
        });
    }

    private static IEnumerable<byte[]> GetGuidsEnumerable()
    {
        return Enumerable.Range(0, 10_000_000).Select(x =>
        {
            Guid.NewGuid();
            Guid.NewGuid();
            Guid.NewGuid();
            Guid.NewGuid();
            Guid.NewGuid();
            return Guid.NewGuid().ToByteArray();
        });
    }

    private static ReadDurations ReadAll(string tableName)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT * FROM {tableName}";
        var allReadsSw = Stopwatch.StartNew();
        var readerSw = Stopwatch.StartNew();
        using var reader = cmd.ExecuteReader();
        readerSw.Stop();
        while (reader.Read())
        {
            // nothing
        }

        allReadsSw.Stop();
        return new ReadDurations(readerSw.Elapsed, allReadsSw.Elapsed.Subtract(readerSw.Elapsed));
    }

    private record ReadDurations(TimeSpan ExecuteReaderDuration, TimeSpan AllReadsDuration);
}