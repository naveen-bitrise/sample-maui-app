namespace SampleMauiApp.Core;

/// <summary>
/// Tiny piece of platform-independent business logic so there is something
/// meaningful for the unit test workflow to exercise.
/// </summary>
public class CounterService
{
    public int Count { get; private set; }

    public int Increment()
    {
        Count++;
        return Count;
    }

    public void Reset() => Count = 0;

    public string Describe() => Count switch
    {
        0 => "Click me",
        1 => "Clicked 1 time",
        _ => $"Clicked {Count} times",
    };
}
