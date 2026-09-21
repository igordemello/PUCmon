using Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.Controllers;
using UnityEditor;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Editor
{
    [CustomEditor(typeof(TargetTrackController))]

    public class TargetTrackControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TargetTrackController ttc = (TargetTrackController)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                ttc.CreateScheme();
            }
        }
    }
}
