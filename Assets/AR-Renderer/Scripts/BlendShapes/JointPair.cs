using System;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.BlendShapes
{
    [Serializable]
    public class JointPair
    {
        [SerializeField] private int jointFirstIndex;
        [SerializeField] private int jointSecondIndex;
        [SerializeField] private ARBlendShapesType arBlendShapesType;
        [SerializeField] private float CoefficientMul;
        [HideInInspector]public float JointDistanceDefault { get; private set; }
        [HideInInspector]public float JointDistanceMax { get; private set; }
        [HideInInspector]public float JointDistanceCurrent { get; private set; }

        public int JointSecondIndex => jointSecondIndex;

        public int JointFirstIndex => jointFirstIndex;

        public ARBlendShapesType BlendShapesType => arBlendShapesType;

        private bool isInit;
        private float blendShapeCoefficient;
        
        public void Init(Vector3 firstJoint,Vector3 secondJoint)
        {
            JointDistanceDefault = Vector3.Distance(firstJoint,secondJoint);
            JointDistanceMax = (JointDistanceDefault * CoefficientMul)-JointDistanceDefault;
            isInit = true;
        }
        /// <summary>
        /// Recalculation of blendshape coefficient by points.
        /// </summary>
        /// <param name="firstJoint"></param>
        /// <param name="secondJoint"></param>
        /// <returns></returns>
        public float RecalculateCoefficient(Vector3 firstJoint,Vector3 secondJoint)
        {
            if (!isInit)
            {
                Init(firstJoint,secondJoint);
                return 0;
            }
            JointDistanceCurrent = Vector3.Distance(firstJoint,secondJoint)-JointDistanceDefault;
            blendShapeCoefficient = JointDistanceCurrent / JointDistanceMax;
            return blendShapeCoefficient*100f;
        }
       
    }
}