using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Creatures", menuName = "Scriptable Objects/Creatures")]
public class CreaturesObject : ScriptableObject {
    public List<Creature> Creatures = new List<Creature>();
}
