using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Kreeture", menuName = "Kreeture/New Kreeture")]
public class Kreeture : ScriptableObject
{
    public string kreetureName;
    public int level = 1;
    public int baseHP = 45;
    public int currentHP = 45;
    public int attack = 50;
    public int defense = 40;
    public int agility = 60;
    public int elementalStrike = 65;
    public int elementalWard = 50;
    public KreetureType kreetureType;
    public KreetureType kreetureType2 = KreetureType.None;
    public List<KreetureType> Types = new List<KreetureType>();
    public GameObject modelPrefab; // Reference to the 3D model Prefab
    public List<Attack> knownAttacks = new List<Attack>();
    [SerializeField] private LearnableMovesDatabase learnableMovesDatabase;
    // Add more fields as needed for abilities, type advantages, etc.

    public void TakeDamage(int damageAmount, AttackType attackType, List<KreetureType> defenderTypes)
    {
        // Reduce the current HP by the adjusted damage
        currentHP -= Mathf.FloorToInt(damageAmount);
        currentHP = Mathf.Max(currentHP, 0);
    }
}



public enum KreetureType
{
    None,
    Flame,
    Aqua,
    Earth,
    Wind,
    Terra,
    Volt,
    Umbral,
    Neutral,
    Physical,
    Toxic,
    Enigma,
    Verdant,
    Stone,
    Ethereal,
    Ironclad,
    Arcane,
    Beast,
    Frost
}