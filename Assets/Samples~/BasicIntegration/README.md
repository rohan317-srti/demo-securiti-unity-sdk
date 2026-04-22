# Securiti Consent SDK - Basic Integration Example

This example demonstrates basic integration of the Securiti Consent SDK for Unity.

## What This Example Shows

- SDK initialization with configuration options
- Subscribing to SDK events (OnReady, OnError)
- Presenting the consent banner
- Presenting the preference center
- Getting purposes and consent status
- Setting consent for purposes
- Resetting consents

## How to Use

1. Open the `BasicIntegrationScene` scene
2. In the Inspector, configure the `SecuritiConsentExample` component with your credentials:
   - App URL
   - CDN URL
   - Tenant ID
   - App ID
3. Enter Play Mode
4. Use the UI buttons to interact with the SDK:
   - **Initialize**: Initializes the SDK with your configuration
   - **Show Banner**: Displays the consent banner
   - **Show Preferences**: Opens the preference center
   - **Get Purposes**: Retrieves and displays all purposes
   - **Reset Consents**: Resets all consents to default

## Configuration

Replace the placeholder values in the Inspector with your actual Securiti credentials:

```csharp
appURL = "https://your-app-url.securiti.xyz";
cdnURL = "https://cdn-qa.securiti.xyz";
tenantID = "your-tenant-id";
appID = "your-app-id";
```

## Testing

### In Editor
The SDK runs in mock mode in the Unity Editor. All methods will log their calls but won't show real UI.

### On Device
Build for Android or iOS to test with the real native SDK and see actual consent UI.

## Next Steps

- Customize the UI to match your game's style
- Implement consent checking before tracking/analytics
- Add error handling for network issues
- Test on both Android and iOS devices

## Documentation

For complete API documentation, see:
- [API Reference](https://github.com/rohan317-srti/demo-securiti-unity-sdk/blob/main/docs/API_REFERENCE.md)
- [Full README](https://github.com/rohan317-srti/demo-securiti-unity-sdk/blob/main/README.md)
