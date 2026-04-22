using UnityEngine;
using Securiti.Consent;

/// <summary>
/// Bootstraps the Securiti Consent SDK at game start.
/// Auto-creates itself via RuntimeInitializeOnLoadMethod — no scene setup needed.
/// Replace the credential fields below with your real Securiti values.
/// </summary>
public class ConsentManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindObjectOfType<ConsentManager>() != null) return;
        new GameObject("ConsentManager").AddComponent<ConsentManager>();
    }

    [Header("Android Configuration — fill with your Securiti credentials")]
    public string androidAppURL = "";
    public string androidCdnURL = "";
    public string androidTenantID = "";
    public string androidAppID = "";

    [Header("iOS Configuration — fill with your Securiti credentials")]
    public string iosAppURL = "";
    public string iosCdnURL = "";
    public string iosTenantID = "";
    public string iosAppID = "";

    [Header("Options")]
    public bool testingMode = true;
    public LoggerLevel loggerLevel = LoggerLevel.INFO;

    SecuritiConsentSDK _sdk;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        _sdk = SecuritiConsentSDK.Instance;
        _sdk.OnSDKReadyEvent += OnSDKReady;
        _sdk.OnErrorEvent += OnError;

#if UNITY_IOS
        string appURL = iosAppURL;
        string cdnURL = iosCdnURL;
        string tenantID = iosTenantID;
        string appID = iosAppID;
#else
        string appURL = androidAppURL;
        string cdnURL = androidCdnURL;
        string tenantID = androidTenantID;
        string appID = androidAppID;
#endif

        var opts = new CmpSDKOptions
        {
            AppURL = appURL,
            CdnURL = cdnURL,
            TenantID = tenantID,
            AppID = appID,
            TestingMode = testingMode,
            LoggerLevel = loggerLevel
        };

        if (!opts.IsValid())
        {
            Debug.LogError("[ConsentManager] Invalid SDK options: " + opts.GetValidationError());
            return;
        }

        Debug.Log("[ConsentManager] Initializing Securiti SDK (" +
            Application.platform + ")...");
        _sdk.Initialize(opts);
    }

    void OnSDKReady()
    {
        Debug.Log("[ConsentManager] SDK ready — presenting consent banner.");
        _sdk.PresentConsentBanner();
    }

    void OnError(string error)
    {
        Debug.LogError("[ConsentManager] SDK error: " + error);
    }

    void OnDestroy()
    {
        if (_sdk == null) return;
        _sdk.OnSDKReadyEvent -= OnSDKReady;
        _sdk.OnErrorEvent -= OnError;
    }
}
