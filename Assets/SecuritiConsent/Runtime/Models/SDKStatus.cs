using System;

namespace Securiti.Consent
{
    /// <summary>
    /// Represents the SDK initialization status.
    /// Matches the native SDK SDKStatus values.
    /// </summary>
    [Serializable]
    public enum SDKStatus
    {
        AVAILABLE = 0,
        NOT_AVAILABLE = 1,
        IN_PROGRESS = 2
    }
}
