using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A collection card: the PUCmon's picture and name, or a black silhouette and "???" while not caught; a "!" when it
/// is hungry or sad. Tapping a caught one opens its pen.
/// </summary>
public class CreatureCard : MonoBehaviour
{
    public Image portrait;
    public TMP_Text label;
    public GameObject alert;

    public void Show(Catalog.Creature creature, bool caught, bool needsCare)
    {
        portrait.sprite = creature.portrait;
        portrait.color = caught ? Color.white : new Color32(0x10, 0x1F, 0x3F, 0xFF);  // navy silhouette
        label.text = caught ? creature.displayName : "???";
        alert.SetActive(needsCare);
    }
}
