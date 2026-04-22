using System;

namespace Securiti.Consent
{
    /// <summary>
    /// Represents the consent status for a purpose or permission.
    /// Matches the native SDK ConsentStatus values.
    /// </summary>
    [Serializable]
    public enum ConsentStatus
    {
        NOT_DETERMINED = 0,
        GRANTED = 1,
        DECLINED = 2,
        WITHDRAWN = 3
    }
}
