using System.Collections.Generic;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.BlendShapes
{
    [CreateAssetMenu(fileName = "BlendShapesDataSet", menuName = "AR-Renderer/BlendShapeSystem/Settings", order = 1)]
    public class ARBlendShapeSettingsSO : ScriptableObject
    {
        public List<JointPair> JointPairs;
    }
}
