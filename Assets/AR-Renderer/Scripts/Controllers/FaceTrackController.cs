using System.Runtime.InteropServices;
using Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.BlendShapes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.Controllers
{
    public class FaceTrackController : MonoBehaviour
    {
        [DllImport("__Internal")]
        protected static extern int LoadScene(string message, string index, string targetImg);
        const float SCALE_FACTOR = 0.115f;

        [SerializeField] private FaceMesh faceMesh;
        [SerializeField] private bool hideFacemesh = true;
        [SerializeField] private ARBlendShapeController arBlendShapeController;
        [SerializeField] private bool useBlendShapes;
        [SerializeField] private bool alwaysDisplayFacemesh = false;
        [SerializeField] private bool fixedCameraFieldOfView = false;
        [SerializeField] private bool displayWebCamBackground;
        [SerializeField] private Transform scene;
        [SerializeField] private Camera cam;
        [SerializeField] private Constraints movementConstraints;
        [SerializeField] private Constraints rotationConstraints;
        private ArBackground arBackground;
        private void OnEnable()
        {
            Receiver.Activate();
            EventBus.onFaceTracking += Tracking;
        }

        private void OnDisable()
        {
            //Send message to frontend to stop frontend`s face recognition scene.
            LoadScene("stop", null, null);
            EventBus.onFaceTracking -= Tracking;
        }

        private void Start()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;

            //Send message to frontend to start frontend`s face recognition scene.
            LoadScene("face", sceneIndex.ToString(), null);

            faceMesh.HideFacemesh(alwaysDisplayFacemesh ? false : hideFacemesh);

            if (!useBlendShapes) return;

            arBlendShapeController.FaceMesh = faceMesh;
            faceMesh.ARBlendShapeController = arBlendShapeController != null ? arBlendShapeController : null;

            arBackground = new ArBackground(cam);
            ShowWebCamBackground(displayWebCamBackground);
        }

        public void ShowWebCamBackground(bool flag)
        {
            arBackground.ShowBackground(flag);
        }

        /// <summary>
        /// Invoke by EventBus.onFaceTracking event and apply received parameter to facemesh and camera.
        /// </summary>
        /// <param name="position">Facemesh position on scene</param>
        /// <param name="rotation">Facemesh rotation on scene</param>
        /// <param name="scale">Facemesh scale on scene (multiplayed by constant SCALE_FACTOR)</param>
        /// <param name="cameraFOV">Camera field of view</param>
        /// <param name="facemeshData">Array of positions for each vertex</param>
        /// <param name="status">Current recognition status (-1 - face is not recognized)</param>
        protected void Tracking(Vector3 position, Quaternion rotation, Vector3 scale, float cameraFOV, Vector3[] facemeshData, int status)
        {
            if (faceMesh != null && faceMesh.gameObject.activeSelf)
            {
                faceMesh.RecalculateFaceMesh(facemeshData);
            }

            Vector3 positionConverted = new Vector3(movementConstraints.x ? scene.position.x : -position.x,
                                                    movementConstraints.y ? scene.position.y : position.y,
                                                    movementConstraints.z ? scene.position.z : position.z);

            Quaternion rotationConstr = new Quaternion(rotationConstraints.x ? scene.rotation.x : rotation.x,
                                                        rotationConstraints.y ? scene.rotation.y : rotation.y,
                                                        rotationConstraints.z ? scene.rotation.z : rotation.z,
                                                        rotation.w);

            Vector3 scaleConverted = scale * SCALE_FACTOR;
            if (status == 1)
            {
                if (!scene.gameObject.activeSelf)
                {
                    scene.gameObject.SetActive(true);
                }

                scene.position = positionConverted;
                scene.rotation = rotationConstr;
                scene.localScale = scaleConverted;
                cam.fieldOfView = fixedCameraFieldOfView ? cam.fieldOfView : cameraFOV;
            }
            else
            {
                if (scene.gameObject.activeSelf)
                {
                    scene.gameObject.SetActive(alwaysDisplayFacemesh);
                }
            }
        }

        [System.Serializable]
        public struct Constraints
        {
            public bool x;
            public bool y;
            public bool z;
        }
    }
}