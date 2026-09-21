using System.Collections.Generic;
using Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.BlendShapes;
using Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.Demo
{
    public class DemoFaceTracking : MonoBehaviour
    {
        #region UI
        [SerializeField] private GameObject calibrateUI;
        [SerializeField] private Button calibrate;
        #endregion
        #region Model

        [SerializeField] private GameObject model;
        [SerializeField] private Transform headBone;
        [SerializeField] private Transform jawBone;
        [SerializeField] private Transform leftEye;
        [SerializeField] private Transform rightEye;
        #endregion
        #region Other
        [SerializeField] private ARBlendShapeController arBlendShapeController;
        [SerializeField] private FaceTrackController faceTrackController;
        [SerializeField] private List<ARBlendShapeDictionaryInspector> blendShapesMap;
        private  Camera camera;
       
        private const float JawCloseX = 7.618162f; // JawBone X position with closed jaw.
        private const float JawOpenX = 14.618162f; // JawBone X position with an open jaw.
       
        #endregion
       
        private void Start()
        {
            arBlendShapeController.Init(blendShapesMap,model,headBone,jawBone);
            arBlendShapeController.JawSetup(JawCloseX,JawOpenX);
            calibrate.onClick.AddListener(Calibrate);
            camera = Camera.main;
            arBlendShapeController.StartTracking();
            faceTrackController.ShowWebCamBackground(true);
        }

      
        private void Update()
        {
        LookAtCamera();
        }

        private void Calibrate()
        {
            model.SetActive(true);
            arBlendShapeController.Calibrate();
            calibrateUI.SetActive(false);
            faceTrackController.ShowWebCamBackground(false);
        }
        private void LookAtCamera()
        {
            leftEye.LookAt(camera.transform);
            rightEye.LookAt(camera.transform);
        }
    }
}
