using SampleMauiApp.Core;

namespace SampleMauiApp.UnitTests;

public class CounterServiceTests
{
    [Fact]
    public void StartsAtZero()
    {
        var counter = new CounterService();
        Assert.Equal(0, counter.Count);
        Assert.Equal("Click me", counter.Describe());
    }

    [Fact]
    public void IncrementReturnsNewValue()
    {
        var counter = new CounterService();
        Assert.Equal(1, counter.Increment());
        Assert.Equal(2, counter.Increment());
        Assert.Equal(2, counter.Count);
    }

    [Theory]
    [InlineData(0, "Click me")]
    [InlineData(1, "Clicked 1 time")]
    [InlineData(2, "Clicked 2 times")]
    [InlineData(7, "Clicked 7 times")]
    public void DescribeUsesSingularAndPlural(int clicks, string expected)
    {
        var counter = new CounterService();
        for (var i = 0; i < clicks; i++)
            counter.Increment();

        Assert.Equal(expected, counter.Describe());
    }

    [Fact]
    public void ResetReturnsToZero()
    {
        var counter = new CounterService();
        counter.Increment();
        counter.Increment();

        counter.Reset();

        Assert.Equal(0, counter.Count);
        Assert.Equal("Click me", counter.Describe());
    }
}
