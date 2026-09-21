using UnityEngine;
using UnityEngine.SceneManagement;

namespace Qualium_Systems.WebXRPlugin.Examples.Scripts
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadScene(int index) { 
            SceneManager.LoadScene(index);
        }
    }
}
