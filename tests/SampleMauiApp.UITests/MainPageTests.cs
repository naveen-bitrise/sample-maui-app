using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SampleMauiApp.UITests;

public class MainPageTests : BaseTest
{
    [Test]
    public void AppLaunchesAndShowsGreeting()
    {
        var label = FindByAutomationId("HelloLabel");

        Assert.That(label.Text, Is.EqualTo("Hello, Bitrise!"));
    }

    [Test]
    public void CounterStartsAtClickMe()
    {
        Reset();

        Assert.That(FindByAutomationId("CounterBtn").Text, Is.EqualTo("Click me"));
    }

    [Test]
    public void ClickingOnceShowsSingularText()
    {
        Reset();

        FindByAutomationId("CounterBtn").Click();

        Assert.That(FindByAutomationId("CounterBtn").Text, Is.EqualTo("Clicked 1 time"));
    }

    [Test]
    public void ClickingSeveralTimesShowsPluralText()
    {
        Reset();

        for (var i = 0; i < 3; i++)
            FindByAutomationId("CounterBtn").Click();

        Assert.That(FindByAutomationId("CounterBtn").Text, Is.EqualTo("Clicked 3 times"));
    }

    [Test]
    public void ResetPutsCounterBackToZero()
    {
        Reset();
        FindByAutomationId("CounterBtn").Click();
        FindByAutomationId("CounterBtn").Click();

        Reset();

        Assert.That(FindByAutomationId("CounterBtn").Text, Is.EqualTo("Click me"));
    }
}
