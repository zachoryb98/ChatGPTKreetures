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
	public Condition Status { get; private set; }
	public int StatusTime { get; set; }
	public Condition VolatileStatus { get; private set; }
	public int VolatileStatusTime { get; set; }

	public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
	public bool HpChanged { get; set; }
	public event System.Action OnStatusChanged;

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

		ResetStatBoost();
		Status = null;
		VolatileStatus = null;
	}

	void CalculateStats()
	{
		Stats = new Dictionary<Stat, int>();
		Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
		Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
		Stats.Add(Stat.ElementalStrike, Mathf.FloorToInt((Base.ElmtStrike * Level) / 100f) + 5);
		Stats.Add(Stat.ElementalWard, Mathf.FloorToInt((Base.ElmtWard * Level) / 100f) + 5);
		Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

		MaxHp = Mathf.FloorToInt((Base.Speed * Level) / 100f) + 10 + Level;
	}

	void ResetStatBoost()
	{
		StatBoosts = new Dictionary<Stat, int>()
		{
			{Stat.Attack, 0},
			{Stat.Defense, 0},
			{Stat.ElementalStrike, 0},
			{Stat.ElementalWard, 0},
			{Stat.Speed, 0},
			{Stat.Accuracy, 0},
			{Stat.Evasion, 0},
		};
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
			
			if (boost > 0)
				StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
			else
				StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");

			Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
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

		UpdateHP(damage);

		return damageDetails;
	}

	public void UpdateHP(int damage)
	{
		HP = Mathf.Clamp(HP - damage, 0, MaxHp);
		HpChanged = true;
	}

	public void SetStatus(ConditionID conditionId)
	{
		if (Status != null) return;

		Status = ConditionsDB.Conditions[conditionId];
		
		Status?.OnStart?.Invoke(this);
		StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
		OnStatusChanged?.Invoke();
	}

	public void CureStatus()
	{
		Status = null;
		OnStatusChanged?.Invoke();
	}

	public void SetVolatileStatus(ConditionID conditionId)
	{
		if (VolatileStatus != null) return;

		VolatileStatus = ConditionsDB.Conditions[conditionId];
		VolatileStatus?.OnStart?.Invoke(this);
		StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
	}

	public void CureVolatileStatus()
	{
		VolatileStatus = null;
	}

	public Attack GetRandomMove()
	{
		int r = Random.Range(0, Attacks.Count);
		return Attacks[r];
	}

	public bool OnBeforeMove()
	{
		bool canPerformMove = true;
		if (Status?.OnBeforeMove != null)
		{
			if (!Status.OnBeforeMove(this))
				canPerformMove = false;
		}

		if (VolatileStatus?.OnBeforeMove != null)
		{
			if (!VolatileStatus.OnBeforeMove(this))
				canPerformMove = false;
		}

		return canPerformMove;
	}

	public void OnAfterTurn()
	{
		Status?.OnAfterTurn?.Invoke(this);
		VolatileStatus?.OnAfterTurn?.Invoke(this);
	}

	public void OnBattleOver()
	{
		VolatileStatus = null;
		ResetStatBoost();
	}
}

public class DamageDetails
{
	public bool Fainted { get; set; }
	public float Critical { get; set; }
	public float TypeEffectiveness { get; set; }
}