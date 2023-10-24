using System.Security.Cryptography;

namespace DbTest.Models;


public class TestRecord
{
    public TestRecord(Guid id, DateTime createdAt, string payload)
    {
        Id = id;
        CreatedAt = createdAt;
        Payload = payload;
    }

    public Guid Id { get; }
    public DateTime CreatedAt { get; }
    public string Payload { get; }

    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    public static TestRecord[] Range(
        IdGenerator idGenerator,
        int count,
        DateTime startDate,
        TimeSpan step)
    {
        var result = new TestRecord[count];
        Span<byte> binValue = stackalloc byte[64];
        for (int i = 0; i < count; i++)
        {
            var createdAt = startDate.Add(step);
            startDate = createdAt;
            Rng.GetBytes(binValue);
            var value = Convert.ToHexString(binValue);
            var id = idGenerator(createdAt, value);
            result[i] = new TestRecord(id, createdAt, value);
        }

        return result;
    }
}