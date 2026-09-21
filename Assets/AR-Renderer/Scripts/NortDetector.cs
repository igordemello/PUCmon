using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public class NortDetector : MonoBehaviour
    {
        private float _angleToNorth = -1;

        [SerializeField] private float requiredAccuracy = 100;
        [SerializeField] private float lerpSpeed = 5;
        [SerializeField] private float stabilizationAngle = 20;

        public void NorthDirection(float angleToNorth, float accuracy)
        {
            if (accuracy > requiredAccuracy) return;

            if (_angleToNorth == -1)
            {
                _angleToNorth = angleToNorth;
                Vector3 rotation = new Vector3(0, _angleToNorth, 0);
                transform.rotation = Quaternion.Euler(rotation);
            }

            _angleToNorth = angleToNorth;
        }

        private void FixedUpdate()
        {
            if (_angleToNorth == -1) return;

            Vector3 rotation = new Vector3(0, _angleToNorth, 0);
            float angle = Quaternion.Angle(transform.rotation, Quaternion.Euler(rotation));
            float multiplayer = Mathf.Clamp01(angle / stabilizationAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotation), Time.fixedDeltaTime * lerpSpeed * multiplayer);
        }

        private void OnValidate()
        {
            const float MIN = 0.1f;
            if (stabilizationAngle <= MIN) stabilizationAngle = MIN;
        }
    }
}
