#if UNITY_ANDROID
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace BoneThrone.EditorTools
{
    [InitializeOnLoad]
    internal static class AndroidExternalToolsAutoConfigurator
    {
        private const string JdkRootPath = @"D:\Program Files\Processing\app\resources\jdk";
        private const string RequiredNdkVersion = "27.2.12479018";
        private const string RequiredAndroidPlatform = "android-36";
        private const string RequiredBuildToolsVersion = "36.0.0";
        private const string RequiredCmakeVersion = "3.22.1";

        static AndroidExternalToolsAutoConfigurator()
        {
            EditorApplication.delayCall += ConfigureIfAndroidBuildTarget;
        }

        [MenuItem("BoneThrone/Android/Configure External Tools")]
        public static void ConfigureExternalToolsFromMenu()
        {
            ConfigureExternalTools(force: true, logSuccess: true);
        }

        private static void ConfigureIfAndroidBuildTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                return;
            }

            ConfigureExternalTools(force: false, logSuccess: false);
        }

        private static void ConfigureExternalTools(bool force, bool logSuccess)
        {
            string sdkRootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Android",
                "Sdk");
            string ndkRootPath = Path.Combine(sdkRootPath, "ndk", RequiredNdkVersion);
            string cmakeRootPath = Path.Combine(sdkRootPath, "cmake", RequiredCmakeVersion);

            if (!HasLocalToolchain(JdkRootPath, sdkRootPath, ndkRootPath, cmakeRootPath, out string reason))
            {
                Debug.LogWarning($"[BoneThrone] Android external tools were not configured: {reason}");
                return;
            }

            if (!force && CurrentExternalToolsAreValid())
            {
                return;
            }

            try
            {
                AndroidExternalToolsSettings.jdkRootPath = JdkRootPath;
                AndroidExternalToolsSettings.sdkRootPath = sdkRootPath;
                AndroidExternalToolsSettings.ndkRootPath = ndkRootPath;

                if (logSuccess)
                {
                    Debug.Log("[BoneThrone] Android External Tools configured for Unity 6000 Android builds.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoneThrone] Failed to configure Android External Tools: {ex.Message}");
            }
        }

        private static bool CurrentExternalToolsAreValid()
        {
            try
            {
                return HasLocalToolchain(
                    AndroidExternalToolsSettings.jdkRootPath,
                    AndroidExternalToolsSettings.sdkRootPath,
                    AndroidExternalToolsSettings.ndkRootPath,
                    Path.Combine(AndroidExternalToolsSettings.sdkRootPath, "cmake", RequiredCmakeVersion),
                    out _);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool HasLocalToolchain(string jdkRootPath, string sdkRootPath, string ndkRootPath, string cmakeRootPath, out string reason)
        {
            if (!File.Exists(Path.Combine(jdkRootPath, "bin", "java.exe")))
            {
                reason = $"JDK 17 was not found at {jdkRootPath}";
                return false;
            }

            if (!File.Exists(Path.Combine(sdkRootPath, "platforms", RequiredAndroidPlatform, "android.jar")))
            {
                reason = $"Android platform {RequiredAndroidPlatform} was not found under {sdkRootPath}";
                return false;
            }

            if (!File.Exists(Path.Combine(sdkRootPath, "build-tools", RequiredBuildToolsVersion, "aapt2.exe")))
            {
                reason = $"Android Build Tools {RequiredBuildToolsVersion} were not found under {sdkRootPath}";
                return false;
            }

            string ndkSourcePropertiesPath = Path.Combine(ndkRootPath, "source.properties");
            if (!File.Exists(ndkSourcePropertiesPath))
            {
                reason = $"NDK {RequiredNdkVersion} was not found at {ndkRootPath}";
                return false;
            }

            string ndkSourceProperties = File.ReadAllText(ndkSourcePropertiesPath);
            if (!ndkSourceProperties.Contains($"Pkg.Revision = {RequiredNdkVersion}", StringComparison.Ordinal))
            {
                reason = $"NDK at {ndkRootPath} is not {RequiredNdkVersion}";
                return false;
            }

            string cmakeSourcePropertiesPath = Path.Combine(cmakeRootPath, "source.properties");
            if (!File.Exists(cmakeSourcePropertiesPath) || !File.Exists(Path.Combine(cmakeRootPath, "bin", "cmake.exe")))
            {
                reason = $"CMake {RequiredCmakeVersion} was not found at {cmakeRootPath}";
                return false;
            }

            string cmakeSourceProperties = File.ReadAllText(cmakeSourcePropertiesPath);
            if (!cmakeSourceProperties.Contains($"Pkg.Revision = {RequiredCmakeVersion}", StringComparison.Ordinal))
            {
                reason = $"CMake at {cmakeRootPath} is not {RequiredCmakeVersion}";
                return false;
            }

            reason = string.Empty;
            return true;
        }
    }
}
#endif
