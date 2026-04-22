using System;
using UnityEngine;

namespace Securiti.Consent
{
    /// <summary>
    /// Configuration options for initializing the Securiti Consent SDK
    /// </summary>
    [Serializable]
    public class CmpSDKOptions
    {
        [Header("Required Configuration")]
        [Tooltip("Base URL for the consent application")]
        public string AppURL;

        [Tooltip("CDN URL for consent assets")]
        public string CdnURL;

        [Tooltip("Securiti tenant identifier")]
        public string TenantID;

        [Tooltip("Application identifier")]
        public string AppID;

        [Header("Optional Configuration")]
        [Tooltip("Enable testing/sandbox mode")]
        public bool TestingMode = false;

        [Tooltip("Logging verbosity level")]
        public LoggerLevel LoggerLevel = LoggerLevel.INFO;

        [Tooltip("Interval for checking consent updates (seconds)")]
        public int ConsentsCheckInterval = 300;

        [Tooltip("Optional subject/user identifier")]
        public string SubjectId = "";

        [Tooltip("Optional language code (e.g., 'en', 'es')")]
        public string LanguageCode = "";

        [Tooltip("Optional location/region code")]
        public string LocationCode = "";

        /// <summary>
        /// Validate that all required fields are set
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AppURL) &&
                   !string.IsNullOrEmpty(CdnURL) &&
                   !string.IsNullOrEmpty(TenantID) &&
                   !string.IsNullOrEmpty(AppID);
        }

        /// <summary>
        /// Get validation error message if configuration is invalid
        /// </summary>
        public string GetValidationError()
        {
            if (string.IsNullOrEmpty(AppURL))
                return "AppURL is required";
            if (string.IsNullOrEmpty(CdnURL))
                return "CdnURL is required";
            if (string.IsNullOrEmpty(TenantID))
                return "TenantID is required";
            if (string.IsNullOrEmpty(AppID))
                return "AppID is required";
            return null;
        }
    }
}
