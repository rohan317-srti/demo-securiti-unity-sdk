using System;

namespace Securiti.Consent
{
    /// <summary>
    /// Logging verbosity levels for the SDK.
    /// Matches the native SDK log levels.
    /// </summary>
    [Serializable]
    public enum LoggerLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR
    }
}
