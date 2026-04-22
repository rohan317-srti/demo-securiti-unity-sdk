using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Securiti.Consent
{
    [Serializable]
    public class PostConsentsRequest
    {
        [JsonProperty("uuid")]
        public string Uuid;

        [JsonProperty("app_uuid")]
        public string AppUuid;

        [JsonProperty("device")]
        public string Device;

        [JsonProperty("implicit_consent")]
        public bool ImplicitConsent;

        [JsonProperty("version")]
        public int Version;

        [JsonProperty("purpose_consents")]
        public List<PurposeConsent> PurposeConsents;

        [JsonProperty("permissions")]
        public List<PermissionConsentEntry> Permissions;

        [JsonProperty("is_test_mode")]
        public bool IsTestMode;

        [JsonProperty("ad_id")]
        public string AdId;

        [JsonProperty("banner_info")]
        public string BannerInfo;

        [JsonProperty("sdk_version")]
        public string SdkVersion;

        [JsonProperty("platform")]
        public string Platform;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public class PurposeConsent
    {
        [JsonProperty("purpose_id")]
        public int PurposeId;

        [JsonProperty("consent_status")]
        public string ConsentStatusValue;

        [JsonProperty("timestamp")]
        public long Timestamp;

        [JsonProperty("is_essential")]
        public bool IsEssential;
    }

    [Serializable]
    public class PermissionConsentEntry
    {
        [JsonProperty("permission")]
        public string Permission;

        [JsonProperty("consent_status")]
        public string ConsentStatusValue;

        [JsonProperty("timestamp")]
        public long Timestamp;
    }
}
