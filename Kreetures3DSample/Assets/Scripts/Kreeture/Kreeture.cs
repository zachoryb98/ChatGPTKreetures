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

	public void Init()
	{
		HP = MaxHp;

		// Generate Moves
		Attacks = new List<Attack>();
		foreach (var attack in Base.LearnableAttacks)
		{
			if (attack.Level <= Level)
				Attacks.Add(new Attack(attack.Base));

			if (Attacks.Count >= 4)
				break;
		}
	}

	public int Attack
	{
		get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
	}

	public int Defense
	{
		get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
	}

	public int SpAttack
	{
		get { return Mathf.FloorToInt((Base.ElmtStrike * Level) / 100f) + 5; }
	}

	public int SpDefense
	{
		get { return Mathf.FloorToInt((Base.ElmtWard * Level) / 100f) + 5; }
	}

	public int Speed
	{
		get
		{
			return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5;
		}
	}

	public int MaxHp
	{
		get
		{
			return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10;
		}
	}

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

		float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
		float defense = (move.Base.IsSpecial) ? SpDefense : Defense;

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