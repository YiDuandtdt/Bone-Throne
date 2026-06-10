using UnityEditor;
using UnityEngine;

namespace BoneThrone.Editor
{
    /// <summary>
    /// Legacy cleanup for the removed Android input-handling build mutation.
    /// Android builds must keep the project-level Active Input Handling unchanged.
    /// </summary>
    internal static class AndroidBuildInputHandlingGuard
    {
        private const string RestoreActiveKey = "BoneThrone.AndroidBuildInputHandlingGuard.RestoreActive";
        private const string OriginalInputHandlingKey = "BoneThrone.AndroidBuildInputHandlingGuard.OriginalInputHandling";

        [InitializeOnLoadMethod]
        private static void ClearLegacyRestoreState()
        {
            if (!EditorPrefs.GetBool(RestoreActiveKey, false))
            {
                return;
            }

            EditorPrefs.DeleteKey(RestoreActiveKey);
            EditorPrefs.DeleteKey(OriginalInputHandlingKey);
            Debug.Log("[BoneThrone] Cleared legacy Android input handling restore state. Android builds keep Active Input Handling unchanged.");
        }
    }
}
