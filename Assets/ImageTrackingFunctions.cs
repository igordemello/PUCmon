using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageTrackingFunctions : MonoBehaviour
{
    public int number;
    public TextMesh txt;
    public GameObject arTargetPreview;
    public GameObject arObject;

    public void TargetSeen()
    {
        arTargetPreview.SetActive(true);
        arObject.SetActive(true);
        number = 67;
        txt.text = "TESTE " + number;
    }

    public void TargetNOTSeen()
    {
        arTargetPreview.SetActive(false);
        arObject.SetActive(false);
    }
}
