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
    }

    public List<Creature> creatures = new List<Creature>();

    public Creature Find(string poster) => creatures.Find(c => c.poster == poster);

    /// <summary>How many of these the player has caught.</summary>
    public int Caught => creatures.Count(c => Account.Collection.Contains(c.poster));
}
