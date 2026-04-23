using System;
using Newtonsoft.Json;

namespace Securiti.Consent
{
    [Serializable]
    public class GCMConsent
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("consent")]
        public int Consent;

        [JsonProperty("consentString")]
        public string ConsentString;

        public ConsentStatus GetConsentStatus()
        {
            return (ConsentStatus)Consent;
        }
    }
}
