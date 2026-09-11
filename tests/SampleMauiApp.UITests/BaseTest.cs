using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SampleMauiApp.UITests;

public abstract class BaseTest
{
    protected AppiumDriver App => AppiumSetup.App;

    /// <summary>
    /// MAUI surfaces <c>AutomationId</c> differently per platform:
    ///   iOS     -> the accessibility identifier
    ///   Android -> the view's resource-id, namespaced by the package
    /// This hides that difference so the tests themselves stay platform-agnostic.
    /// </summary>
    protected AppiumElement FindByAutomationId(string automationId)
    {
        var by = AppiumSetup.Platform == "ios"
            ? MobileBy.AccessibilityId(automationId)
            : MobileBy.Id($"{AppiumSetup.AppId}:id/{automationId}");

        return (AppiumElement)App.FindElement(by);
    }

    /// <summary>Puts the app back into a known state before each assertion.</summary>
    protected void Reset() => FindByAutomationId("ResetBtn").Click();

    [TearDown]
    public void AttachScreenshotOnFailure()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Failed)
            return;

        var dir = Environment.GetEnvironmentVariable("BITRISE_TEST_RESULT_DIR")
                  ?? TestContext.CurrentContext.WorkDirectory;
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, $"{TestContext.CurrentContext.Test.Name}.png");
        App.GetScreenshot().SaveAsFile(path);
        TestContext.AddTestAttachment(path);
    }
}
