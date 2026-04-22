using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;

namespace Securiti.Consent
{
    /// <summary>
    /// Main API for Securiti Consent SDK in Unity
    /// Provides a unified C# interface for Android and iOS native SDKs
    /// </summary>
    public class SecuritiConsentSDK : MonoBehaviour
    {
        private const string BRIDGE_CLASS = "ai.securiti.consent.unity.UnityConsentBridge";

        // Singleton instance
        private static SecuritiConsentSDK _instance;

        // Events
        /// <summary>
        /// Fired when the SDK has completed initialization and is ready to use
        /// </summary>
        public event Action OnSDKReadyEvent;

        /// <summary>
        /// Fired when an error occurs in the SDK
        /// </summary>
        public event Action<string> OnErrorEvent;

        /// <summary>
        /// Fired when consents are reset
        /// </summary>
        public event Action OnConsentsResetEvent;

        /// <summary>
        /// Fired when consents have been uploaded
        /// </summary>
        public event Action<bool> OnConsentsUploadedEvent;

        /// <summary>
        /// Get or create the singleton instance
        /// </summary>
        public static SecuritiConsentSDK Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SecuritiConsentSDK");
                    _instance = go.AddComponent<SecuritiConsentSDK>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ==================== Public API Methods ====================

        /// <summary>
        /// Initialize the Securiti Consent SDK
        /// Must be called before any other SDK methods
        /// </summary>
        /// <param name="options">Configuration options for the SDK</param>
        public void Initialize(CmpSDKOptions options)
        {
            if (options == null)
            {
                LogError("Initialize failed: options is null");
                OnErrorEvent?.Invoke("Options is null");
                return;
            }

            if (!options.IsValid())
            {
                string error = options.GetValidationError();
                LogError($"Initialize failed: {error}");
                OnErrorEvent?.Invoke(error);
                return;
            }

            Log($"Initialize called - AppURL: {options.AppURL}, TenantID: {options.TenantID}");

#if UNITY_ANDROID && !UNITY_EDITOR
            InitializeAndroid(options);
#elif UNITY_IOS && !UNITY_EDITOR
            InitializeIOS(options);
#elif UNITY_EDITOR
            InitializeMock(options);
#else
            LogWarning("Platform not supported");
            OnErrorEvent?.Invoke("Platform not supported");
#endif
        }

        /// <summary>
        /// Present the consent banner to the user
        /// </summary>
        public void PresentConsentBanner()
        {
            Log("PresentConsentBanner called");

#if UNITY_ANDROID && !UNITY_EDITOR
            PresentBannerAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            PresentBannerIOS();
#elif UNITY_EDITOR
            Log("[Mock] Consent banner would be displayed");
#else
            LogWarning("Platform not supported");
#endif
        }

        /// <summary>
        /// Present the preference center where users can manage their consent choices
        /// </summary>
        public void PresentPreferenceCenter()
        {
            Log("PresentPreferenceCenter called");

#if UNITY_ANDROID && !UNITY_EDITOR
            PresentPreferenceCenterAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            PresentPreferenceCenterIOS();
#elif UNITY_EDITOR
            Log("[Mock] Preference center would be displayed");
#else
            LogWarning("Platform not supported");
#endif
        }

        /// <summary>
        /// Check if the SDK is ready (convenience for GetSDKStatus() == AVAILABLE)
        /// </summary>
        public bool IsSdkReady()
        {
            return GetSDKStatus() == SDKStatus.AVAILABLE;
        }

        /// <summary>
        /// Get the current SDK status
        /// </summary>
        /// <returns>SDK status (AVAILABLE, NOT_AVAILABLE, or IN_PROGRESS)</returns>
        public SDKStatus GetSDKStatus()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return GetSDKStatusAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            return GetSDKStatusIOS();
#elif UNITY_EDITOR
            return SDKStatus.AVAILABLE; // Mock mode always available
#else
            return SDKStatus.NOT_AVAILABLE;
#endif
        }

        /// <summary>
        /// Get all purposes with their current consent status
        /// </summary>
        /// <returns>Array of purposes</returns>
        public Purpose[] GetPurposes()
        {
            Log("GetPurposes called");

#if UNITY_ANDROID && !UNITY_EDITOR
            return GetPurposesAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            return GetPurposesIOS();
#elif UNITY_EDITOR
            return GetMockPurposes();
#else
            return new Purpose[0];
#endif
        }

        /// <summary>
        /// Get all permissions with their current consent status
        /// </summary>
        /// <returns>Array of permissions</returns>
        public AppPermission[] GetPermissions()
        {
            Log("GetPermissions called");

#if UNITY_ANDROID && !UNITY_EDITOR
            return GetPermissionsAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            return GetPermissionsIOS();
#elif UNITY_EDITOR
            return GetMockPermissions();
#else
            return new AppPermission[0];
#endif
        }

        /// <summary>
        /// Get the consent status for a specific purpose
        /// </summary>
        /// <param name="purposeId">The purpose identifier</param>
        /// <returns>Consent status</returns>
        public ConsentStatus GetConsentByPurposeId(string purposeId)
        {
            if (string.IsNullOrEmpty(purposeId))
            {
                LogWarning("GetConsentByPurposeId: purposeId is null or empty");
                return ConsentStatus.NOT_DETERMINED;
            }

            Log($"GetConsentByPurposeId called for: {purposeId}");

#if UNITY_ANDROID && !UNITY_EDITOR
            return GetConsentByPurposeIdAndroid(purposeId);
#elif UNITY_IOS && !UNITY_EDITOR
            return GetConsentByPurposeIdIOS(purposeId);
#elif UNITY_EDITOR
            return ConsentStatus.NOT_DETERMINED; // Mock
#else
            return ConsentStatus.NOT_DETERMINED;
#endif
        }

        /// <summary>
        /// Set consent for a specific purpose
        /// </summary>
        /// <param name="purposeId">The purpose identifier</param>
        /// <param name="status">The consent status to set</param>
        /// <returns>True if the consent was saved successfully</returns>
        public bool SetPurposeConsent(string purposeId, ConsentStatus status)
        {
            if (string.IsNullOrEmpty(purposeId))
            {
                LogWarning("SetPurposeConsent: purposeId is null or empty");
                return false;
            }

            Log($"SetPurposeConsent called for: {purposeId}, status: {status}");

#if UNITY_ANDROID && !UNITY_EDITOR
            return SetPurposeConsentAndroid(purposeId, status);
#elif UNITY_IOS && !UNITY_EDITOR
            return SetPurposeConsentIOS(purposeId, status);
#elif UNITY_EDITOR
            Log($"[Mock] Set consent for {purposeId} to {status}");
            return true;
#else
            LogWarning("Platform not supported");
            return false;
#endif
        }

        /// <summary>
        /// Set consent for a specific permission
        /// </summary>
        /// <param name="permissionId">The permission identifier</param>
        /// <param name="status">The consent status to set</param>
        /// <returns>True if the consent was saved successfully</returns>
        public bool SetPermissionConsent(string permissionId, ConsentStatus status)
        {
            if (string.IsNullOrEmpty(permissionId))
            {
                LogWarning("SetPermissionConsent: permissionId is null or empty");
                return false;
            }

            Log($"SetPermissionConsent called for: {permissionId}, status: {status}");

#if UNITY_ANDROID && !UNITY_EDITOR
            return SetPermissionConsentAndroid(permissionId, status);
#elif UNITY_IOS && !UNITY_EDITOR
            return SetPermissionConsentIOS(permissionId, status);
#elif UNITY_EDITOR
            Log($"[Mock] Set permission consent for {permissionId} to {status}");
            return true;
#else
            LogWarning("Platform not supported");
            return false;
#endif
        }

        /// <summary>
        /// Reset all consents to their default state
        /// </summary>
        public void ResetConsents()
        {
            Log("ResetConsents called");

#if UNITY_ANDROID && !UNITY_EDITOR
            ResetConsentsAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            ResetConsentsIOS();
#elif UNITY_EDITOR
            Log("[Mock] All consents reset");
            OnConsentsResetEvent?.Invoke();
#else
            LogWarning("Platform not supported");
#endif
        }

        /// <summary>
        /// Get the consent status for a specific permission
        /// </summary>
        /// <param name="permissionId">The permission identifier</param>
        /// <returns>Consent status</returns>
        public ConsentStatus GetConsentByPermissionId(string permissionId)
        {
            if (string.IsNullOrEmpty(permissionId))
            {
                LogWarning("GetConsentByPermissionId: permissionId is null or empty");
                return ConsentStatus.NOT_DETERMINED;
            }

            Log($"GetConsentByPermissionId called for: {permissionId}");

#if UNITY_ANDROID && !UNITY_EDITOR
            return GetConsentByPermissionIdAndroid(permissionId);
#elif UNITY_IOS && !UNITY_EDITOR
            return GetConsentByPermissionIdIOS(permissionId);
#elif UNITY_EDITOR
            return ConsentStatus.NOT_DETERMINED;
#else
            return ConsentStatus.NOT_DETERMINED;
#endif
        }

        /// <summary>
        /// Get Google Consent Mode consent statuses.
        /// Each entry maps a consent type (e.g. ad_storage) to its aggregated consent status.
        /// </summary>
        public GCMConsent[] GetGCMConsents()
        {
            Log("GetGCMConsents called");

#if UNITY_ANDROID && !UNITY_EDITOR
            return GetGCMConsentsAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            return GetGCMConsentsIOS();
#elif UNITY_EDITOR
            return new GCMConsent[0];
#else
            return new GCMConsent[0];
#endif
        }

        /// <summary>
        /// Get the Google Consent Mode configuration
        /// </summary>
        public GcmConfig GetGCMConfig()
        {
            Log("GetGCMConfig called");

            string json = "{}";
#if UNITY_ANDROID && !UNITY_EDITOR
            json = GetGCMConfigAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            json = GetGCMConfigIOS();
#endif
            try { return JsonConvert.DeserializeObject<GcmConfig>(json); }
            catch (Exception e) { LogError($"Failed to parse GCMConfig: {e.Message}"); return null; }
        }

        /// <summary>
        /// Get the banner configuration
        /// </summary>
        public BannerConfig GetBannerConfig()
        {
            Log("GetBannerConfig called");

            string json = "{}";
#if UNITY_ANDROID && !UNITY_EDITOR
            json = GetBannerConfigAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            json = GetBannerConfigIOS();
#endif
            try { return JsonConvert.DeserializeObject<BannerConfig>(json); }
            catch (Exception e) { LogError($"Failed to parse BannerConfig: {e.Message}"); return null; }
        }

        /// <summary>
        /// Get the settings prompt configuration
        /// </summary>
        public SettingsPrompt GetSettingsPrompt()
        {
            Log("GetSettingsPrompt called");

            string json = "{}";
#if UNITY_ANDROID && !UNITY_EDITOR
            json = GetSettingsPromptAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
            json = GetSettingsPromptIOS();
#endif
            try { return JsonConvert.DeserializeObject<SettingsPrompt>(json); }
            catch (Exception e) { LogError($"Failed to parse SettingsPrompt: {e.Message}"); return null; }
        }

        /// <summary>
        /// Get the SDKs associated with a purpose (Android only)
        /// </summary>
        public SdkInfo[] GetSdksInPurpose(string purposeId)
        {
            Log($"GetSdksInPurpose called for: {purposeId}");

            string json = "[]";
#if UNITY_ANDROID && !UNITY_EDITOR
            json = GetSdksInPurposeAndroid(purposeId);
#else
            LogWarning("GetSdksInPurpose is only available on Android");
#endif
            try { return JsonConvert.DeserializeObject<SdkInfo[]>(json); }
            catch (Exception e) { LogError($"Failed to parse SdkInfo: {e.Message}"); return new SdkInfo[0]; }
        }

        /// <summary>
        /// Upload consents to the server
        /// </summary>
        /// <param name="request">The consent upload request</param>
        public void UploadConsents(PostConsentsRequest request)
        {
            if (request == null)
            {
                LogWarning("UploadConsents: request is null");
                return;
            }

            Log("UploadConsents called");
            string json = JsonConvert.SerializeObject(request);

#if UNITY_ANDROID && !UNITY_EDITOR
            UploadConsentsAndroid(json);
#elif UNITY_IOS && !UNITY_EDITOR
            UploadConsentsIOS(json);
#elif UNITY_EDITOR
            Log("[Mock] Consents uploaded");
            OnConsentsUploadedEvent?.Invoke(true);
#else
            LogWarning("Platform not supported");
#endif
        }

        /// <summary>
        /// Remove all SDK listeners (iOS only)
        /// </summary>
        public void RemoveListeners()
        {
            Log("RemoveListeners called");

#if UNITY_IOS && !UNITY_EDITOR
            _RemoveListeners();
#else
            Log("RemoveListeners: no-op on this platform");
#endif
        }

        // ==================== Callback Methods (Called by Native via UnitySendMessage) ====================

        /// <summary>
        /// Called by native SDK when it's ready
        /// Method name must match what native sends via UnitySendMessage
        /// </summary>
        private void OnSDKReady(string json)
        {
            Log($"OnSDKReady callback received: {json}");
            OnSDKReadyEvent?.Invoke();
        }

        /// <summary>
        /// Called by native SDK when an error occurs
        /// Method name must match what native sends via UnitySendMessage
        /// </summary>
        private void OnError(string json)
        {
            LogError($"OnError callback received: {json}");

            try
            {
                var dict = JsonHelper.ParseJsonObject(json);
                string errorMessage = dict.ContainsKey("error") ? dict["error"] : "Unknown error";
                OnErrorEvent?.Invoke(errorMessage);
            }
            catch (Exception e)
            {
                LogError($"Failed to parse error JSON: {e.Message}");
                OnErrorEvent?.Invoke("Unknown error");
            }
        }

        /// <summary>
        /// Called by native SDK when consents are reset
        /// Method name must match what native sends via UnitySendMessage
        /// </summary>
        private void OnConsentsReset(string json)
        {
            Log($"OnConsentsReset callback received: {json}");
            OnConsentsResetEvent?.Invoke();
        }

        /// <summary>
        /// Called by native SDK when consents have been uploaded
        /// </summary>
        private void OnConsentsUploaded(string json)
        {
            Log($"OnConsentsUploaded callback received: {json}");
            var dict = JsonHelper.ParseJsonObject(json);
            bool success = dict.ContainsKey("success") && dict["success"] == "true";
            OnConsentsUploadedEvent?.Invoke(success);
        }

        // ==================== Android Implementation ====================

#if UNITY_ANDROID && !UNITY_EDITOR

        private void InitializeAndroid(CmpSDKOptions options)
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    bridgeClass.CallStatic("initialize",
                        options.AppURL,
                        options.CdnURL,
                        options.TenantID,
                        options.AppID,
                        options.TestingMode,
                        options.LoggerLevel.ToString(),
                        options.ConsentsCheckInterval,
                        string.IsNullOrEmpty(options.SubjectId) ? null : options.SubjectId,
                        string.IsNullOrEmpty(options.LanguageCode) ? null : options.LanguageCode,
                        string.IsNullOrEmpty(options.LocationCode) ? null : options.LocationCode
                    );
                }
                Log("Android SDK initialization started");
            }
            catch (Exception e)
            {
                LogError($"Android initialize failed: {e.Message}");
                OnErrorEvent?.Invoke($"Initialize failed: {e.Message}");
            }
        }

        private void PresentBannerAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    bridgeClass.CallStatic("presentConsentBanner");
                }
            }
            catch (Exception e)
            {
                LogError($"Android presentBanner failed: {e.Message}");
                OnErrorEvent?.Invoke($"Banner error: {e.Message}");
            }
        }

        private void PresentPreferenceCenterAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    bridgeClass.CallStatic("presentPreferenceCenter");
                }
            }
            catch (Exception e)
            {
                LogError($"Android presentPreferenceCenter failed: {e.Message}");
                OnErrorEvent?.Invoke($"Preference center error: {e.Message}");
            }
        }

        private SDKStatus GetSDKStatusAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    int status = bridgeClass.CallStatic<int>("getSDKStatus");
                    return (SDKStatus)status;
                }
            }
            catch (Exception e)
            {
                LogError($"Android getSDKStatus failed: {e.Message}");
                return SDKStatus.NOT_AVAILABLE;
            }
        }

        private Purpose[] GetPurposesAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    string json = bridgeClass.CallStatic<string>("getPurposes");
                    Log($"Received purposes JSON: {json}");
                    return JsonHelper.FromJsonArray<Purpose>(json);
                }
            }
            catch (Exception e)
            {
                LogError($"Android getPurposes failed: {e.Message}");
                return new Purpose[0];
            }
        }

        private AppPermission[] GetPermissionsAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    string json = bridgeClass.CallStatic<string>("getPermissions");
                    Log($"Received permissions JSON: {json}");
                    return JsonHelper.FromJsonArray<AppPermission>(json);
                }
            }
            catch (Exception e)
            {
                LogError($"Android getPermissions failed: {e.Message}");
                return new AppPermission[0];
            }
        }

        private ConsentStatus GetConsentByPurposeIdAndroid(string purposeId)
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    int statusInt = bridgeClass.CallStatic<int>("getConsentByPurposeId", purposeId);
                    return (ConsentStatus)statusInt;
                }
            }
            catch (Exception e)
            {
                LogError($"Android getConsentByPurposeId failed: {e.Message}");
                return ConsentStatus.NOT_DETERMINED;
            }
        }

        private bool SetPurposeConsentAndroid(string purposeId, ConsentStatus status)
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    return bridgeClass.CallStatic<bool>("setPurposeConsent", purposeId, (int)status);
                }
            }
            catch (Exception e)
            {
                LogError($"Android setPurposeConsent failed: {e.Message}");
                OnErrorEvent?.Invoke($"Set consent failed: {e.Message}");
                return false;
            }
        }

        private bool SetPermissionConsentAndroid(string permissionId, ConsentStatus status)
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    return bridgeClass.CallStatic<bool>("setPermissionConsent", permissionId, (int)status);
                }
            }
            catch (Exception e)
            {
                LogError($"Android setPermissionConsent failed: {e.Message}");
                OnErrorEvent?.Invoke($"Set permission consent failed: {e.Message}");
                return false;
            }
        }

        private void ResetConsentsAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    bridgeClass.CallStatic("resetConsents");
                }
            }
            catch (Exception e)
            {
                LogError($"Android resetConsents failed: {e.Message}");
                OnErrorEvent?.Invoke($"Reset consents failed: {e.Message}");
            }
        }

        private ConsentStatus GetConsentByPermissionIdAndroid(string permissionId)
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    int statusInt = bridgeClass.CallStatic<int>("getConsentByPermissionId", permissionId);
                    return (ConsentStatus)statusInt;
                }
            }
            catch (Exception e)
            {
                LogError($"Android getConsentByPermissionId failed: {e.Message}");
                return ConsentStatus.NOT_DETERMINED;
            }
        }

        private GCMConsent[] GetGCMConsentsAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    string json = bridgeClass.CallStatic<string>("getGCMConsents");
                    return JsonConvert.DeserializeObject<GCMConsent[]>(json) ?? new GCMConsent[0];
                }
            }
            catch (Exception e)
            {
                LogError($"Android getGCMConsents failed: {e.Message}");
                return new GCMConsent[0];
            }
        }

        private string GetGCMConfigAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    return bridgeClass.CallStatic<string>("getGCMConfig");
                }
            }
            catch (Exception e)
            {
                LogError($"Android getGCMConfig failed: {e.Message}");
                return "{}";
            }
        }

        private string GetBannerConfigAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    return bridgeClass.CallStatic<string>("getBannerConfig");
                }
            }
            catch (Exception e)
            {
                LogError($"Android getBannerConfig failed: {e.Message}");
                return "{}";
            }
        }

        private string GetSettingsPromptAndroid()
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    return bridgeClass.CallStatic<string>("getSettingsPrompt");
                }
            }
            catch (Exception e)
            {
                LogError($"Android getSettingsPrompt failed: {e.Message}");
                return "{}";
            }
        }

        private string GetSdksInPurposeAndroid(string purposeId)
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    return bridgeClass.CallStatic<string>("getSdksInPurpose", purposeId);
                }
            }
            catch (Exception e)
            {
                LogError($"Android getSdksInPurpose failed: {e.Message}");
                return "[]";
            }
        }

        private void UploadConsentsAndroid(string jsonRequest)
        {
            try
            {
                using (AndroidJavaClass bridgeClass = new AndroidJavaClass(BRIDGE_CLASS))
                {
                    bridgeClass.CallStatic("uploadConsents", jsonRequest);
                }
            }
            catch (Exception e)
            {
                LogError($"Android uploadConsents failed: {e.Message}");
                OnErrorEvent?.Invoke($"Upload consents failed: {e.Message}");
            }
        }

#endif

        // ==================== iOS Implementation ====================

#if UNITY_IOS && !UNITY_EDITOR

        [DllImport("__Internal")]
        private static extern void _InitializeConsentSDK(
            string appURL, string cdnURL, string tenantID, string appID,
            bool testingMode, string loggerLevel, int consentsCheckInterval,
            string subjectId, string languageCode, string locationCode);

        [DllImport("__Internal")]
        private static extern void _PresentConsentBanner();

        [DllImport("__Internal")]
        private static extern void _PresentPreferenceCenter();

        [DllImport("__Internal")]
        private static extern int _GetSDKStatus();

        [DllImport("__Internal")]
        private static extern string _GetPurposes();

        [DllImport("__Internal")]
        private static extern string _GetPermissions();

        [DllImport("__Internal")]
        private static extern int _GetConsentByPurposeId(string purposeId);

        [DllImport("__Internal")]
        private static extern bool _SetPurposeConsent(string purposeId, int consentStatus);

        [DllImport("__Internal")]
        private static extern bool _SetPermissionConsent(string permissionId, int consentStatus);

        [DllImport("__Internal")]
        private static extern void _ResetConsents();

        [DllImport("__Internal")]
        private static extern int _GetConsentByPermissionId(string permissionId);

        [DllImport("__Internal")]
        private static extern string _GetGCMConsents();

        [DllImport("__Internal")]
        private static extern string _GetGCMConfig();

        [DllImport("__Internal")]
        private static extern string _GetBannerConfig();

        [DllImport("__Internal")]
        private static extern string _GetSettingsPrompt();

        [DllImport("__Internal")]
        private static extern void _UploadConsents(string jsonRequest);

        [DllImport("__Internal")]
        private static extern void _RemoveListeners();

        private void InitializeIOS(CmpSDKOptions options)
        {
            try
            {
                _InitializeConsentSDK(
                    options.AppURL,
                    options.CdnURL,
                    options.TenantID,
                    options.AppID,
                    options.TestingMode,
                    options.LoggerLevel.ToString(),
                    options.ConsentsCheckInterval,
                    options.SubjectId,
                    options.LanguageCode,
                    options.LocationCode
                );
                Log("iOS SDK initialization started");
            }
            catch (Exception e)
            {
                LogError($"iOS initialize failed: {e.Message}");
                OnErrorEvent?.Invoke($"Initialize failed: {e.Message}");
            }
        }

        private void PresentBannerIOS()
        {
            try
            {
                _PresentConsentBanner();
            }
            catch (Exception e)
            {
                LogError($"iOS presentBanner failed: {e.Message}");
                OnErrorEvent?.Invoke($"Banner error: {e.Message}");
            }
        }

        private void PresentPreferenceCenterIOS()
        {
            try
            {
                _PresentPreferenceCenter();
            }
            catch (Exception e)
            {
                LogError($"iOS presentPreferenceCenter failed: {e.Message}");
                OnErrorEvent?.Invoke($"Preference center error: {e.Message}");
            }
        }

        private SDKStatus GetSDKStatusIOS()
        {
            try
            {
                int status = _GetSDKStatus();
                return (SDKStatus)status;
            }
            catch (Exception e)
            {
                LogError($"iOS getSDKStatus failed: {e.Message}");
                return SDKStatus.NOT_AVAILABLE;
            }
        }

        private Purpose[] GetPurposesIOS()
        {
            try
            {
                string json = _GetPurposes();
                Log($"Received purposes JSON: {json}");
                return JsonHelper.FromJsonArray<Purpose>(json);
            }
            catch (Exception e)
            {
                LogError($"iOS getPurposes failed: {e.Message}");
                return new Purpose[0];
            }
        }

        private AppPermission[] GetPermissionsIOS()
        {
            try
            {
                string json = _GetPermissions();
                Log($"Received permissions JSON: {json}");
                return JsonHelper.FromJsonArray<AppPermission>(json);
            }
            catch (Exception e)
            {
                LogError($"iOS getPermissions failed: {e.Message}");
                return new AppPermission[0];
            }
        }

        private ConsentStatus GetConsentByPurposeIdIOS(string purposeId)
        {
            try
            {
                int statusInt = _GetConsentByPurposeId(purposeId);
                return (ConsentStatus)statusInt;
            }
            catch (Exception e)
            {
                LogError($"iOS getConsentByPurposeId failed: {e.Message}");
                return ConsentStatus.NOT_DETERMINED;
            }
        }

        private bool SetPurposeConsentIOS(string purposeId, ConsentStatus status)
        {
            try
            {
                return _SetPurposeConsent(purposeId, (int)status);
            }
            catch (Exception e)
            {
                LogError($"iOS setPurposeConsent failed: {e.Message}");
                OnErrorEvent?.Invoke($"Set consent failed: {e.Message}");
                return false;
            }
        }

        private bool SetPermissionConsentIOS(string permissionId, ConsentStatus status)
        {
            try
            {
                return _SetPermissionConsent(permissionId, (int)status);
            }
            catch (Exception e)
            {
                LogError($"iOS setPermissionConsent failed: {e.Message}");
                OnErrorEvent?.Invoke($"Set permission consent failed: {e.Message}");
                return false;
            }
        }

        private void ResetConsentsIOS()
        {
            try
            {
                _ResetConsents();
            }
            catch (Exception e)
            {
                LogError($"iOS resetConsents failed: {e.Message}");
                OnErrorEvent?.Invoke($"Reset consents failed: {e.Message}");
            }
        }

        private ConsentStatus GetConsentByPermissionIdIOS(string permissionId)
        {
            try
            {
                int statusInt = _GetConsentByPermissionId(permissionId);
                return (ConsentStatus)statusInt;
            }
            catch (Exception e)
            {
                LogError($"iOS getConsentByPermissionId failed: {e.Message}");
                return ConsentStatus.NOT_DETERMINED;
            }
        }

        private GCMConsent[] GetGCMConsentsIOS()
        {
            try
            {
                string json = _GetGCMConsents();
                return JsonConvert.DeserializeObject<GCMConsent[]>(json) ?? new GCMConsent[0];
            }
            catch (Exception e)
            {
                LogError($"iOS getGCMConsents failed: {e.Message}");
                return new GCMConsent[0];
            }
        }

        private string GetGCMConfigIOS()
        {
            try
            {
                return _GetGCMConfig();
            }
            catch (Exception e)
            {
                LogError($"iOS getGCMConfig failed: {e.Message}");
                return "{}";
            }
        }

        private string GetBannerConfigIOS()
        {
            try
            {
                return _GetBannerConfig();
            }
            catch (Exception e)
            {
                LogError($"iOS getBannerConfig failed: {e.Message}");
                return "{}";
            }
        }

        private string GetSettingsPromptIOS()
        {
            try
            {
                return _GetSettingsPrompt();
            }
            catch (Exception e)
            {
                LogError($"iOS getSettingsPrompt failed: {e.Message}");
                return "{}";
            }
        }

        private void UploadConsentsIOS(string jsonRequest)
        {
            try
            {
                _UploadConsents(jsonRequest);
            }
            catch (Exception e)
            {
                LogError($"iOS uploadConsents failed: {e.Message}");
                OnErrorEvent?.Invoke($"Upload consents failed: {e.Message}");
            }
        }

#endif

        // ==================== Editor Mock Mode ====================

#if UNITY_EDITOR

        private void InitializeMock(CmpSDKOptions options)
        {
            Log("[Mock] SDK initialized successfully");
            Log($"[Mock] AppURL: {options.AppURL}");
            Log($"[Mock] TenantID: {options.TenantID}");
            Log($"[Mock] TestingMode: {options.TestingMode}");

            // Simulate async initialization
            Invoke(nameof(FireMockReady), 0.5f);
        }

        private void FireMockReady()
        {
            Log("[Mock] SDK ready callback");
            OnSDKReadyEvent?.Invoke();
        }

        private Purpose[] GetMockPurposes()
        {
            return new Purpose[]
            {
                new Purpose
                {
                    id = "analytics",
                    name = "Analytics",
                    description = "Collect usage analytics",
                    consent = (int)ConsentStatus.NOT_DETERMINED,
                    consentString = "NOT_DETERMINED",
                    required = false
                },
                new Purpose
                {
                    id = "marketing",
                    name = "Marketing",
                    description = "Personalized marketing",
                    consent = (int)ConsentStatus.NOT_DETERMINED,
                    consentString = "NOT_DETERMINED",
                    required = false
                }
            };
        }

        private AppPermission[] GetMockPermissions()
        {
            return new AppPermission[]
            {
                new AppPermission
                {
                    id = "camera",
                    name = "Camera",
                    description = "Access camera for photos",
                    consent = (int)ConsentStatus.NOT_DETERMINED,
                    consentString = "NOT_DETERMINED"
                }
            };
        }

#endif

        // ==================== Logging Helpers ====================

        private void Log(string message)
        {
            Debug.Log($"[SecuritiConsent] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SecuritiConsent] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SecuritiConsent] {message}");
        }
    }
}
