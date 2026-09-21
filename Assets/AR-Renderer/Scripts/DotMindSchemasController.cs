using System.IO;
using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public static class DotMindSchemasController {
        public static void CreateSchema(int sceneIndex, Target[] targets) {
            string[] strings = new string[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                TargetData td = new TargetData(sceneIndex, i, targets[i].TargetPreview.Image.name);
                strings[i] = JsonUtility.ToJson(td);
            }

            WriteToFile(strings, sceneIndex.ToString());

        }

        private static void WriteToFile(string[] strings, string fileName)
        {
            string path = GetFilePath(fileName);
            FileStream fileStream = new FileStream(path, FileMode.Create);

            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.Write("{\n");
                for (int i = 0; i < strings.Length; i++)
                {

                    writer.Write("\"" + i + "\" :" + strings[i]);
                    if (i != strings.Length - 1)
                    {
                        writer.Write(",\n");
                    }
                }
                writer.Write("\n}");
            }
        }

        private static string GetFilePath(string fileName)
        {
            string filePath = Application.dataPath + "/Compiler/Schemas";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            return filePath  + "/" + fileName + ".json";
        }
    }

    public struct TargetData
    {
        public int sceneIndex;
        public int objectIndex;
        public string targetImgName;

        public TargetData(int sceneIndex, int objectIndex, string targetImgName)
        {
            this.sceneIndex = sceneIndex;
            this.objectIndex = objectIndex;
            this.targetImgName = targetImgName;
        }
    }
}