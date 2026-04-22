using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Securiti.Consent
{
    [Serializable]
    public class SdkInfo
    {
        [JsonProperty("sdk_id")]
        public int? SdkId;

        [JsonProperty("namespace_id")]
        public string NamespaceId;

        [JsonProperty("sdk_name")]
        public Dictionary<string, string> SdkName;

        [JsonProperty("sdk_description")]
        public Dictionary<string, string> SdkDescription;

        [JsonProperty("vendor")]
        public string Vendor;

        [JsonProperty("logo_base64")]
        public string LogoBase64;

        [JsonProperty("website")]
        public string Website;

        [JsonProperty("matched_by")]
        public List<string> MatchedBy;

        [JsonProperty("collecting_data")]
        public bool? CollectingData;

        [JsonProperty("release_date")]
        public string ReleaseDate;

        [JsonProperty("license")]
        public string License;
    }
}
