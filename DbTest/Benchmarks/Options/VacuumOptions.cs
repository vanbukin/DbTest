namespace DbTest.Benchmarks.Options;

public class VacuumOptions
{
    public VacuumOptions(TimeSpan pause)
    {
        Pause = pause;
    }

    public TimeSpan Pause { get; }
}