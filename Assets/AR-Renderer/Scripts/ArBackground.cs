using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
    public class ArBackground
    {
        private Camera camera;
        private CameraClearFlags defaultClearFlags;
        private Color defaultBackgroundColor;
        public ArBackground(Camera camera)
        {
            this.camera = camera;
            SaveCamSettings();
        }

        private void SaveCamSettings()
        {
            defaultClearFlags = camera.clearFlags;
            defaultBackgroundColor =  camera.backgroundColor;
            defaultBackgroundColor.a = 1;
        }

        public void ShowBackground(bool flag)
        {
            if (flag)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                return;
            }

            camera.clearFlags = defaultClearFlags;
            camera.backgroundColor = defaultBackgroundColor;
        }
    }
}