#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace Securiti.Consent.Editor
{
    /// <summary>
    /// Post-process build script for iOS.
    ///
    /// Runs AFTER EDM4U / pod install (priority 150 > EDM4U's 99).
    /// CocoaPods already links ConsentUI into UnityFramework, but its
    /// "Embed Pods Frameworks" script phase doesn't run in Unity's build pipeline.
    /// This processor copies the xcframework from Pods/ into the project's
    /// Frameworks/ folder and adds it to the main target's "Embed Frameworks"
    /// build phase so dyld can find it at runtime.
    /// </summary>
    public class SecuritiConsentBuildProcessor
    {
        // Fallback paths if CocoaPods didn't place the xcframework yet
        private static readonly string[] LocalFallbackPaths = new[]
        {
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "Documents", "securiti", "ConsentUI.xcframework"),
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "Documents", "ConsentUI.xcframework"),
        };

        [PostProcessBuild(150)]   // 150 > EDM4U (99) — runs after pod install
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            Debug.Log("[SecuritiConsent] Running iOS post-process build...");

            try
            {
                string projectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
                if (!File.Exists(projectPath))
                {
                    Debug.LogError($"[SecuritiConsent] Xcode project not found at: {projectPath}");
                    return;
                }

                var project = new PBXProject();
                project.ReadFromFile(projectPath);

                string mainTargetGuid      = project.GetUnityMainTargetGuid();
                string frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

                // Apply build settings to both targets
                ConfigureTarget(project, frameworkTargetGuid, "UnityFramework");
                ConfigureTarget(project, mainTargetGuid,      "Unity-iPhone");

                // Embed ConsentUI.xcframework so dyld finds it at runtime
                EmbedConsentUI(project, mainTargetGuid, pathToBuiltProject);

                project.WriteToFile(projectPath);
                Debug.Log("[SecuritiConsent] iOS post-process build completed.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SecuritiConsent] Error during iOS post-process build: {e.Message}\n{e.StackTrace}");
            }
        }

        // ── Embed ConsentUI ───────────────────────────────────────────────────

        private static void EmbedConsentUI(PBXProject project, string mainTargetGuid, string buildPath)
        {
            // 1. Locate the source xcframework
            //    Prefer the CocoaPods-downloaded copy; fall back to the local SDK path.
            string xfSrc = Path.Combine(buildPath, "Pods", "ConsentUI", "ConsentUI.xcframework");
            if (!Directory.Exists(xfSrc))
            {
                xfSrc = FindLocalXCFramework();
                if (xfSrc == null)
                {
                    Debug.LogError(
                        "[SecuritiConsent] ConsentUI.xcframework not found in Pods/ or local fallback paths.\n" +
                        "Expected Pods path: " + Path.Combine(buildPath, "Pods", "ConsentUI", "ConsentUI.xcframework") + "\n" +
                        "The app will crash on launch.");
                    return;
                }
                Debug.Log($"[SecuritiConsent] Pods xcframework not found; using local fallback: {xfSrc}");
            }
            else
            {
                Debug.Log($"[SecuritiConsent] Found xcframework in Pods: {xfSrc}");
            }

            // 2. Copy it into <XcodeProject>/Frameworks/ so the file reference is stable
            string xfDst = Path.Combine(buildPath, "Frameworks", "ConsentUI.xcframework");
            Directory.CreateDirectory(Path.Combine(buildPath, "Frameworks"));
            if (Directory.Exists(xfDst))
                Directory.Delete(xfDst, true);
            CopyDirectory(xfSrc, xfDst);
            Debug.Log($"[SecuritiConsent] Copied xcframework → {xfDst}");

            // 3. Add a file reference inside the Xcode project
            const string relPath = "Frameworks/ConsentUI.xcframework";
            string fileGuid = project.AddFile(relPath, relPath, PBXSourceTree.Source);

            // 4. Embed + sign in the main app target (copies to App.app/Frameworks/ at build time).
            //    CocoaPods already handles linking against UnityFramework, so we only embed here.
            PBXProjectExtensions.AddFileToEmbedFrameworks(project, mainTargetGuid, fileGuid);

            Debug.Log("[SecuritiConsent] ConsentUI.xcframework added to Embed Frameworks phase.");
        }

        private static string FindLocalXCFramework()
        {
            foreach (var path in LocalFallbackPaths)
                if (Directory.Exists(path)) return path;
            return null;
        }

        private static void CopyDirectory(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (string file in Directory.GetFiles(src))
                File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), overwrite: true);
            foreach (string dir in Directory.GetDirectories(src))
                CopyDirectory(dir, Path.Combine(dst, Path.GetFileName(dir)));
        }

        // ── Build settings ────────────────────────────────────────────────────

        private static void ConfigureTarget(PBXProject project, string targetGuid, string targetName)
        {
            project.SetBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");
            project.SetBuildProperty(targetGuid, "CLANG_ENABLE_MODULES", "YES");
            project.SetBuildProperty(targetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

            project.AddBuildProperty(targetGuid, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(targetGuid, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");

            string current = project.GetBuildPropertyForAnyConfig(targetGuid, "IPHONEOS_DEPLOYMENT_TARGET");
            if (string.IsNullOrEmpty(current) || CompareVersions(current, "15.0") < 0)
                project.SetBuildProperty(targetGuid, "IPHONEOS_DEPLOYMENT_TARGET", "15.0");

            Debug.Log($"[SecuritiConsent] Configured target: {targetName}");
        }

        private static int CompareVersions(string v1, string v2)
        {
            try
            {
                var p1 = v1.Split('.');
                var p2 = v2.Split('.');
                int len = System.Math.Max(p1.Length, p2.Length);
                for (int i = 0; i < len; i++)
                {
                    int a = i < p1.Length ? int.Parse(p1[i]) : 0;
                    int b = i < p2.Length ? int.Parse(p2[i]) : 0;
                    if (a < b) return -1;
                    if (a > b) return  1;
                }
                return 0;
            }
            catch { return -1; }
        }
    }
}
#endif
