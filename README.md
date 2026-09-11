# Sample .NET MAUI app on Bitrise

A minimal [.NET MAUI](https://learn.microsoft.com/dotnet/maui/) demo app wired up to
Bitrise with unit tests, Appium UI tests, and simulator/emulator builds.

- **Targets:** iOS and Android (`net10.0-ios`, `net10.0-android`)
- **App identifier:** `io.bitrise.mauiapp`
- **SDK:** .NET 10

## Layout

| Path | What it is |
|---|---|
| `src/SampleMauiApp` | The MAUI app — XAML UI, one page with a counter |
| `src/SampleMauiApp.Core` | Platform-independent logic (`CounterService`), so it can be unit tested without a device |
| `tests/SampleMauiApp.UnitTests` | xUnit tests over `SampleMauiApp.Core` |
| `tests/SampleMauiApp.UITests` | Appium + NUnit tests that drive the real app on a simulator/emulator |
| `bitrise.yml` | CI configuration |

The app itself is deliberately small: a label, a counter button, and a reset button.
The point is the pipeline around it, not the app.

## Bitrise workflows

| Workflow | Stack | What it does |
|---|---|---|
| `unit_tests` | Ubuntu | `dotnet test` over the core library. No device needed, so it runs on Linux. |
| `build_simulator` | macOS / Xcode | Builds the iOS `.app` for `iossimulator-arm64` and uploads it zipped. |
| `build_emulator` | Ubuntu | Builds a debug APK (arm64-v8a + x86_64) that installs on an emulator. |
| `ui_tests_ios` | macOS / Xcode | Boots a simulator, starts Appium, runs the UI tests. |
| `ui_tests_android` | Ubuntu | Boots an emulator via AVD Manager, starts Appium, runs the UI tests. |

There is also a **`ui_tests` pipeline** that runs `ui_tests_ios` and `ui_tests_android`
in parallel. It has to be a pipeline rather than one workflow because Android emulators
cannot run on Bitrise's Apple Silicon macOS machines — each half needs its own stack.

`.NET` has no first-party Bitrise Step, so the SDK is installed with Microsoft's
official `dotnet-install.sh` in a Script Step and cached between builds, along with
the NuGet package cache.

## Running locally

Prerequisites: .NET 10 SDK, the MAUI workload, JDK 17 (Android), Xcode (iOS),
and Appium with the relevant driver.

```bash
dotnet workload install maui
npm install -g appium && appium driver install xcuitest && appium driver install uiautomator2
```

Unit tests need nothing else:

```bash
dotnet test tests/SampleMauiApp.UnitTests/SampleMauiApp.UnitTests.csproj
```

UI tests need a running Appium server plus a booted device. Start Appium with
`ANDROID_HOME` exported (the *server* resolves the SDK, not the test process):

```bash
ANDROID_HOME="$HOME/Library/Android/sdk" appium server --port 4723
```

Then point the tests at a build. iOS:

```bash
dotnet build src/SampleMauiApp/SampleMauiApp.csproj -f net10.0-ios -c Debug -p:RuntimeIdentifier=iossimulator-arm64
```

```bash
UITEST_PLATFORM=ios UITEST_APP_PATH="$PWD/src/SampleMauiApp/bin/Debug/net10.0-ios/iossimulator-arm64/SampleMauiApp.app" UITEST_DEVICE_NAME="iPhone 17" UITEST_PLATFORM_VERSION="26.5" dotnet test tests/SampleMauiApp.UITests/SampleMauiApp.UITests.csproj
```

Android:

```bash
dotnet build src/SampleMauiApp/SampleMauiApp.csproj -f net10.0-android -c Debug
```

```bash
UITEST_PLATFORM=android UITEST_APP_PATH="$PWD/src/SampleMauiApp/bin/Debug/net10.0-android/io.bitrise.mauiapp-Signed.apk" UITEST_DEVICE_NAME=emulator-5554 dotnet test tests/SampleMauiApp.UITests/SampleMauiApp.UITests.csproj
```

`UITEST_APP_PATH` must be absolute — it is resolved against the test host's working
directory, not the repo root.

## Three MAUI gotchas worth knowing

All three bit this repo during setup and are baked into the config now.

**1. Debug APKs are not self-contained by default.** MAUI's "Fast Deployment" ships
Debug Android assemblies to the device separately instead of packaging them, so an
APK installed standalone dies at launch with
`No assemblies found in '...__override__/arm64-v8a' ... Exiting...`. `SampleMauiApp.csproj`
sets `EmbedAssembliesIntoApk=true` for Android so every APK we produce is installable.

**2. `AutomationId` does not map to the same thing on both platforms.**
On iOS it becomes the accessibility identifier; on Android it becomes the view's
*resource-id* (`io.bitrise.mauiapp:id/CounterBtn`), not the content-description.
`BaseTest.FindByAutomationId` hides that difference so the tests stay platform-agnostic.

**3. Building one platform still requires every platform's workload.**
`dotnet build --framework net10.0-ios` looks like it only needs the iOS workload,
but the implicit restore evaluates *every* entry in `TargetFrameworks`, so it fails
with `NETSDK1147: the following workloads must be installed: android`. Overriding
`-p:TargetFrameworks` on the command line is the wrong fix: it is a global property
and leaks into `SampleMauiApp.Core`, which targets plain `net10.0`. Instead the app
csproj reads a custom `BuildPlatform` property, and CI passes `-p:BuildPlatform=ios`
or `-p:BuildPlatform=android`:

```bash
dotnet build src/SampleMauiApp/SampleMauiApp.csproj -p:BuildPlatform=ios --framework net10.0-ios -c Debug -p:RuntimeIdentifier=iossimulator-arm64
```

With no `BuildPlatform` set you get both platforms on macOS and Android only on Linux,
so a plain `dotnet build` still does the obvious thing locally.

The Android launcher activity name is also pinned via
`[Activity(Name = "io.bitrise.mauiapp.MainActivity")]` — without it, MAUI generates a
hash-based name like `crc64ebf26f014e0a28cf.MainActivity` that changes as the code does.
