using UnityEngine;
using UnityEngine.UI;
using Securiti.Consent;
using System.Text;

/// <summary>
/// Example implementation of Securiti Consent SDK for Unity
/// Demonstrates all core SDK functionality with a simple UI
/// </summary>
public class SecuritiConsentExample : MonoBehaviour
{
    [Header("Securiti Configuration")]
    [Tooltip("Base URL for the consent application")]
    public string appURL = "https://your-app-url.securiti.xyz";

    [Tooltip("CDN URL for consent assets")]
    public string cdnURL = "https://cdn-qa.securiti.xyz";

    [Tooltip("Securiti tenant identifier")]
    public string tenantID = "your-tenant-id";

    [Tooltip("Application identifier")]
    public string appID = "your-app-id";

    [Tooltip("Enable testing/sandbox mode")]
    public bool testingMode = true;

    [Tooltip("Logging verbosity level")]
    public LoggerLevel loggerLevel = LoggerLevel.DEBUG;

    [Header("UI References (Optional - will auto-create if missing)")]
    [Tooltip("Text component to display SDK status")]
    public Text statusText;

    [Tooltip("Text component to display purposes")]
    public Text purposesText;

    [Tooltip("Parent transform for dynamically created buttons")]
    public Transform buttonContainer;

    // SDK reference
    private SecuritiConsentSDK sdk;

    // Status tracking
    private bool isInitialized = false;
    private StringBuilder statusLog = new StringBuilder();

    void Start()
    {
        // Get SDK instance
        sdk = SecuritiConsentSDK.Instance;

        // Subscribe to events
        sdk.OnSDKReadyEvent += OnSDKReady;
        sdk.OnErrorEvent += OnSDKError;
        sdk.OnConsentsResetEvent += OnConsentsReset;

        // Create UI if not manually assigned
        if (buttonContainer == null || statusText == null)
        {
            CreateSimpleUI();
        }

        LogStatus("Securiti Consent SDK Example loaded");
        LogStatus("Configure your credentials in the Inspector and press Initialize");
        LogStatus($"Platform: {GetCurrentPlatform()}");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (sdk != null)
        {
            sdk.OnSDKReadyEvent -= OnSDKReady;
            sdk.OnErrorEvent -= OnSDKError;
            sdk.OnConsentsResetEvent -= OnConsentsReset;
        }
    }

    // ==================== Public Methods (Called by UI Buttons) ====================

    /// <summary>
    /// Initialize the SDK with configured options
    /// </summary>
    public void InitializeSDK()
    {
        LogStatus("Initializing SDK...");

        var options = new CmpSDKOptions
        {
            AppURL = appURL,
            CdnURL = cdnURL,
            TenantID = tenantID,
            AppID = appID,
            TestingMode = testingMode,
            LoggerLevel = loggerLevel,
            ConsentsCheckInterval = 300
        };

        // Validate options
        if (!options.IsValid())
        {
            LogError($"Invalid configuration: {options.GetValidationError()}");
            return;
        }

        // Initialize SDK
        sdk.Initialize(options);
        LogStatus("SDK initialization started...");
    }

    /// <summary>
    /// Present the consent banner
    /// </summary>
    public void ShowBanner()
    {
        if (!CheckInitialized()) return;

        LogStatus("Presenting consent banner...");
        sdk.PresentConsentBanner();
    }

    /// <summary>
    /// Present the preference center
    /// </summary>
    public void ShowPreferences()
    {
        if (!CheckInitialized()) return;

        LogStatus("Presenting preference center...");
        sdk.PresentPreferenceCenter();
    }

    /// <summary>
    /// Get and display all purposes
    /// </summary>
    public void GetPurposes()
    {
        if (!CheckInitialized()) return;

        LogStatus("Fetching purposes...");
        Purpose[] purposes = sdk.GetPurposes();

        if (purposes.Length == 0)
        {
            LogStatus("No purposes found");
            if (purposesText != null)
            {
                purposesText.text = "No purposes available";
            }
            return;
        }

        LogStatus($"Retrieved {purposes.Length} purposes");

        // Display purposes
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Purposes ({purposes.Length}):");
        sb.AppendLine();

        foreach (var purpose in purposes)
        {
            sb.AppendLine($"• {purpose.name}");
            sb.AppendLine($"  ID: {purpose.id}");
            sb.AppendLine($"  Status: {purpose.GetConsentStatus()}");
            sb.AppendLine($"  Required: {purpose.required}");
            sb.AppendLine();
        }

        if (purposesText != null)
        {
            purposesText.text = sb.ToString();
        }
        else
        {
            Debug.Log(sb.ToString());
        }
    }

    /// <summary>
    /// Get and display all permissions
    /// </summary>
    public void GetPermissions()
    {
        if (!CheckInitialized()) return;

        LogStatus("Fetching permissions...");
        AppPermission[] permissions = sdk.GetPermissions();

        if (permissions.Length == 0)
        {
            LogStatus("No permissions found");
            return;
        }

        LogStatus($"Retrieved {permissions.Length} permissions");

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Permissions ({permissions.Length}):");

        foreach (var permission in permissions)
        {
            sb.AppendLine($"• {permission.name} - {permission.GetConsentStatus()}");
        }

        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Example: Grant consent for analytics purpose
    /// </summary>
    public void GrantAnalyticsConsent()
    {
        if (!CheckInitialized()) return;

        LogStatus("Granting analytics consent...");
        sdk.SetPurposeConsent("analytics", ConsentStatus.GIVEN);
        LogStatus("Analytics consent granted");
    }

    /// <summary>
    /// Example: Deny consent for marketing purpose
    /// </summary>
    public void DenyMarketingConsent()
    {
        if (!CheckInitialized()) return;

        LogStatus("Denying marketing consent...");
        sdk.SetPurposeConsent("marketing", ConsentStatus.DENIED);
        LogStatus("Marketing consent denied");
    }

    /// <summary>
    /// Reset all consents to default state
    /// </summary>
    public void ResetAllConsents()
    {
        if (!CheckInitialized()) return;

        LogStatus("Resetting all consents...");
        sdk.ResetConsents();
    }

    /// <summary>
    /// Check if SDK is ready
    /// </summary>
    public void CheckSDKStatus()
    {
        bool ready = sdk.IsSdkReady();
        LogStatus($"SDK Ready: {ready}");
    }

    /// <summary>
    /// Clear the status log
    /// </summary>
    public void ClearLog()
    {
        statusLog.Clear();
        if (statusText != null)
        {
            statusText.text = "";
        }
    }

    // ==================== Event Handlers ====================

    private void OnSDKReady()
    {
        isInitialized = true;
        LogStatus("✓ SDK Ready - You can now use all SDK features");
        LogStatus("Try showing the banner or fetching purposes");
    }

    private void OnSDKError(string error)
    {
        LogError($"SDK Error: {error}");
    }

    private void OnConsentsReset()
    {
        LogStatus("✓ All consents have been reset");
    }

    // ==================== Helper Methods ====================

    private bool CheckInitialized()
    {
        if (!isInitialized)
        {
            LogError("SDK not initialized! Please press Initialize first.");
            return false;
        }
        return true;
    }

    private void LogStatus(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp}] {message}";

        statusLog.Insert(0, logMessage + "\n");

        // Keep only last 20 messages
        int maxLines = 20;
        string[] lines = statusLog.ToString().Split('\n');
        if (lines.Length > maxLines)
        {
            statusLog.Clear();
            for (int i = 0; i < maxLines; i++)
            {
                statusLog.AppendLine(lines[i]);
            }
        }

        if (statusText != null)
        {
            statusText.text = statusLog.ToString();
        }

        Debug.Log($"[SecuritiConsentExample] {message}");
    }

    private void LogError(string message)
    {
        LogStatus($"❌ ERROR: {message}");
    }

    private string GetCurrentPlatform()
    {
#if UNITY_EDITOR
        return "Unity Editor (Mock Mode)";
#elif UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "iOS";
#else
        return "Unsupported Platform";
#endif
    }

    // ==================== Simple UI Creation ====================

    private void CreateSimpleUI()
    {
        Debug.Log("[SecuritiConsentExample] Creating simple UI programmatically");

        // Create Canvas if not present
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create container panel
        GameObject panel = new GameObject("ConsentSDK_Panel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = new Vector2(20, 20);
        panelRect.offsetMax = new Vector2(-20, -20);

        // Create status text
        GameObject statusGO = new GameObject("StatusText");
        statusGO.transform.SetParent(panel.transform, false);
        statusText = statusGO.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusText.fontSize = 14;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperLeft;

        RectTransform statusRect = statusGO.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.4f);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.offsetMin = new Vector2(10, 10);
        statusRect.offsetMax = new Vector2(-10, -10);

        // Create purposes text
        GameObject purposesGO = new GameObject("PurposesText");
        purposesGO.transform.SetParent(panel.transform, false);
        purposesText = purposesGO.AddComponent<Text>();
        purposesText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        purposesText.fontSize = 12;
        purposesText.color = Color.yellow;
        purposesText.alignment = TextAnchor.UpperLeft;

        RectTransform purposesRect = purposesGO.GetComponent<RectTransform>();
        purposesRect.anchorMin = new Vector2(0, 0);
        purposesRect.anchorMax = new Vector2(0.5f, 0.35f);
        purposesRect.offsetMin = new Vector2(10, 10);
        purposesRect.offsetMax = new Vector2(-10, -10);

        // Create button container
        GameObject buttonsGO = new GameObject("Buttons");
        buttonsGO.transform.SetParent(panel.transform, false);
        buttonContainer = buttonsGO.transform;

        RectTransform buttonsRect = buttonsGO.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.5f, 0);
        buttonsRect.anchorMax = new Vector2(1, 0.35f);
        buttonsRect.offsetMin = new Vector2(10, 10);
        buttonsRect.offsetMax = new Vector2(-10, -10);

        // Create buttons
        CreateButton("Initialize SDK", InitializeSDK, 0);
        CreateButton("Show Banner", ShowBanner, 1);
        CreateButton("Show Preferences", ShowPreferences, 2);
        CreateButton("Get Purposes", GetPurposes, 3);
        CreateButton("Grant Analytics", GrantAnalyticsConsent, 4);
        CreateButton("Deny Marketing", DenyMarketingConsent, 5);
        CreateButton("Reset Consents", ResetAllConsents, 6);
        CreateButton("Clear Log", ClearLog, 7);

        LogStatus("UI created - Ready to use");
    }

    private void CreateButton(string label, UnityEngine.Events.UnityAction onClick, int index)
    {
        GameObject buttonGO = new GameObject($"Button_{label}");
        buttonGO.transform.SetParent(buttonContainer, false);

        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        // Add background image
        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.3f, 0.4f);

        // Position button
        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        float height = 35f;
        float spacing = 5f;
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -index * (height + spacing));
        rect.sizeDelta = new Vector2(0, height);

        // Add text label
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        Text text = textGO.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
