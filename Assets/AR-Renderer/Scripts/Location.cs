using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{

    public class Location : MonoBehaviour
    {
        public string locationName;
        public float latitude;
        public float longitude;
        public float altitude;
        public bool useAltitude;


        [HideInInspector] public float _longitudeDiff = 0;
        [HideInInspector] public float _latitudeDiff = 0;
        [HideInInspector] public float _altitudeDiff = 0;

        public void SetNewDiffs(float longitudeDiff, float latitudeDiff, float altitudeDiff)
        {
            if (_longitudeDiff == 0 && _latitudeDiff == 0 && _altitudeDiff == 0)
            {
                transform.position = new Vector3(longitudeDiff, altitudeDiff, latitudeDiff);
            }

            _longitudeDiff = longitudeDiff;
            _latitudeDiff = latitudeDiff;

            if (!useAltitude) return;
            _altitudeDiff = altitudeDiff;
        }

        public void LerpPosition(float lerpSpeed)
        {
            if (_longitudeDiff == 0 && _latitudeDiff == 0 && _altitudeDiff == 0) return;

            transform.position = Vector3.Lerp(transform.position, new Vector3(_longitudeDiff, _altitudeDiff, _latitudeDiff), Time.fixedDeltaTime * lerpSpeed);
        }

        public void LerpScale(float distanceCap)
        {
            if (distanceCap == -1) return;

            float scaleMultiplayer = Mathf.Clamp01(1 - VectorToCamera().magnitude / distanceCap + 0.1f);
            transform.localScale = Vector3.one * scaleMultiplayer;
        }

        public Vector3 VectorToCamera()
        {
            return new Vector3(_longitudeDiff, _altitudeDiff, _latitudeDiff);
        }
    }
}