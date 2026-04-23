#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Securiti.Consent.Editor
{
    /// <summary>
    /// Detects External Dependency Manager for Unity (EDM4U) and blocks
    /// Android/iOS builds when it's missing. EDM4U is required to resolve
    /// the native Securiti Consent SDK from Maven/CocoaPods.
    /// </summary>
    [InitializeOnLoad]
    public class EDM4UChecker : IPreprocessBuildWithReport
    {
        const string EDM4U_DOWNLOAD_URL = "https://github.com/googlesamples/unity-jar-resolver/releases/latest";
        const string EDM4U_OPENUPM_PACKAGE = "com.google.external-dependency-manager";

        static readonly string[] EDM4U_TYPES =
        {
            "Google.VersionHandler, Google.VersionHandlerImpl",
            "Google.JarResolver, Google.JarResolver",
            "GooglePlayServices.PlayServicesResolver, Google.JarResolver"
        };

        public int callbackOrder => 0;

        static EDM4UChecker()
        {
            if (!IsEDM4UInstalled())
                Debug.LogError(BuildMissingMessage(atBuildTime: false));
            else
                Debug.Log("[SecuritiConsent] EDM4U detected — Android/iOS dependency resolution will work.");
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            var target = report.summary.platform;
            if (target != BuildTarget.Android && target != BuildTarget.iOS)
                return;

            if (!IsEDM4UInstalled())
                throw new BuildFailedException(BuildMissingMessage(atBuildTime: true));
        }

        static bool IsEDM4UInstalled()
        {
            foreach (var typeName in EDM4U_TYPES)
                if (System.Type.GetType(typeName) != null)
                    return true;
            return false;
        }

        static string BuildMissingMessage(bool atBuildTime)
        {
            string header = atBuildTime
                ? "[SecuritiConsent] BUILD BLOCKED: External Dependency Manager for Unity (EDM4U) is not installed."
                : "[SecuritiConsent] External Dependency Manager for Unity (EDM4U) is not installed.";

            return header + "\n" +
                "The Securiti Consent SDK requires EDM4U to fetch its native Android (Maven) and iOS (CocoaPods) dependencies.\n" +
                "Without it, Android/iOS builds will compile successfully but crash at runtime with NoClassDefFoundError\n" +
                "or symbol-not-found errors.\n\n" +
                "Install EDM4U one of two ways:\n" +
                "  1. OpenUPM (recommended) — add to Packages/manifest.json:\n" +
                "     \"scopedRegistries\": [{\n" +
                "         \"name\": \"OpenUPM\",\n" +
                "         \"url\": \"https://package.openupm.com\",\n" +
                "         \"scopes\": [\"" + EDM4U_OPENUPM_PACKAGE + "\"]\n" +
                "     }],\n" +
                "     \"dependencies\": {\n" +
                "         \"" + EDM4U_OPENUPM_PACKAGE + "\": \"1.2.187\"\n" +
                "     }\n\n" +
                "  2. Download the .unitypackage from: " + EDM4U_DOWNLOAD_URL + "\n" +
                "     Then: Assets → Import Package → Custom Package → select the downloaded file.";
        }
    }
}
#endif
