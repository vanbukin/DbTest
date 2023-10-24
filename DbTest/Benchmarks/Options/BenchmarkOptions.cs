using System.Data;

namespace DbTest.Benchmarks.Options;

public delegate long MaxItemsToSkip(long alreadyWritten);

public class BenchmarkOptions<TDbConnection, TDbTransaction>
    where TDbConnection : IDbConnection
    where TDbTransaction : IDbTransaction
{
    public BenchmarkOptions(
        InsertOptions insert,
        DeleteOptions delete,
        SelectOptions select,
        UpdateOptions update,
        VacuumOptions vacuum,
        Func<TDbConnection> connectionFactory,
        Func<TDbConnection, IsolationLevel, TDbTransaction> transactionFactory)
    {
        Insert = insert;
        Delete = delete;
        Select = select;
        Update = update;
        Vacuum = vacuum;
        ConnectionFactory = connectionFactory;
        TransactionFactory = transactionFactory;
    }

    public InsertOptions Insert { get; }
    public DeleteOptions Delete { get; }
    public SelectOptions Select { get; }
    public UpdateOptions Update { get; }
    public VacuumOptions Vacuum { get; }
    public Func<TDbConnection> ConnectionFactory { get; }
    public Func<TDbConnection, IsolationLevel, TDbTransaction> TransactionFactory { get; }
}