using System;
using System.Collections.Generic;
using UnityEngine;

public class Attack
{
	public AttackBase Base { get; set; }
	public int PP { get; set; }

	public Attack(AttackBase kBase)
	{
		Base = kBase;
		PP = kBase.PP;
	}

	public Attack(AttackSaveData saveData)
	{		
		Base = AttackDB.GetAttackByName(saveData.name);
		PP = saveData.pp;
	}

	public AttackSaveData GetSaveData()
	{
		var saveData = new AttackSaveData()
		{
			name = Base.Name,
			pp = PP
		};
		return saveData;
	}
}

[Serializable]
public class AttackSaveData
{
	public string name;
	public int pp;
}