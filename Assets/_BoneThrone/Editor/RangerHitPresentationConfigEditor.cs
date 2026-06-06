using BoneThrone.Units;
using UnityEditor;
using UnityEngine;

namespace BoneThrone.Editor
{
    [CustomEditor(typeof(RangerHitPresentationConfig))]
    public sealed class RangerHitPresentationConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Ranger arrow and skill placement should now be tuned with real scene objects from BoneThrone/VFX/Ranger/Create Real Tuning Rig.",
                MessageType.Info);
        }
    }
}
