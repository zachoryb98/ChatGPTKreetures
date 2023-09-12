using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Kreeture
{

	[SerializeField] KreetureBase _base;
	[SerializeField] int level;

	public KreetureBase Base
	{
		get
		{
			return _base;
		}
	}
	public int Level
	{
		get
		{
			return level;
		}
	}

	public int HP { get; set; }
	public List<Attack> Attacks { get; set; }
	public Dictionary<Stat, int> Stats { get; private set; }
	public Dictionary<Stat, int> StatBoosts { get; private set; }

	public void Init()
	{
		// Generate Moves
		Attacks = new List<Attack>();
		foreach (var attack in Base.LearnableAttacks)
		{
			if (attack.Level <= Level)
				Attacks.Add(new Attack(attack.Base));

			if (Attacks.Count >= 4)
				break;
		}

		CalculateStats();
		HP = MaxHp;

		StatBoosts = new Dictionary<Stat, int>()
		{
			{Stat.Attack, 0},
			{Stat.Defense, 0},
			{Stat.ElementalStrike, 0},
			{Stat.ElementalWard, 0},
			{Stat.Speed, 0},
		};
	}

	void CalculateStats()
	{
		Stats = new Dictionary<Stat, int>();
		Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
		Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
		Stats.Add(Stat.ElementalStrike, Mathf.FloorToInt((Base.ElmtStrike * Level) / 100f) + 5);
		Stats.Add(Stat.ElementalWard, Mathf.FloorToInt((Base.ElmtWard * Level) / 100f) + 5);
		Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

		MaxHp = Mathf.FloorToInt((Base.Speed * Level) / 100f) + 10;
	}

	int GetStat(Stat stat)
	{
		int statVal = Stats[stat];

		// Apply stat boost
		int boost = StatBoosts[stat];
		var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

		if (boost >= 0)
			statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
		else
			statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

		return statVal;
	}

	public void ApplyBoosts(List<StatBoost> statBoosts)
	{
		foreach (var statBoost in statBoosts)
		{
			var stat = statBoost.stat;
			var boost = statBoost.boost;

			StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

			Debug.Log($"{stat} has been bossted to {StatBoosts[stat]}");
		}
	}

	public int Attack
	{
		get { return GetStat(Stat.Attack); }
	}

	public int Defense
	{
		get { return GetStat(Stat.Defense); }
	}

	public int ElementalStrike
	{
		get { return Mathf.FloorToInt((Base.ElmtStrike * Level) / 100f) + 5; }
	}

	public int ElementalWard
	{
		get { return GetStat(Stat.ElementalWard); }
	}

	public int Speed
	{
		get
		{
			return GetStat(Stat.Speed);
		}
	}

	public int MaxHp { get; private set; }

	public DamageDetails TakeDamage(Attack move, Kreeture attacker)
	{
		float critical = 1f;
		if (Random.value * 100f <= 6.25f)
			critical = 2f;

		float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

		var damageDetails = new DamageDetails()
		{
			TypeEffectiveness = type,
			Critical = critical,
			Fainted = false
		};

		float attack = (move.Base.Category == MoveCategory.Special) ? attacker.ElementalStrike : attacker.Attack;
		float defense = (move.Base.Category == MoveCategory.Special) ? ElementalWard : Defense;

		float modifiers = Random.Range(0.85f, 1f) * type * critical;
		float a = (2 * attacker.Level + 10) / 250f;
		float d = a * move.Base.Power * ((float)attack / defense) + 2;
		int damage = Mathf.FloorToInt(d * modifiers);

		HP -= damage;
		if (HP <= 0)
		{
			HP = 0;
			damageDetails.Fainted = true;
		}

		return damageDetails;
	}

	public Attack GetRandomMove()
	{
		int r = Random.Range(0, Attacks.Count);
		return Attacks[r];
	}
}

public class DamageDetails
{
	public bool Fainted { get; set; }
	public float Critical { get; set; }
	public float TypeEffectiveness { get; set; }
}