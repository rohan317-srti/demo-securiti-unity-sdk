using System;

namespace Securiti.Consent
{
    /// <summary>
    /// Google Consent Mode types for Firebase Analytics integration.
    /// Matches native SDK GoogleConsentType.
    /// </summary>
    [Serializable]
    public enum GoogleConsentType
    {
        ANALYTICS_STORAGE,
        AD_STORAGE,
        AD_USER_DATA,
        AD_PERSONALIZATION
    }
}
