using System;

namespace Securiti.Consent
{
    /// <summary>
    /// Represents a consent purpose (e.g., analytics, marketing)
    /// </summary>
    [Serializable]
    public class Purpose
    {
        /// <summary>
        /// Unique identifier for the purpose
        /// </summary>
        public string id;

        /// <summary>
        /// Display name of the purpose
        /// </summary>
        public string name;

        /// <summary>
        /// Detailed description of the purpose
        /// </summary>
        public string description;

        /// <summary>
        /// Current consent status for this purpose
        /// </summary>
        public int consent; // 0=NOT_DETERMINED, 1=GRANTED, 2=DECLINED, 3=WITHDRAWN

        /// <summary>
        /// String representation of consent status
        /// </summary>
        public string consentString;

        /// <summary>
        /// Whether this purpose is required
        /// </summary>
        public bool required;

        /// <summary>
        /// Get the consent status as an enum
        /// </summary>
        public ConsentStatus GetConsentStatus()
        {
            return (ConsentStatus)consent;
        }

        /// <summary>
        /// Set the consent status from an enum
        /// </summary>
        public void SetConsentStatus(ConsentStatus status)
        {
            consent = (int)status;
            consentString = status.ToString();
        }

        public override string ToString()
        {
            return $"Purpose[id={id}, name={name}, consent={GetConsentStatus()}, required={required}]";
        }
    }
}
