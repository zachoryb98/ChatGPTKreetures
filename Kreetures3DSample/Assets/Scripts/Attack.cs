using System.Collections.Generic;
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
	Physical,
	Verdant,
	Stone,
	Ethereal,
	Ironclad,
	Arcane,
	Beast,
	Frost
}

#region TypeEffectiveness
public static class TypeEffectivenessCalculator
{
	public static float CalculateEffectiveness(AttackType attackType, List<KreetureType> defenderTypes)
	{
		float effectiveness = 1f;

		float typeMultiplier = GetEffectivenessMultiplier(attackType, defenderTypes);
		effectiveness *= typeMultiplier;


		return effectiveness;
	}

	private static float GetEffectivenessMultiplier(AttackType attackType, List<KreetureType> defenderTypes)
	{
		float multiplier1 = GetSingleTypeEffectivenessMultiplier(attackType, defenderTypes[0]);
		float multiplier2 = GetSingleTypeEffectivenessMultiplier(attackType, defenderTypes[1]);

		// Calculate the combined effectiveness multiplier
		return multiplier1 * multiplier2;
	}

	private static float GetSingleTypeEffectivenessMultiplier(AttackType attackType, KreetureType defenderType)
	{
		switch (attackType)
		{
			//Water replacement
			case AttackType.Aqua:
				switch (defenderType)
				{
					case KreetureType.Flame:
					case KreetureType.Earth:
					case KreetureType.Stone:
						return 2f; // Strong against Flame, Earth, Stone
					case KreetureType.Volt:
					case KreetureType.Terra:
						return 0.5f; // Weak against Volt, Terra
					default:
						return 1f; // Default effectiveness (neutral)
				}
			//Fire Replacement
			case AttackType.Flame:
				switch (defenderType)
				{
					case KreetureType.Verdant:
					case KreetureType.Ironclad:
					case KreetureType.Terra:
					case KreetureType.Frost:// Strong against Verdant, Ironclad, Terra, and Frost
						return 2f;
					case KreetureType.Earth:
					case KreetureType.Aqua:
					case KreetureType.Stone:
						return 0.5f; // Weak against Earth, aqua, stone
					default:
						return 1f;
				}
			//Flying Replacement
			case AttackType.Wind:
				switch (defenderType)
				{
					case KreetureType.Earth:
					case KreetureType.Verdant:
					case KreetureType.Physical:// Strong against Earht, Verdant, and Physical
						return 2f;
					case KreetureType.Volt:
					case KreetureType.Stone:
					case KreetureType.Frost:
						return 0.5f; // Weak against Earth, aqua, stone
					default:
						return 1f;
				}
			//Grass replacement
			case AttackType.Earth:
				switch (defenderType)
				{
					case KreetureType.Terra:
					case KreetureType.Stone:
					case KreetureType.Aqua:// Strong against Earht, Verdant, and Physical
						return 2f;
					case KreetureType.Wind:
					case KreetureType.Toxic:
					case KreetureType.Verdant:
					case KreetureType.Flame:
					case KreetureType.Frost:
						return 0.5f; // Weak against Earth, aqua, stone
					default:
						return 1f;
				}
			//Dark Type Replacement
			case AttackType.Umbral:
				switch (defenderType)
				{
					case KreetureType.Ethereal:
					case KreetureType.Enigma:// Strong against Ethereal, Enigma
						return 2f;
					case KreetureType.Verdant:
					case KreetureType.Arcane:
					case KreetureType.Physical:
						return 0.5f; // Weak against Earth, aqua, stone
					default:
						return 1f;
				}
			//Electric Type Replacement
			case AttackType.Volt:
				switch (defenderType)
				{
					case KreetureType.Wind:
					case KreetureType.Aqua:// Strong against Wind, Aqua
						return 2f;
					case KreetureType.Terra:
						return 0.5f; // Weak against Earth, aqua, stone
					default:
						return 1f;
				}
			//Ground Type Replacement
			case AttackType.Terra:
				switch (defenderType)
				{
					case KreetureType.Toxic:
					case KreetureType.Stone:
					case KreetureType.Ironclad:
					case KreetureType.Flame:
					case KreetureType.Volt:// Strong against Toxic, Stone, Ironclad, Flame, Volt
						return 2f;
					case KreetureType.Aqua:
					case KreetureType.Earth:
					case KreetureType.Frost:
						return 0.5f; // Weak against Earth, aqua, stone
					default:
						return 1f;
				}
			//Normal Type Replacement
			case AttackType.Neutral:
				switch (defenderType)
				{
					case KreetureType.Physical: //Physical is super affective
						return 2f;
					case KreetureType.Ethereal: //Ethereal type does nothing.
						return 0f;
					default:
						return 1f;
				}
			//Fighting Replacement
			case AttackType.Physical:
				switch (defenderType)
				{
					case KreetureType.Neutral:
					case KreetureType.Stone:
					case KreetureType.Ironclad:
					case KreetureType.Umbral:
						return 2f;
					case KreetureType.Wind:
					case KreetureType.Enigma:
					case KreetureType.Arcane:
						return 0.5f;
					default:
						return 1f;
				}
			//Poison Replacement
			case AttackType.Toxic:
				switch (defenderType)
				{
					case KreetureType.Earth:
					case KreetureType.Arcane:
						return 2f;
					case KreetureType.Terra:
					case KreetureType.Enigma:
						return .5f;
					default:
						return 1f;
				}
			//Psychic Type
			case AttackType.Enigma:
				switch (defenderType)
				{
					case KreetureType.Physical:
					case KreetureType.Toxic://String against physical and toxic
						return 2f;
					case KreetureType.Verdant:
					case KreetureType.Ethereal:
					case KreetureType.Umbral: //Weak against Verdant, Ethereal, and Umbral
						return 0.5f;
					default:
						return 1f;
				}
			//Bug Type replacement
			case AttackType.Verdant:
				switch (defenderType)
				{
					case KreetureType.Earth:
					case KreetureType.Enigma:
					case KreetureType.Umbral: //Strong against Earth, Enigma, and Umbral
						return 2f;
					case KreetureType.Wind:
					case KreetureType.Stone:
					case KreetureType.Flame:
					case KreetureType.Frost: //Weak against Wind, Stone, Flame, and frost
						return 0.5f;
					default:
						return 1f;
				}
			//Rock Type replacement
			case AttackType.Stone:
				switch (defenderType)
				{
					case KreetureType.Wind:
					case KreetureType.Verdant:
					case KreetureType.Flame: //Strong against wind verdant and flame
						return 2f;
					case KreetureType.Physical:
					case KreetureType.Earth:
					case KreetureType.Ironclad:
					case KreetureType.Aqua:
					case KreetureType.Terra: // Weak against physical, earth, ironclad, Aqua, Terra
						return 0.5f;
					default:
						return 1f;
				}
			//Ghost type replacemenet
			case AttackType.Ethereal:
				switch (defenderType)
				{
					case KreetureType.Ethereal:
					case KreetureType.Enigma: //strong against other Ethereal types and enigma
						return 2f;
					case KreetureType.Umbral:
					case KreetureType.Arcane: //Weak against umbral and arcane
						return 0.5f;
					default:
						return 1f;
				}
			//Steel type replacement
			case AttackType.Ironclad:
				switch (defenderType)
				{
					case KreetureType.Stone:
					case KreetureType.Arcane:
					case KreetureType.Beast://Super effective against stone, Arcane, and Beast types
						return 2f;
					case KreetureType.Physical:
					case KreetureType.Terra:
					case KreetureType.Flame: //Weak against Physical, Terra, and flame
						return 0.5f;
					default:
						return 1f;
				}
			//Fairy replacement
			case AttackType.Arcane:
				switch (defenderType)
				{
					case KreetureType.Physical:
					case KreetureType.Beast:
					case KreetureType.Umbral: //strong to Physical, beast, and umbral
						return 2f;
					case KreetureType.Toxic:
					case KreetureType.Ironclad: //weak to toxic, and ironclad
						return 0.5f;
					default:
						return 1f;
				}
			//Dragon replacement
			case AttackType.Beast:
				switch (defenderType)
				{
					case KreetureType.Beast:
					case KreetureType.Stone: //strong against beast and stone
						return 2f;
					case KreetureType.Arcane:
					case KreetureType.Frost: //weak against arcane and frost
						return 0.5f;
					default:
						return 1f;
				}
			//Ice replacement
			case AttackType.Frost:
				switch (defenderType)
				{
					case KreetureType.Wind:
					case KreetureType.Terra:
					case KreetureType.Earth:
					case KreetureType.Beast://Strong against wind terra eearth and beast
						return 2f;

					case KreetureType.Physical:
					case KreetureType.Stone:
					case KreetureType.Ironclad:
					case KreetureType.Aqua:
					case KreetureType.Flame://weak agaomst physical, stone, ironclad, aqua, and flame
						return 0.5f;
					default:
						return 1f;
				}
			default:
				return 1f; // Default effectiveness (neutral) for unknown types
		}
	}
	#endregion TypeEffectiveness
}