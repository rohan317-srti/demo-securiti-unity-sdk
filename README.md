# Securiti Consent SDK for Unity

This is the Unity package for Securiti's consent management platform.

## Installation

This package requires **External Dependency Manager for Unity (EDM4U)** to be installed first.

### Install EDM4U
1. Download from https://github.com/googlesamples/unity-jar-resolver/releases/latest
2. Import the .unitypackage
3. Restart Unity

### Install This Package

#### Via Package Manager (Local)
1. Open Package Manager (Window > Package Manager)
2. Click '+' > "Add package from disk..."
3. Select the `package.json` file

#### Via Git URL
```
https://github.com/rohan317-srti/demo-securiti-unity-sdk.git?path=/unity-package
```

## Quick Start

```csharp
using Securiti.Consent;

var options = new CmpSDKOptions
{
    AppURL = "https://your-app-url.securiti.xyz",
    CdnURL = "https://cdn-qa.securiti.xyz",
    TenantID = "your-tenant-id",
    AppID = "your-app-id",
    TestingMode = true
};

var sdk = SecuritiConsentSDK.Instance;
sdk.OnSDKReadyEvent += () => {
    Debug.Log("SDK Ready!");
    sdk.PresentConsentBanner();
};

sdk.Initialize(options);
```

## Documentation

- [Full README](https://github.com/rohan317-srti/demo-securiti-unity-sdk/blob/main/README.md)
- [API Reference](https://github.com/rohan317-srti/demo-securiti-unity-sdk/blob/main/docs/API_REFERENCE.md)
- [Troubleshooting](https://github.com/rohan317-srti/demo-securiti-unity-sdk/blob/main/docs/TROUBLESHOOTING.md)

## Examples

Import the "Basic Integration" example from Package Manager:
1. Open Package Manager
2. Select "Securiti Consent SDK"
3. Expand "Samples"
4. Click "Import" next to "Basic Integration"

## Support

- GitHub Issues: https://github.com/rohan317-srti/demo-securiti-unity-sdk/issues
- Documentation: https://docs.securiti.ai

## Version

0.1.0

## License

MIT
