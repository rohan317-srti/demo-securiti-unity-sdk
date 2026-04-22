using System;

namespace Securiti.Consent
{
    /// <summary>
    /// Represents an app permission (e.g., camera, location)
    /// </summary>
    [Serializable]
    public class AppPermission
    {
        /// <summary>
        /// Unique identifier for the permission
        /// </summary>
        public string id;

        /// <summary>
        /// Display name of the permission
        /// </summary>
        public string name;

        /// <summary>
        /// Detailed description of the permission
        /// </summary>
        public string description;

        /// <summary>
        /// Current consent status for this permission
        /// </summary>
        public int consent; // 0=NOT_DETERMINED, 1=GRANTED, 2=DECLINED, 3=WITHDRAWN

        /// <summary>
        /// String representation of consent status
        /// </summary>
        public string consentString;

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
            return $"AppPermission[id={id}, name={name}, consent={GetConsentStatus()}]";
        }
    }
}
