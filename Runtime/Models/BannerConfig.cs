using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Securiti.Consent
{
    [Serializable]
    public class BannerConfig
    {
        [JsonProperty("hide_close_button")]
        public bool? HideCloseButton;

        [JsonProperty("hide_accept_toggle")]
        public bool? HideAcceptButton;

        [JsonProperty("embed_dsr_portal_link")]
        public bool? EmbedDSRPortalLink;

        [JsonProperty("record_consent_upon_app_start")]
        public bool? RecordConsentUponAppStart;

        [JsonProperty("hide_toggle_for_essential_categories")]
        public bool? HideToggleForEssentialCategories;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("dsr_portal_link")]
        public string DsrPortalLink;

        [JsonProperty("compliance_type")]
        public string ComplianceType;

        [JsonProperty("banner_reappearance_time")]
        public string BannerReappearanceTime;

        [JsonProperty("privacy_notice_link")]
        public string PrivacyNoticeLink;

        [JsonProperty("accept")]
        public Dictionary<string, string> Accept;

        [JsonProperty("reject")]
        public Dictionary<string, string> Reject;

        [JsonProperty("banner_text")]
        public Dictionary<string, string> BannerText;

        [JsonProperty("banner_heading")]
        public Dictionary<string, string> BannerHeading;

        [JsonProperty("sdk_tab_heading")]
        public Dictionary<string, string> SdkTabHeading;

        [JsonProperty("privacy_notice_text")]
        public Dictionary<string, string> PrivacyNoticeText;

        [JsonProperty("preference_center_link")]
        public Dictionary<string, string> PreferenceCenterLink;

        [JsonProperty("permissions_tab_heading")]
        public Dictionary<string, string> PermissionsTabHeading;

        [JsonProperty("permissions_tab_guidance")]
        public Dictionary<string, string> PermissionsTabGuidance;

        [JsonProperty("preference_center_heading")]
        public Dictionary<string, string> PreferenceCenterHeading;

        [JsonProperty("preference_center_guidance")]
        public Dictionary<string, string> PreferenceCenterGuidance;

        [JsonProperty("permissions_tab_description")]
        public Dictionary<string, string> PermissionsTabDescription;

        [JsonProperty("preference_center_description")]
        public Dictionary<string, string> PreferenceCenterDescription;

        [JsonProperty("show_powered_by_securiti_logo")]
        public bool? ShowPoweredBySecuritiLogo;

        [JsonProperty("show_description_text_with_preference_center_toggle")]
        public bool? ShowDescriptionTextWithPrefCenterToggle;

        [JsonProperty("palette_theme")]
        public int? PaletteTheme;

        [JsonProperty("banner_position")]
        public string BannerPositionValue;

        [JsonProperty("button_shape")]
        public string ButtonShapeValue;

        [JsonProperty("company_logo")]
        public string CompanyLogo;

        [JsonProperty("palette")]
        public CustomColors CustomPaletteTheme;

        [JsonProperty("should_show_settings_prompt")]
        public bool? ShouldShowSettingsPrompt;

        [JsonProperty("translations")]
        public Dictionary<string, Dictionary<string, string>> Translations;
    }
}
