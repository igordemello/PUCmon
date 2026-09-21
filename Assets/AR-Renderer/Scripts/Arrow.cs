using TMPro;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts {
    public class Arrow : MonoBehaviour
    {
        public Location location;
        public TextMeshPro text;
        Vector2 position;

        void Update()
        {
            transform.position = location.VectorToCamera().normalized * 2;
            int distance = DistanceCalculator.GetDistance(new Vector2(location.latitude, location.longitude), position);
            text.text = location.locationName + "\n" + distance.ToString() + " m";
            transform.LookAt(location.transform.position);
        }

        public void SetPositions(Vector2 position)
        {
            this.position = position;
        }
    }
}
