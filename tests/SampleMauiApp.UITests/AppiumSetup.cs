using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;

namespace SampleMauiApp.UITests;

/// <summary>
/// Creates a single Appium session for the whole UI test run.
///
/// Everything is driven by environment variables so the same test assembly runs
/// against an Android emulator or an iOS simulator without code changes:
///
///   UITEST_PLATFORM          android | ios          (default: android)
///   UITEST_APP_PATH          path to the .apk / .app bundle produced by the build
///   UITEST_DEVICE_NAME       e.g. "iPhone 16" or "Android Emulator"
///   UITEST_PLATFORM_VERSION  e.g. "18.2" or "15"    (optional)
///   UITEST_UDID              simulator/emulator udid (optional, disambiguates)
///   APPIUM_HOST              default http://127.0.0.1:4723
/// </summary>
[SetUpFixture]
public class AppiumSetup
{
    public const string AppId = "io.bitrise.mauiapp";

    private static AppiumDriver? driver;

    public static AppiumDriver App =>
        driver ?? throw new NullReferenceException("Appium driver has not been initialised.");

    public static string Platform =>
        (Environment.GetEnvironmentVariable("UITEST_PLATFORM") ?? "android").ToLowerInvariant();

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        var host = Environment.GetEnvironmentVariable("APPIUM_HOST") ?? "http://127.0.0.1:4723";
        var appPath = Environment.GetEnvironmentVariable("UITEST_APP_PATH");
        var deviceName = Environment.GetEnvironmentVariable("UITEST_DEVICE_NAME");
        var platformVersion = Environment.GetEnvironmentVariable("UITEST_PLATFORM_VERSION");
        var udid = Environment.GetEnvironmentVariable("UITEST_UDID");

        var options = new AppiumOptions();
        options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);

        if (!string.IsNullOrWhiteSpace(appPath))
            options.App = Path.GetFullPath(appPath);

        if (!string.IsNullOrWhiteSpace(platformVersion))
            options.PlatformVersion = platformVersion;

        if (!string.IsNullOrWhiteSpace(udid))
            options.AddAdditionalAppiumOption("appium:udid", udid);

        if (Platform == "ios")
        {
            options.AutomationName = "XCUITest";
            options.PlatformName = "iOS";
            options.DeviceName = deviceName ?? "iPhone 16";
            options.AddAdditionalAppiumOption("appium:bundleId", AppId);
            // The simulator is already booted by CI; don't let Appium shut it down.
            options.AddAdditionalAppiumOption("appium:noReset", false);

            driver = new IOSDriver(new Uri(host), options, TimeSpan.FromMinutes(5));
        }
        else
        {
            options.AutomationName = "UiAutomator2";
            options.PlatformName = "Android";
            options.DeviceName = deviceName ?? "Android Emulator";
            options.AddAdditionalAppiumOption("appium:appPackage", AppId);
            options.AddAdditionalAppiumOption("appium:appActivity", $"{AppId}.MainActivity");
            options.AddAdditionalAppiumOption("appium:autoGrantPermissions", true);

            driver = new AndroidDriver(new Uri(host), options, TimeSpan.FromMinutes(5));
        }

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        driver?.Quit();
        driver?.Dispose();
        driver = null;
    }
}
