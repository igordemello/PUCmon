#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Qualium_Systems.WebXRPlugin.Editor
{
    class BuildPrecrocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            //Application.runInBackground = true;
            if (Directory.Exists("Assets/WebGLTemplates/AR-Renderer/Targets")) {
                FileUtil.DeleteFileOrDirectory("Assets/WebGLTemplates/AR-Renderer/Targets");
            }

            if (Directory.Exists("Assets/WebGLTemplates/AR-Renderer/Targets.meta"))
            {
                FileUtil.DeleteFileOrDirectory("Assets/WebGLTemplates/AR-Renderer/Targets.meta");
            }

            if (Directory.Exists("Assets/Targets"))
            {
                FileUtil.CopyFileOrDirectory("Assets/Targets", "Assets/WebGLTemplates/AR-Renderer/Targets");
            }
        }
    }
}
#endif