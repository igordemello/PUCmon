using System;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public static class EventBus
    {
        /// <summary>
        /// Vector3 - position,
        /// Quaternion - rotation,
        /// Vector3 - scale,
        /// float - camera field of view,
        /// string - face mesh vertices array as string
        /// </summary>
        public static Action<
             Vector3,
             Quaternion,
             Vector3,
             float,
             Vector3[],
             int> onFaceTracking;                           //Calls when the face is tracking

        /// <summary>
        /// Vector3 - position,
        /// Quaternion - rotation,
        /// Vector3 - scale,
        /// float - camera field of view,
        /// int - target index
        /// </summary>
        public static Action<
            Vector3,
            Quaternion,
            Vector3,
            float,
            int> onTargetTracking;                          //Calls when the target is tracking

        /// <summary>
        /// Vector3 - camera position,
        /// Quaternion - camera rotation,
        /// float - distance to hitpoint,
        /// float - camera field of view
        /// </summary>
        public static Action<
            Vector3,
            Quaternion,
            float,
            float> onMarkerTracking;                        //Calls when the marker is tracking

        /// <summary>
        /// Vector3 - camera position,
        /// Quaternion - camera rotation,
        /// float - camera field of view
        /// </summary>
        public static Action<
            Vector3,
            Quaternion,
            float,
            GeolocationCoordinates> onGeolocationTracking;  //Calls when the geolocation is tracking

        public static Action onScanningFinished;            //Calls when surface scanning is finished

        /// <summary>
        /// int - scene index
        /// </summary>
        public static Action<int> onSceneLoaded;            //Calls when frontend scene is loaded

        /// <summary>
        /// Vector3 - user location (longitude, altitude, latitude)
        /// </summary>
        public static Action<Vector3> onGeolocationUpdated; //Calls when user`s geolocation updated

        /// <summary>
        /// Vector3 - hit point position;
        /// </summary>
        public static Action<Vector3> onHitPointUpdated;
    }
}