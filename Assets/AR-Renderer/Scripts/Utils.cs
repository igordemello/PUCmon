using System.Globalization;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public static class Utils
    {
        //Miscellaneous
        //Convert string to vector 3 array (verticies positions)
        public static Vector3[] FaceVerticies(string str)
        {
            float[] faceMeshData = StringToFloatArray(str);
            Vector3[] faceVerticies = new Vector3[faceMeshData.Length / 3];
            for (int i = 0; i < faceMeshData.Length / 3; i++)
            {
                faceVerticies[i] = new Vector3(faceMeshData[i * 3], faceMeshData[i * 3 + 1], faceMeshData[i * 3 + 2]);
            }
            return faceVerticies;
        }


        private static float[] StringToFloatArray(string str)
        {
            string[] strArr = str.Split('\u002C');              //Split array by comma (,)
            float[] floats = new float[strArr.Length];

            for (int i = 0; i < strArr.Length; i++)
            {
                floats[i] = float.Parse(strArr[i], CultureInfo.InvariantCulture);
            }

            return floats;
        }
    }
}
