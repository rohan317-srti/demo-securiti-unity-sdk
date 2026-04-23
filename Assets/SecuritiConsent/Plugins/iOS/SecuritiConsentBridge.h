#ifndef SecuritiConsentBridge_h
#define SecuritiConsentBridge_h

#ifdef __cplusplus
extern "C" {
#endif

// Initialize the Securiti Consent SDK
void _InitializeConsentSDK(
    const char* appURL,
    const char* cdnURL,
    const char* tenantID,
    const char* appID,
    bool testingMode,
    const char* loggerLevel,
    int consentsCheckInterval,
    const char* subjectId,
    const char* languageCode,
    const char* locationCode
);

// Present the consent banner
void _PresentConsentBanner();

// Present the preference center
void _PresentPreferenceCenter();

// Check if SDK is ready
bool _IsSdkReady();

// Get all purposes as JSON string
const char* _GetPurposes();

// Get all permissions as JSON string
const char* _GetPermissions();

// Get consent status for a specific purpose
int _GetConsentByPurposeId(const char* purposeId);

// Set consent for a purpose
void _SetPurposeConsent(const char* purposeId, int consentStatus);

// Set consent for a permission
void _SetPermissionConsent(const char* permissionId, int consentStatus);

// Reset all consents
void _ResetConsents();

#ifdef __cplusplus
}
#endif

#endif /* SecuritiConsentBridge_h */
