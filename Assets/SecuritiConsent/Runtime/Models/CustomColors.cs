using System;
using Newtonsoft.Json;

namespace Securiti.Consent
{
    [Serializable]
    public class CustomColors
    {
        [JsonProperty("button_background")]
        public string ButtonBackground;

        [JsonProperty("button_text")]
        public string ButtonText;

        [JsonProperty("button_border")]
        public string ButtonBorder;

        [JsonProperty("banner_background")]
        public string BannerBackground;

        [JsonProperty("banner_text")]
        public string BannerTextColor;

        [JsonProperty("banner_links")]
        public string BannerLinks;

        [JsonProperty("preference_center_footer_background")]
        public string PreferenceCenterFooterBackground;

        [JsonProperty("preference_center_footer_selector")]
        public string PreferenceCenterFooterSelector;

        [JsonProperty("toggle_primary")]
        public string TogglePrimary;

        [JsonProperty("toggle_secondary")]
        public string ToggleSecondary;

        [JsonProperty("purpose_disclaimer_text")]
        public string PurposeDisclaimerText;

        [JsonProperty("purpose_disclaimer_background")]
        public string PurposeDisclaimerBackground;
    }
}
