#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace Securiti.Consent.Editor
{
    /// <summary>
    /// Patches the Gradle wrapper version used by EDM4U's Android Resolver.
    /// EDM4U v1.2.187 ships with Gradle 5.1.1 which is incompatible with JDK 17+.
    ///
    /// Default target version is 8.7. To override, create a file at:
    ///   Assets/SecuritiConsent/gradle-version.txt
    /// containing just the version number, e.g.: 8.10
    /// </summary>
    [InitializeOnLoad]
    public class GradleWrapperPatcher
    {
        private const string DEFAULT_GRADLE_VERSION = "8.7";
        private const string OVERRIDE_FILE = "Assets/SecuritiConsent/gradle-version.txt";
        private static readonly Regex GradleDistRegex =
            new Regex(@"gradle-[\d.]+(-\w+)?-bin\.zip", RegexOptions.Compiled);

        static GradleWrapperPatcher()
        {
            EditorApplication.delayCall += PatchIfNeeded;
        }

        static string GetTargetVersion()
        {
            string overridePath = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, OVERRIDE_FILE);

            if (File.Exists(overridePath))
            {
                string version = File.ReadAllText(overridePath).Trim();
                if (!string.IsNullOrEmpty(version))
                {
                    Debug.Log($"[SecuritiConsent] Using custom Gradle version from {OVERRIDE_FILE}: {version}");
                    return version;
                }
            }

            return DEFAULT_GRADLE_VERSION;
        }

        static void PatchIfNeeded()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            string[] possiblePaths = new[]
            {
                Path.Combine(Application.temporaryCachePath, "..",
                    "Temp", "PlayServicesResolverGradle",
                    "gradle", "wrapper", "gradle-wrapper.properties"),
                Path.Combine(projectRoot,
                    "Temp", "PlayServicesResolverGradle",
                    "gradle", "wrapper", "gradle-wrapper.properties")
            };

            string targetVersion = GetTargetVersion();
            string targetDist = $"gradle-{targetVersion}-bin.zip";

            foreach (string path in possiblePaths)
                PatchFile(path, targetVersion, targetDist);
        }

        static void PatchFile(string path, string targetVersion, string targetDist)
        {
            if (!File.Exists(path)) return;

            string content = File.ReadAllText(path);

            // Already on the target version
            if (content.Contains(targetDist)) return;

            // Replace any gradle-X.Y.Z-bin.zip with the target version
            string patched = GradleDistRegex.Replace(content, targetDist);
            if (patched != content)
            {
                File.WriteAllText(path, patched);
                Debug.Log($"[SecuritiConsent] Patched EDM4U Gradle wrapper -> {targetVersion}");
            }
        }
    }
}
#endif
