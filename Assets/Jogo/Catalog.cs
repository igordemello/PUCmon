using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>The PUCmons there are to catch: one per poster, with the name and picture the screens show.</summary>
[CreateAssetMenu(menuName = "PUCmon/Catálogo")]
public class Catalog : ScriptableObject
{
    [Serializable]
    public class Creature
    {
        [Tooltip("Name of the poster image that brings it (Assets/ImagesToTracker), e.g. \"1\". Also its id in Supabase.")]
        public string poster;
        public string displayName;
        [Tooltip("2D picture for the collection and the capture screen.")]
        public Sprite portrait;
        [HideInInspector, Tooltip("Model and pose the picture was made from (PortraitMaker redoes it when they change).")]
        public string portraitOf;

        [Header("Minigame")]
        [Tooltip("Chance that a hit catches it, before the ring bonus (see CaptureMinigame).")]
        [Range(0.05f, 1f)] public float catchRate = 0.5f;
        [Tooltip("How fast it sways around in its hole (1 = normal).")]
        [Range(0.3f, 3f)] public float agility = 1f;
        [Tooltip("How often it hides back in the hole (1 = normal, 0 = never).")]
        [Range(0f, 3f)] public float shyness = 1f;
    }

    public List<Creature> creatures = new List<Creature>();

    public Creature Find(string poster) => creatures.Find(c => c.poster == poster);

    /// <summary>How many of these the player has caught.</summary>
    public int Caught => creatures.Count(c => Account.Collection.Contains(c.poster));
}
