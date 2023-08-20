using UnityEngine;

[CreateAssetMenu(fileName = "New Kreeture", menuName = "Kreeture")]
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
    public GameObject modelPrefab; // Reference to the 3D model Prefab
    // Add more fields as needed for abilities, type advantages, etc.
}

public enum KreetureType
{
    None,
    Flame,
    Aqua,
    Earth,
    Wind,
    Terra,
    Umbral,
    Neutral,
    Physical,
    Toxic,
    Enigma,
    Verdant,
    Stone,
    Ethereal,
    IronClad,
    Arcane,
    Beast,
    Frost
}