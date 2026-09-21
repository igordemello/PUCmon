using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public static class DistanceCalculator
    {

        public static int GetDistance(Vector2 coords1, Vector2 coords2)
        {
            float R = 6371000f;

            float lat1 = coords1.x;
            float lon1 = coords1.y;
            float lat2 = coords2.x;
            float lon2 = coords2.y;

            float dLat = toRad(lat2 - lat1);
            float dLon = toRad(lon2 - lon1);
            float lat1r = toRad(lat1);
            float lat2r = toRad(lat2);

            float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2) * Mathf.Cos(lat1r) * Mathf.Cos(lat2r);
            float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
            int d = Mathf.RoundToInt(R * c);
            return d;
        }

        private static float toRad(float value)
        {
            return value * Mathf.PI / 180f;
        }
    }
}
