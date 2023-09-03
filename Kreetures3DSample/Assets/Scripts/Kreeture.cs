using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Kreeture", menuName = "Kreeture/New Kreeture")]
public class Kreeture : ScriptableObject
{
    public string kreetureName;
    public int currentXP = 0;
    public int currentLevel = 1;
    public int xpRequiredForNextLevel = 100; // Adjust this value as needed
    public bool leveledUp;
    public int xpTransfer;
    public int baseHP = 45;
    public int currentHP = 45;
    public int attack = 50;
    public int defense = 40;
    public int agility = 60;
    public int elementalStrike = 65;
    public int elementalWard = 50;
    public int focus = 50;
    public int mobility = 40;    
    public KreetureType kreetureType;
    public KreetureType kreetureType2 = KreetureType.None;
    public List<KreetureType> Types = new List<KreetureType>();
    public GameObject modelPrefab; // Reference to the 3D model Prefab
    public List<Attack> knownAttacks = new List<Attack>();
    [SerializeField] private LearnableMovesDatabase learnableMovesDatabase;
    public int minCoreAttribute = 1; // Minimum possible IV adjustment
    public int maxCoreAttribute = 10;  // Maximum possible IV adjustment
    private int minValue = 1;
    // Add more fields as needed for abilities, type advantages, etc.

    public void TakeDamage(int damageAmount, AttackType attackType, List<KreetureType> defenderTypes)
    {
        // Reduce the current HP by the adjusted damage
        currentHP -= Mathf.FloorToInt(damageAmount);
        currentHP = Mathf.Max(currentHP, 0);
    }

    public void AdjustStatsBasedOnLevel(int _level)
    {
        this.currentLevel = _level;
        float levelMultiplier = (float)currentLevel / 100.0f;
        int hpCoreAttribute = Random.Range(minCoreAttribute, maxCoreAttribute + 11); // Adjust the range
        int attackCoreAttribute = Random.Range(minCoreAttribute, maxCoreAttribute + 6);
        int defenseCoreAttribute = Random.Range(minCoreAttribute, maxCoreAttribute + 6);
        int agilityCoreAttribute = Random.Range(minCoreAttribute, maxCoreAttribute + 6);
        int elementalStrikeCoreAttribute = Random.Range(minCoreAttribute, maxCoreAttribute + 6);
        int elementalWardCoreAttribute = Random.Range(minCoreAttribute, maxCoreAttribute + 6);
        int focusCoreAttribute = Random.Range(minCoreAttribute, maxCoreAttribute + 6);
        int mobilityCoreAttribute = Random.Range(minCoreAttribute, maxCoreAttribute + 6);

        //ADD LAYERS OF CODE
        //SAY 3 PERFECT CORE ATTRIBUTES ARE SCORED MAYBE REROLL THE ONES THAT ARE NOT THE BEST SCORE FOR THE LEVEL

        // Apply adjustments based on level and IVs
        baseHP = Mathf.Max(Mathf.RoundToInt(baseHP * levelMultiplier) + hpCoreAttribute, minValue);
        attack = Mathf.Max(Mathf.RoundToInt(attack * levelMultiplier) + attackCoreAttribute, minValue);
        defense = Mathf.Max(Mathf.RoundToInt(defense * levelMultiplier) + defenseCoreAttribute, minValue);
        agility = Mathf.Max(Mathf.RoundToInt(agility * levelMultiplier) + agilityCoreAttribute, minValue);
        elementalStrike = Mathf.Max(Mathf.RoundToInt(elementalStrike * levelMultiplier) + elementalStrikeCoreAttribute, minValue);
        elementalWard = Mathf.Max(Mathf.RoundToInt(elementalWard * levelMultiplier) + elementalWardCoreAttribute, minValue);
        focus = Mathf.Max(Mathf.RoundToInt(focus * levelMultiplier) + focusCoreAttribute, minValue);
        mobility = Mathf.Max(Mathf.RoundToInt(mobility * levelMultiplier) + mobilityCoreAttribute, minValue);
    }

    public void GainXP(int xpAmount)
    {
        currentXP += xpAmount;

		while (currentXP >= xpRequiredForNextLevel)
		{
			currentLevel++;
			currentXP -= xpRequiredForNextLevel;
			xpRequiredForNextLevel = CalculateRequiredXPForNextLevel(currentLevel);
            leveledUp = true;
            // Implement attribute adjustments, evolution, or other logic when leveling up

            //YOU NEED TO SET THE XP SO WE CAN CHECK IF THERE WAS XP LEFT ON THE LEVEL UP
            xpTransfer = currentXP;

			// You can also send a message to the UI to display level up information            
		}        
    }
    private int CalculateRequiredXPForNextLevel(int level)
    {
        // Example formula: xpRequired = level * level * 100
        return level * level * 100; // Adjust the formula as needed
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