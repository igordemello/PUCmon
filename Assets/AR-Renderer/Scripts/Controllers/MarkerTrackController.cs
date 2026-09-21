using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.Controllers
{
    public class MarkerTrackController : MonoBehaviour
    {
        public Transform hitPoint;

        const float POSITION_MULTUPLAYER = 100;
        [SerializeField] private Camera cam;
        [SerializeField] private Transform cameraTransform;

        [DllImport("__Internal")]
        protected static extern int LoadScene(string message, string index, string targetImg);

        private void Start()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;

            //Send message to frontend to start frontend`s recognition scene.
            LoadScene("marker", sceneIndex.ToString(), null);
        }

        private void OnEnable()
        {
            Receiver.Activate();
            EventBus.onMarkerTracking += Tracking;
        }

        private void OnDisable()
        {
            EventBus.onMarkerTracking -= Tracking;

            //Send message to frontend to stop frontend`s recognition scene.
            LoadScene("stop", null, null);
        }

        /// <summary>
        /// Invoke by EventBus.onMarkerTracking event and apply received parameter to camera.
        /// </summary>
        /// <param name="position">Camera position on scene</param>
        /// <param name="rotation">Camera rotation on scene</param>
        /// <param name="cameraFOV">Camera field of view on scene</param>
        void Tracking(Vector3 position, Quaternion rotation, float distanceToHitPoint, float cameraFOV)
        {
            Vector3 rotationConverted = new Quaternion(rotation.x, -rotation.z, rotation.y, rotation.w).eulerAngles;
            cameraTransform.localRotation = Quaternion.Euler(rotationConverted);
            cameraTransform.position = new Vector3(position.x, position.z, -position.y) * POSITION_MULTUPLAYER;

            cam.fieldOfView = cameraFOV;

            hitPoint.localPosition = new Vector3 (0,0, distanceToHitPoint)  * POSITION_MULTUPLAYER;
            EventBus.onHitPointUpdated?.Invoke(hitPoint.position);
        }
    }
}