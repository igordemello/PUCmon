using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

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
        [Tooltip("What of PUC-Rio it stands for: a course (\"Psicologia\") or anything else of the university (\"Bandejão\").")]
        [FormerlySerializedAs("course")] public string represents;
        [Tooltip("Who it is, its story: the text of its page in the collection.")]
        [TextArea(3, 8)] public string description;
        [Tooltip("2D picture for the collection and the capture screen.")]
        public Sprite portrait;
        [HideInInspector, Tooltip("Model and pose the picture was made from (PortraitMaker redoes it when they change).")]
        public string portraitOf;

        [Header("Repertório (amizade)")]
        [Tooltip("Its 4 actions, learned at friendship 1, 2, 3 and 4 (at 0 it knows none yet).")]
        public Skill[] actions = new Skill[4];
        [Tooltip("Passive ability, unlocked at friendship 5.")]
        public Skill passive;

        [Header("Primeira visita ao recinto")]
        [Tooltip("Conversation the first time the player opens its pen in the laboratory.")]
        public Dialogue.Line[] labIntro;

        [Header("Minigame")]
        [Tooltip("Share of its consciousness a rock takes, before the ring bonus (see CaptureMinigame): 0.5 = two hits.")]
        [FormerlySerializedAs("catchRate")] [Range(0.05f, 1f)] public float knock = 0.5f;
        [Tooltip("How fast it sways around in its hole (1 = normal).")]
        [Range(0.3f, 3f)] public float agility = 1f;
        [Tooltip("How often it hides back in the hole (1 = normal, 0 = never).")]
        [Range(0f, 3f)] public float shyness = 1f;
    }

    /// <summary>An action or the passive ability of a PUCmon, as its page in the collection shows it.</summary>
    [Serializable]
    public class Skill
    {
        public string name;
        [TextArea(2, 4)] public string description;
    }

    public List<Creature> creatures = new List<Creature>();

    public Creature Find(string poster) => creatures.Find(c => c.poster == poster);

    /// <summary>How many of these the player has caught.</summary>
    public int Caught => creatures.Count(c => Account.Collection.Contains(c.poster));
}
