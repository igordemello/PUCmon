using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

namespace Qualium_Systems.WebXRPlugin.Examples.aphrodite
{
    public class CompassTest : MonoBehaviour
    {
        public TMP_Text output;
        public TMP_Text northAngle;

        private void Start()
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermission(Permission.FineLocation, callbacks);
        }

        internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            Input.location.Start();
            Input.compass.enabled = true;
            StartCoroutine(InfiniteLoop());
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
        }

        IEnumerator LocationService()
        {
            // Check if the user has location service enabled.
            if (!Input.location.isEnabledByUser)
            {
                output.text = "Location service disabled";
                yield break;
            }

            // Starts the location service.
            Input.location.Start();
            Input.compass.enabled = true;

            // Waits until the location service initializes
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // If the service didn't initialize in 20 seconds this cancels location service use.
            if (maxWait < 1)
            {
                output.text = "Timed out";
                yield break;
            }

            // If the connection failed this cancels location service use.
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                output.text = "Unable to determine device location";
                yield break;
            }
            else
            {
                // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
                output.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude;
                northAngle.text = "Norh Angle: " + Input.compass.trueHeading;
            }
        }

        private IEnumerator InfiniteLoop()
        {
            WaitForSeconds waitTime = new WaitForSeconds(1);
            while (true)
            {
                output.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude;
                northAngle.text = "Norh Angle: " + Input.compass.trueHeading;
                yield return waitTime;
            }
        }
    }
}
