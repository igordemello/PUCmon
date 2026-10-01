using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>A collection card: the PUCmon's picture and name, or a black silhouette and "???" while not caught.</summary>
public class CreatureCard : MonoBehaviour
{
    public Image portrait;
    public TMP_Text label;

    public void Show(Catalog.Creature creature, bool caught)
    {
        portrait.sprite = creature.portrait;
        portrait.color = caught ? Color.white : new Color(0.07f, 0.04f, 0.15f, 1f);
        label.text = caught ? creature.displayName : "???";
    }
}
