# Securiti Consent SDK for Unity - Integration Guide

> Unity plugin for Securiti's consent management platform.
> Provides a unified C# API for Android and iOS native SDKs, enabling GDPR, CCPA, and other privacy compliance in Unity games.

| | |
|---|---|
| **Package** | `ai.securiti.consent` |
| **Version** | 0.1.0 |
| **Unity** | 2021.3+ |
| **Platforms** | Android, iOS |
| **License** | MIT |

---

## Table of Contents

1. [Requirements](#requirements)
2. [Installation](#installation)
3. [Quick Start](#quick-start)
4. [Configuration](#configuration)
5. [API Reference](#api-reference)
6. [Consent Management](#consent-management)
7. [Google Consent Mode](#google-consent-mode)
8. [Platform-Specific Notes](#platform-specific-notes)
9. [Customization](#customization)
10. [Samples](#samples)
11. [Troubleshooting](#troubleshooting)

---

## Requirements

| Requirement | Version |
|---|---|
| Unity | 2021.3 or later |
| Android minimum SDK | 24 (Android 7.0) |
| iOS deployment target | 15.0 |
| Xcode | 14.0+ |

The SDK bundles the following automatically -- no separate installation needed:

- **External Dependency Manager for Unity (EDM4U)** v1.2.187 -- resolves native Android (Gradle/Maven) and iOS (CocoaPods) dependencies
- **Newtonsoft JSON** (`com.unity.nuget.newtonsoft-json`) -- declared as a UPM dependency, resolved by Unity Package Manager

---

## Installation

### Option A: Local Package (from disk)

1. Clone or download the SDK repository
2. In your Unity project, open `Packages/manifest.json`
3. Add the following to the `dependencies` block:

```json
{
  "dependencies": {
    "ai.securiti.consent": "file:../path/to/unity-package",
    ...
  }
}
```

Replace `../path/to/unity-package` with the relative path from your project root to the SDK's `unity-package` directory.

### Option B: Git URL

Add the package via Unity Package Manager using a Git URL:

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter the repository URL pointing to the `unity-package` directory

### Post-Installation

After adding the package:

1. Unity will import the package and compile
2. EDM4U (bundled) will be detected automatically
3. For **Android**: Go to **Assets > External Dependency Manager > Android Resolver > Resolve** to download native dependencies
4. For **iOS**: Dependencies are resolved automatically during the Xcode build via CocoaPods

---

## Quick Start

The SDK includes a `ConsentManager` component that **auto-bootstraps at game start** -- no scene setup required. It uses `[RuntimeInitializeOnLoadMethod]` to create itself automatically.

### Step 1: Configure Credentials

The `ConsentManager` is created at runtime. To customize your credentials, add a `ConsentManager` component to any GameObject in your scene and configure the Inspector fields:

| Field | Description |
|---|---|
| **Android App URL** | Your Securiti app URL for Android |
| **Android CDN URL** | CDN URL for Android consent assets |
| **Android Tenant ID** | Your Securiti tenant ID for Android |
| **Android App ID** | Your Securiti app ID for Android |
| **iOS App URL** | Your Securiti app URL for iOS |
| **iOS CDN URL** | CDN URL for iOS consent assets |
| **iOS Tenant ID** | Your Securiti tenant ID for iOS |
| **iOS App ID** | Your Securiti app ID for iOS |
| **Testing Mode** | Enable sandbox mode (default: `true`) |
| **Logger Level** | Log verbosity: `DEBUG`, `INFO`, `WARNING`, `ERROR` |

The SDK automatically selects the Android or iOS configuration at runtime based on the build platform.

### Step 2: Build and Run

That's it. On launch, `ConsentManager` will:

1. Initialize the SDK with your platform-specific credentials
2. Listen for the `OnSDKReadyEvent`
3. Automatically present the consent banner once the SDK is ready

### Step 3 (Optional): Add a Preferences Button

To let users revisit their consent choices later, call:

```csharp
SecuritiConsentSDK.Instance.PresentPreferenceCenter();
```

For example, from a settings menu button.

---

## Configuration

### CmpSDKOptions

All SDK configuration is done through the `CmpSDKOptions` class:

```csharp
var options = new CmpSDKOptions
{
    // Required
    AppURL    = "https://your-app.securiti.xyz",
    CdnURL    = "https://cdn.securiti.xyz",
    TenantID  = "your-tenant-id",
    AppID     = "your-app-id",

    // Optional
    TestingMode          = true,
    LoggerLevel          = LoggerLevel.DEBUG,
    ConsentsCheckInterval = 300,    // seconds
    SubjectId            = "",      // user identifier
    LanguageCode         = "en",    // e.g. "en", "es", "de"
    LocationCode         = ""       // region code
};
```

| Field | Type | Required | Default | Description |
|---|---|---|---|---|
| `AppURL` | `string` | Yes | -- | Base URL for the consent application |
| `CdnURL` | `string` | Yes | -- | CDN URL for consent assets |
| `TenantID` | `string` | Yes | -- | Securiti tenant identifier |
| `AppID` | `string` | Yes | -- | Application identifier |
| `TestingMode` | `bool` | No | `false` | Enable testing/sandbox mode |
| `LoggerLevel` | `LoggerLevel` | No | `INFO` | Logging verbosity level |
| `ConsentsCheckInterval` | `int` | No | `300` | Interval (seconds) for checking consent updates |
| `SubjectId` | `string` | No | `""` | Optional subject/user identifier |
| `LanguageCode` | `string` | No | `""` | Language code (e.g. `"en"`, `"es"`) |
| `LocationCode` | `string` | No | `""` | Location/region code |

#### Validation

```csharp
if (!options.IsValid())
{
    Debug.LogError(options.GetValidationError());
    return;
}
```

---

## API Reference

### SecuritiConsentSDK

The main SDK interface. Access via the singleton:

```csharp
var sdk = SecuritiConsentSDK.Instance;
```

### Initialization

```csharp
// Initialize the SDK (must be called before any other method)
sdk.Initialize(CmpSDKOptions options);

// Check SDK status
bool ready = sdk.IsSdkReady();
SDKStatus status = sdk.GetSDKStatus();
// Returns: AVAILABLE, NOT_AVAILABLE, or IN_PROGRESS
```

### Events

Subscribe to these events to respond to SDK state changes:

```csharp
// SDK has finished initialization and is ready to use
sdk.OnSDKReadyEvent += () => { Debug.Log("SDK Ready"); };

// An error occurred
sdk.OnErrorEvent += (string error) => { Debug.LogError(error); };

// All consents have been reset
sdk.OnConsentsResetEvent += () => { Debug.Log("Consents reset"); };

// Consents were uploaded to the server
sdk.OnConsentsUploadedEvent += (bool success) => { Debug.Log($"Upload: {success}"); };
```

> Always unsubscribe in `OnDestroy()` to prevent memory leaks.

### Presenting UI

```csharp
// Show the consent banner
sdk.PresentConsentBanner();

// Show the preference center (for managing consent choices)
sdk.PresentPreferenceCenter();
```

### Querying Consent

```csharp
// Get all purposes
Purpose[] purposes = sdk.GetPurposes();
foreach (var p in purposes)
{
    Debug.Log($"{p.name}: {p.GetConsentStatus()}");
}

// Get all permissions
AppPermission[] permissions = sdk.GetPermissions();

// Get consent for a specific purpose
ConsentStatus status = sdk.GetConsentByPurposeId("analytics");

// Get consent for a specific permission
ConsentStatus permStatus = sdk.GetConsentByPermissionId("camera");
```

### Setting Consent Programmatically

```csharp
// Set consent for a purpose
bool saved = sdk.SetPurposeConsent("analytics", ConsentStatus.GIVEN);

// Set consent for a permission
bool saved = sdk.SetPermissionConsent("camera", ConsentStatus.DENIED);

// Reset all consents
sdk.ResetConsents();
```

### Uploading Consents

```csharp
var request = new PostConsentsRequest
{
    Uuid = "user-uuid",
    AppUuid = "app-uuid",
    Device = SystemInfo.deviceModel,
    Version = 1,
    PurposeConsents = new List<PurposeConsent>
    {
        new PurposeConsent
        {
            PurposeId = 1,
            ConsentStatusValue = "GIVEN",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }
    }
};

sdk.UploadConsents(request);
```

### Configuration Retrieval

```csharp
// Get banner configuration
BannerConfig bannerConfig = sdk.GetBannerConfig();

// Get settings prompt configuration
SettingsPrompt settingsPrompt = sdk.GetSettingsPrompt();

// Get SDKs associated with a purpose (Android only)
SdkInfo[] sdks = sdk.GetSdksInPurpose("analytics");
```

---

## Consent Management

### ConsentStatus Enum

| Value | Int | Description |
|---|---|---|
| `NOT_DETERMINED` | 0 | User has not yet made a choice |
| `GIVEN` | 1 | User granted consent |
| `DENIED` | 2 | User denied consent |
| `WITHDRAWN` | 3 | User withdrew previously given consent |

### Purpose Model

```csharp
public class Purpose
{
    public string id;            // Unique identifier
    public string name;          // Display name
    public string description;   // Detailed description
    public int consent;          // Raw consent value
    public string consentString; // String representation
    public bool required;        // Whether purpose is mandatory

    public ConsentStatus GetConsentStatus();
    public void SetConsentStatus(ConsentStatus status);
}
```

### AppPermission Model

```csharp
public class AppPermission
{
    public string id;            // Unique identifier
    public string name;          // Display name
    public string description;   // Detailed description
    public int consent;          // Raw consent value
    public string consentString; // String representation

    public ConsentStatus GetConsentStatus();
    public void SetConsentStatus(ConsentStatus status);
}
```

---

## Google Consent Mode

The SDK supports Google Consent Mode (GCM) for Firebase Analytics integration:

```csharp
// Get all GCM consent statuses
GCMConsent[] gcmConsents = sdk.GetGCMConsents();
foreach (var gcm in gcmConsents)
{
    Debug.Log($"{gcm.Type}: {gcm.GetConsentStatus()}");
}

// Get GCM configuration
GcmConfig config = sdk.GetGCMConfig();
```

### GoogleConsentType Enum

| Value | Description |
|---|---|
| `ANALYTICS_STORAGE` | Analytics data storage consent |
| `AD_STORAGE` | Advertising data storage consent |
| `AD_USER_DATA` | User data for advertising consent |
| `AD_PERSONALIZATION` | Ad personalization consent |

---

## Platform-Specific Notes

### Android

- The SDK uses a hardware-accelerated trampoline Activity (`SecuritiConsentActivity`) to render consent UI. This is required because Unity's default activity has `hardwareAccelerated="false"`, which is incompatible with Jetpack Compose used by the native SDK.
- Native dependencies are resolved via EDM4U from the Maven repository at `https://cdn-qa.securiti.xyz/consent/maven`
- The Android bridge AAR (`securiti-unity-bridge.aar`) is included in the package under `Plugins/Android/`

**Android Dependencies** (resolved automatically by EDM4U):

| Dependency | Version |
|---|---|
| `ai.securiti.cmpsdkcore:consent-sdk` | 1.141.0 |
| `org.jetbrains.kotlinx:kotlinx-coroutines-core` | 1.7.3 |
| `org.jetbrains.kotlinx:kotlinx-coroutines-android` | 1.7.3 |
| `androidx.lifecycle:lifecycle-runtime-ktx` | 2.6.2 |
| `androidx.activity:activity-ktx` | 1.8.1 |
| `androidx.core:core-ktx` | 1.12.0 |
| `androidx.appcompat:appcompat` | 1.6.1 |

### iOS

- The native SDK (`ConsentUI`) is resolved via CocoaPods during the Xcode build
- A post-build processor (`SecuritiConsentBuildProcessor`) runs automatically to:
  - Embed the `ConsentUI.xcframework` in the app bundle
  - Set Swift 5.0, enable Objective-C modules, and disable Bitcode
  - Set the minimum iOS deployment target to 15.0
- The iOS bridge files (`SecuritiConsentBridge.h`, `.mm`, `.swift`) are in `Plugins/iOS/`

**iOS Dependencies** (resolved automatically via CocoaPods):

| Dependency | Version | Source |
|---|---|---|
| `ConsentUI` | 1.143.0 | GitHub (securitiai/mobile-consent-sdk-ios) |

### Unity Editor

In the Unity Editor, the SDK runs in **mock mode**:

- `Initialize()` simulates a successful initialization after 0.5 seconds
- `PresentConsentBanner()` and `PresentPreferenceCenter()` log messages to the console
- `GetPurposes()` returns mock purposes (Analytics, Marketing)
- `GetPermissions()` returns mock permissions (Camera)
- All setter methods log their actions and return `true`

This allows development and testing without building to a device.

---

## Customization

### Custom Gradle Version

The SDK includes a `GradleWrapperPatcher` that automatically patches EDM4U's Gradle wrapper from 5.1.1 to 8.7 for JDK 17 compatibility.

To use a different Gradle version, create a text file at:

```
Assets/SecuritiConsent/gradle-version.txt
```

With just the version number:

```
8.10
```

The patcher will use your specified version instead of the default 8.7.

### Custom Initialization

If you need more control over initialization (instead of the auto-bootstrapping `ConsentManager`), you can initialize the SDK manually:

```csharp
using Securiti.Consent;

public class MyConsentSetup : MonoBehaviour
{
    void Start()
    {
        var sdk = SecuritiConsentSDK.Instance;

        sdk.OnSDKReadyEvent += () =>
        {
            Debug.Log("SDK is ready");
            // Your custom logic here
        };

        sdk.OnErrorEvent += (error) =>
        {
            Debug.LogError("SDK error: " + error);
        };

        var options = new CmpSDKOptions
        {
            AppURL    = "https://your-app.securiti.xyz",
            CdnURL    = "https://cdn.securiti.xyz",
            TenantID  = "your-tenant-id",
            AppID     = "your-app-id",
            TestingMode = false,
            LoggerLevel = LoggerLevel.INFO
        };

        sdk.Initialize(options);
    }

    void OnDestroy()
    {
        var sdk = SecuritiConsentSDK.Instance;
        // Unsubscribe your event handlers
    }
}
```

> If you use custom initialization, ensure the auto-bootstrapping `ConsentManager` doesn't also run.
> You can prevent this by adding a `ConsentManager` component to a scene GameObject -- the `Bootstrap()` method skips creation if one already exists.

---

## Samples

The package includes a **Basic Integration** sample that demonstrates all SDK functionality with a programmatic UI.

### Importing the Sample

1. Open **Window > Package Manager**
2. Select **Securiti Consent SDK**
3. Expand **Samples**
4. Click **Import** next to **Basic Integration**

### Sample Files

| File | Description |
|---|---|
| `SecuritiConsentExample.cs` | Full-featured example with UI buttons for all SDK methods |

The sample creates a runtime UI with buttons for:

- Initialize SDK
- Show Banner
- Show Preferences
- Get Purposes
- Grant Analytics Consent
- Deny Marketing Consent
- Reset Consents
- Clear Log

---

## Troubleshooting

### Android: `NoClassDefFoundError: SecuritiMobileCmpExtensions`

**Cause:** The native Securiti Android SDK was not bundled into the APK.

**Fix:**
1. Ensure EDM4U is active (it is bundled with this package)
2. Go to **Assets > External Dependency Manager > Android Resolver > Resolve**
3. Rebuild

### Android: Gradle `ReflectionCache` / `GroovyBugError`

**Cause:** EDM4U's bundled Gradle wrapper (5.1.1) is incompatible with JDK 17.

**Fix:** The SDK includes `GradleWrapperPatcher` which auto-patches this. If it persists:
1. Verify the patcher is compiling (check console for `[SecuritiConsent] Patched EDM4U Gradle wrapper` log)
2. Or create `Assets/SecuritiConsent/gradle-version.txt` with a compatible version (e.g. `8.7`)
3. In **Unity > Settings > External Tools**, ensure **JDK/SDK/Gradle Installed with Unity** are all checked

### iOS: `dyld: Library not loaded: ConsentUI.framework`

**Cause:** The `ConsentUI.xcframework` was not embedded in the app bundle.

**Fix:**
1. Ensure CocoaPods is installed (`sudo gem install cocoapods`)
2. The `SecuritiConsentBuildProcessor` should handle embedding automatically
3. If the issue persists, manually verify in Xcode that `ConsentUI.xcframework` is listed under **Target > General > Frameworks, Libraries, and Embedded Content** with **Embed & Sign**

### iOS: Build fails with Swift errors

**Cause:** Swift version or deployment target mismatch.

**Fix:** The build processor sets these automatically, but verify in Xcode:
- Swift Language Version: 5.0
- iOS Deployment Target: 15.0+
- Enable Modules (C and Objective-C): YES

### Unity Editor: SDK methods return mock data

**Expected behavior.** The SDK runs in mock mode in the Editor. Build and deploy to a device for real consent flows.

### Package not detected after import

1. Close and reopen Unity
2. Verify `Packages/manifest.json` contains the `ai.securiti.consent` entry
3. Check the Console for any compilation errors

---

## Package Structure

```
ai.securiti.consent/
├── package.json
├── Assets/
│   ├── SecuritiConsent/
│   │   ├── Editor/
│   │   │   ├── SecuritiConsent.Editor.asmdef
│   │   │   ├── SecuritiConsentBuildProcessor.cs   # iOS post-build processor
│   │   │   ├── SecuritiConsentDependencies.xml     # Native dependency declarations
│   │   │   ├── EDM4UChecker.cs                     # Verifies EDM4U is loaded
│   │   │   ├── GradleWrapperPatcher.cs             # Fixes Gradle/JDK compatibility
│   │   │   └── ExternalDependencyManager/          # Bundled EDM4U v1.2.187
│   │   ├── Plugins/
│   │   │   ├── Android/
│   │   │   │   └── securiti-unity-bridge.aar       # Android native bridge
│   │   │   └── iOS/
│   │   │       ├── SecuritiConsentBridge.h
│   │   │       ├── SecuritiConsentBridge.mm
│   │   │       └── SecuritiConsentBridgeImpl.swift
│   │   └── Runtime/
│   │       ├── SecuritiConsent.Runtime.asmdef
│   │       ├── SecuritiConsentSDK.cs               # Main SDK API
│   │       ├── ConsentManager.cs                   # Auto-bootstrapping manager
│   │       ├── Helpers/
│   │       │   └── JsonHelper.cs
│   │       └── Models/
│   │           ├── AppPermission.cs
│   │           ├── BannerConfig.cs
│   │           ├── BannerPosition.cs
│   │           ├── ButtonShape.cs
│   │           ├── CmpSDKOptions.cs
│   │           ├── ComplianceType.cs
│   │           ├── ConsentStatus.cs
│   │           ├── CustomColors.cs
│   │           ├── GcmConfig.cs
│   │           ├── GCMConsent.cs
│   │           ├── GoogleConsentType.cs
│   │           ├── LoggerLevel.cs
│   │           ├── PostConsentsRequest.cs
│   │           ├── Purpose.cs
│   │           ├── SdkInfo.cs
│   │           ├── SDKStatus.cs
│   │           └── SettingsPrompt.cs
│   └── Samples~/
│       └── BasicIntegration/
│           ├── README.md
│           └── SecuritiConsentExample.cs
```

---

## Support

- Website: [https://securiti.ai](https://securiti.ai)
- Documentation: [Securiti Developer Docs](https://securiti.ai/docs)
