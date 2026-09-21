using Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.BlendShapes;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public class FaceMesh : MonoBehaviour
    {
        public Transform LeftEye { get; private set; }
        public Transform RightEye { get; private set; }

        private Mesh mesh;
        private Vector3[] points;
        private ARBlendShapeController arBlendShapeController;
        public void HideFacemesh(bool state)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = !state;
        }
        public ARBlendShapeController ARBlendShapeController
        {
            set
            {
                arBlendShapeController = value;
            }
        }
        //Set new facemesh vertices
        public void RecalculateFaceMesh(Vector3[] faceMeshData)
        {
            if (mesh == null)
            {
                mesh = GetComponent<MeshFilter>().mesh;
            }
            if (LeftEye == null || RightEye == null)
            {
                LeftEye = new GameObject("LeftEye").transform;
                RightEye = new GameObject("RightEye").transform;

            }

            mesh.vertices = faceMeshData;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            points = mesh.vertices;

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = transform.TransformPoint(points[i]);
            }

            RecalculateLeftEye();
            RecalculateRightEye();

            if (arBlendShapeController == null) return;
            arBlendShapeController.UpdateBlendShapes(faceMeshData);

        }

        /// <summary>
        /// Recalculation of the position of the left eye.
        /// 263 and 362 - indices of the vertices of the edges of the eye according to the canonical face.
        /// </summary>
        private void RecalculateLeftEye()
        {
            var leftEyePosition = (points[263] + points[362]) / 2;
            LeftEye.position = leftEyePosition;
        }

        /// <summary>
        /// Recalculation of the position of the right eye.
        /// 133 and 33 - indices of the vertices of the edges of the eye according to the canonical face.
        /// </summary>
        private void RecalculateRightEye()
        {
            var rightEyePosition = (points[133] + points[33]) / 2;
            RightEye.position = rightEyePosition;
        }
    }
}
