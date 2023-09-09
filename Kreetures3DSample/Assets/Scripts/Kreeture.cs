using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Kreeture", menuName = "Kreeture/New Kreeture")]
public class Kreeture : ScriptableObject
{
	[Header("Kreeture Stats")]
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

	private int healthIncrease;
	private int attackIncrease;
	private int defenseIncrease;
	private int agilityIncrease;
	private int elementalStrikeIncrease;
	private int elementalWardIncrease;

	[Header("Kreeture Growth Rate")]
	public GrowthRate healthGrowthRate;
	public GrowthRate attackGrowthRate;
	public GrowthRate defenseGrowthRate;
	public GrowthRate agilityGrowthRate;
	public GrowthRate elementalStrikeGrowthRate;
	public GrowthRate elementalWardGrowthRate;

	[Header("Attack Info")]
	public List<Attack> knownAttacks = new List<Attack>();
	[SerializeField] private LearnableMovesDatabase learnableMovesDatabase;

	[Header("Core Attributes")]
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
			//Level up
			currentLevel++;
			currentXP -= xpRequiredForNextLevel;
			xpRequiredForNextLevel = CalculateRequiredXPForNextLevel(currentLevel);
			leveledUp = true;
			// Implement attribute adjustments, evolution, or other logic when leveling up						

			healthIncrease = CalculateStatIncrease(healthGrowthRate);
			attackIncrease = CalculateStatIncrease(attackGrowthRate);
			defenseIncrease = CalculateStatIncrease(defenseGrowthRate);
			agilityIncrease = CalculateStatIncrease(agilityGrowthRate);
			elementalStrikeIncrease = CalculateStatIncrease(elementalStrikeGrowthRate);
			elementalWardIncrease = CalculateStatIncrease(elementalWardGrowthRate);


			// You can also send a message to the UI to display level up information            
		}
	}

	public List<int> GetCurrentStats()
	{
		List<int> currentStats = new List<int>();

		currentStats.Add(baseHP);
		currentStats.Add(attack);
		currentStats.Add(defense);
		currentStats.Add(agility);
		currentStats.Add(elementalStrike);
		currentStats.Add(elementalWard);

		return currentStats;
	}

	public List<int> GetNewStats()
	{
		List<int> newStats = new List<int>();

		newStats.Add(healthIncrease);
		newStats.Add(attackIncrease);
		newStats.Add(defenseIncrease);
		newStats.Add(agilityIncrease);
		newStats.Add(elementalStrikeIncrease);
		newStats.Add(elementalWardIncrease);

		return newStats;
	}

	public void UpdateStats()
	{
		baseHP = healthIncrease;
		attack = attackIncrease;
		defense = defenseIncrease;
		agility = agilityIncrease;
		elementalStrike = elementalStrikeIncrease;
		elementalWard = elementalWardIncrease;
	}

	private int CalculateRequiredXPForNextLevel(int level)
	{
		return level * 75; // Adjust this formula for faster leveling
	}

	private int CalculateStatIncrease(GrowthRate growthRate)
	{
		// Define growth rate multipliers (you can adjust these as needed)
		float slowestRateMultiplier = 0.75f;
		float slowerRateMultiplier = 1.0f;
		float slowRateMultiplier = 1.25f;
		float normalRateMultiplier = 1.5f;
		float fastRateMultiplier = 1.75f;
		float fasterRateMultiplier = 2f;
		float fastestRateMultiplier = 2.25f;

		// Calculate the stat increase based on the growth rate
		switch (growthRate)
		{
			case GrowthRate.Slowest:
				return Mathf.FloorToInt(slowestRateMultiplier * currentLevel);
			case GrowthRate.Slower:
				return Mathf.FloorToInt(slowerRateMultiplier * currentLevel);
			case GrowthRate.Slow:
				return Mathf.FloorToInt(slowRateMultiplier * currentLevel);
			case GrowthRate.Normal:
				return Mathf.FloorToInt(normalRateMultiplier * currentLevel);
			case GrowthRate.Fast:
				return Mathf.FloorToInt(fastRateMultiplier * currentLevel);
			case GrowthRate.Faster:
				return Mathf.FloorToInt(fasterRateMultiplier * currentLevel);
			case GrowthRate.Fastest:
				return Mathf.FloorToInt(fastestRateMultiplier * currentLevel);
			default:
				return 0; // Handle any unknown growth rates here
		}
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

public enum GrowthRate
{
	Slowest,
	Slower,
	Slow,
	Normal,
	Fast,
	Faster,
	Fastest
}
