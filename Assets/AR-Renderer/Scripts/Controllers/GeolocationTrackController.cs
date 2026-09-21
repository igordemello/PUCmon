using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.Controllers
{
    public class GeolocationTrackController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LocationsManager lm;
        [SerializeField] private NortDetector nd;

        private float longitude = 0;
        private float latitude = 0;
        private float altitude = 0;

        [DllImport("__Internal")]
        protected static extern int LoadScene(string message, string index, string targetImg);

        private void Start()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;

            //Send message to frontend to start frontend`s recognition scene.
            LoadScene("geolocation", sceneIndex.ToString(), null);
        }

        private void OnEnable()
        {
            Receiver.Activate();
            EventBus.onGeolocationTracking += Tracking;
        }

        private void OnDisable()
        {
            EventBus.onGeolocationTracking -= Tracking;

            //Send message to frontend to stop frontend`s recognition scene.
            LoadScene("stop", null, null);
        }

        /// <summary>
        /// Invoke by EventBus.onMarkerTracking event and apply received parameter to camera.
        /// </summary>
        /// <param name="position">Camera position on scene</param>
        /// <param name="rotation">Camera rotation on scene</param>
        /// <param name="cameraFOV">Camera field of view on scene</param>
        /// <param name="geoPos">Struct with gps accuracy, altitude, heading, latitude, longitude, speed
        /// (For more information check: https://developer.mozilla.org/en-US/docs/Web/API/GeolocationCoordinates)
        /// </param>
        void Tracking(Vector3 position, Quaternion rotation, float cameraFOV, GeolocationCoordinates geoPos)
        {
            longitude = geoPos.longitude;
            latitude = geoPos.latitude;
            altitude = geoPos.altitude;

            if (longitude != 0 || latitude != 0 || altitude != 0)
            {
                EventBus.onGeolocationUpdated?.Invoke(GetCurrentPosition());
            }

            Vector3 rotationConverted = new Quaternion(rotation.x, -rotation.z, rotation.y, rotation.w).eulerAngles;
            nd.NorthDirection(geoPos.heading, geoPos.accuracy);
            lm.SetPositions(geoPos.longitude, geoPos.latitude, geoPos.altitude, geoPos.accuracy);
            rotationConverted.y = 0;
            cameraTransform.localRotation = Quaternion.Euler(rotationConverted);
            cam.fieldOfView = cameraFOV;
        }

        public Location[] GetLocations() {
            return lm.locations;
        }

        public Vector3 GetCurrentPosition()
        {
            return new Vector3(longitude, altitude, latitude);
        }
    }
}