using System;
using System.Collections.Generic;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts.BlendShapes
{
    public class ARBlendShapeDictionary
    {
       
         private Dictionary<ARBlendShapesType, string> faceMap;
        
        public Dictionary<ARBlendShapesType, string> FaceMap => faceMap;

        public void ConvertDictionary(List<ARBlendShapeDictionaryInspector> inspectorDictionary)
        {
            faceMap = new Dictionary<ARBlendShapesType, string>();
            foreach (var blendShapeDictionary in inspectorDictionary)
            {
                faceMap.Add(blendShapeDictionary.Type,blendShapeDictionary.Name);
            }
        }
    }
    [Serializable]
    public  class ARBlendShapeDictionaryInspector
    {
        public ARBlendShapesType Type;
        public string Name;
    }
}
