using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Securiti.Consent
{
    [Serializable]
    public class GcmConfig
    {
        [JsonProperty("show_gcm_description")]
        public bool? ShowGcmDescription;

        [JsonProperty("gcm_description")]
        public Dictionary<string, string> GcmDescription;

        [JsonProperty("default_mapping")]
        public Dictionary<string, List<int>> DefaultMapping;

        [JsonProperty("region_overrides")]
        public Dictionary<string, Dictionary<string, List<int>>> RegionOverrides;
    }
}
