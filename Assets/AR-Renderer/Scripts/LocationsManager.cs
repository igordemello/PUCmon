using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public class LocationsManager : MonoBehaviour
    {

        public Location[] locations;
        public Arrow[] arrows;

        [Tooltip("Distance multiplayer. Default value: 111139")]
        [SerializeField] private float distanceMultiplayer = 111139;
        [Tooltip("Default value: 10")]
        [SerializeField] private float lerpSpeed = 10f;
        [Tooltip("Representing the required accuracy for locations update, with a 95% confidence level, of the latitude and longitude properties expressed in meters. Default value: 50")]
        [SerializeField] private float requiredAccuracy = 50f;
        [Tooltip("Representing the render distance of location's model. Default value: -1 (infinite)")]
        [SerializeField] private float distanceCap = -1;

        public void SetPositions(float longitude, float latitude, float altitude, float accuracy)
        {
            if (accuracy > requiredAccuracy) return;

            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i].SetPositions(new Vector2(latitude, longitude));
            }

            for (int i = 0; i < locations.Length; i++)
            {
                float longitudeDiff = (locations[i].longitude - longitude) * distanceMultiplayer;
                float latitudeDiff = (locations[i].latitude - latitude) * distanceMultiplayer;
                float altitudeDiff = locations[i].altitude - altitude;

                locations[i].SetNewDiffs(longitudeDiff, latitudeDiff, altitudeDiff);
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < locations.Length; i++)
            {
                locations[i].LerpPosition(lerpSpeed);
                locations[i].LerpScale(distanceCap);
            }
        }
    }
}
