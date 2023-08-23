using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Kreeture/Attack")]
public class Attack : ScriptableObject
{
    public new string name;
    public int power;
    public AttackCategory attackCategory;
    public AttackType attackType; // Renamed enum
}

public enum AttackCategory
{
    Physical,
    Special
}

public enum AttackType // Renamed enum
{
    Aqua,
    Flame,
    Wind,
    Earth,
    Umbral,
    Volt,
    Terra,
    Neutral,
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
