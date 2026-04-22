using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Securiti.Consent
{
    [Serializable]
    public class SettingsPrompt
    {
        [JsonProperty("prompt_heading")]
        public Dictionary<string, string> PromptHeading;

        [JsonProperty("prompt_message")]
        public Dictionary<string, string> PromptMessage;

        [JsonProperty("settings_button_text")]
        public Dictionary<string, string> SettingsButtonText;

        [JsonProperty("not_now_button_text")]
        public Dictionary<string, string> NotNowButtonText;

        [JsonProperty("permissions")]
        public Dictionary<string, bool> Permissions;
    }
}
