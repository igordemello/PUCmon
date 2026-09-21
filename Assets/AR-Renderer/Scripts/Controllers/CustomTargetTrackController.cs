using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.Controllers
{
    public class CustomTargetTrackController : TargetTrackController
    {
        [DllImport("__Internal")]
        private static extern int CaptureTarget_cmd(string sceneIndex, string controller);

        protected override void OnValidate() { }

        protected override void Start()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;

            //Send message to frontend to start frontend`s recognition scene.
            LoadScene("surface", sceneIndex.ToString(), "1");
            HideAllTargets();
        }

        //This function must be called manually to start scanning the surface
        public void CaptureTarget()
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            CaptureTarget_cmd(sceneIndex.ToString(), gameObject.name);
        }
    }
}
