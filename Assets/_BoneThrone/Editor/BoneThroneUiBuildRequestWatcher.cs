using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BoneThrone.Editor
{
    [InitializeOnLoad]
    public static class BoneThroneUiBuildRequestWatcher
    {
        private const string RequestFilePath = "Temp/BoneThroneUiBuild.request";
        private const string StatusFilePath = "Temp/BoneThroneUiBuild.status";

        static BoneThroneUiBuildRequestWatcher()
        {
            EditorApplication.delayCall += TryRunRequestedBuild;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += TryRunRequestedBuild;
            }
        }

        private static void TryRunRequestedBuild()
        {
            string requestFullPath = Path.Combine(Directory.GetCurrentDirectory(), RequestFilePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(requestFullPath))
            {
                return;
            }

            string statusFullPath = Path.Combine(Directory.GetCurrentDirectory(), StatusFilePath.Replace('/', Path.DirectorySeparatorChar));
            try
            {
                File.Delete(requestFullPath);
                File.WriteAllText(statusFullPath, "RUNNING " + DateTime.Now.ToString("s"));
                BoneThroneUiBatchBuilder.BuildAll();
                File.WriteAllText(statusFullPath, "SUCCESS " + DateTime.Now.ToString("s"));
                Debug.Log("BoneThroneUiBuildRequestWatcher completed requested UI build.");
            }
            catch (Exception exception)
            {
                File.WriteAllText(statusFullPath, "FAILED " + DateTime.Now.ToString("s") + Environment.NewLine + exception);
                Debug.LogException(exception);
            }
        }
    }
}
