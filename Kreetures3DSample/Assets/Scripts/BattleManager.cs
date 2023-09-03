using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
	public Transform playerSpawnPosition;
	public Transform enemySpawnPosition;

	public List<Kreeture> playerTeam = null;

	private bool SuperEffectiveHit = false;

	public Kreeture activeKreeture;
	public Kreeture activeEnemyKreeture = null;

	public GameObject KreetureGameObject = null;
	public GameObject EnemyKreetureGameObject = null;

	private int turnsTaken;
	private int totalDamageDealt;
	private int totalDamageReceived;
	private int effectiveMoveCount;
	private int criticalHits;
	private int statusEffectsApplied;
	private int remainingHealth;

	private void Awake()
	{
		playerTeam = GetPlayerTeam();
		//Will need to get first healthy kreeture at some point
		activeKreeture = playerTeam[0];

		activeEnemyKreeture = GameManager.Instance.kreetureForBattle;

		activeEnemyKreeture.currentHP = activeEnemyKreeture.baseHP;

		if (playerTeam.Count > 0)
		{
			Kreeture playerKreeture = playerTeam[0];
			KreetureGameObject = Instantiate(playerKreeture.modelPrefab, playerSpawnPosition.position, Quaternion.identity);
		}
		else
		{
			Debug.LogError("Player team is empty.");
		}

		if (activeEnemyKreeture != null)
		{
			EnemyKreetureGameObject = Instantiate(activeEnemyKreeture.modelPrefab, enemySpawnPosition.position, Quaternion.Euler(0f, 180f, 0f));
		}
		else
		{
			Debug.LogError("Enemy Kreeture for battle is null.");
		}
	}

	public bool GetSuperEffectiveHit()
	{
		return SuperEffectiveHit;
	}

	public void SetSuperEffectiveHit(bool result)
	{
		SuperEffectiveHit = result;
	}


	public List<Kreeture> GetPlayerTeam()
	{
		return GameManager.Instance.GetPlayerTeam();
	}

	public int CalculateBattlePerformance()
	{
		int performance = 0;

		// Calculate performance based on various factors
		performance += turnsTaken * 5; // Reward fewer turns
		performance += totalDamageDealt / 10; // Reward more damage dealt
		performance -= totalDamageReceived / 15; // Penalize more damage received
		performance += effectiveMoveCount * 20; // Reward effective moves
		performance += criticalHits * 15; // Reward critical hits
		performance += statusEffectsApplied * 10; // Reward status effects
		performance += remainingHealth / 5; // Reward more remaining health

		return performance;
	}

	// Method to record battle data during the battle
	public void RecordBattleData(int damageDealt, int damageReceived, bool isEffectiveMove, bool isCriticalHit, bool hasAppliedStatusEffect, int remainingPlayerHealth)
	{
		turnsTaken++;
		totalDamageDealt += damageDealt;
		totalDamageReceived += damageReceived;

		if (isEffectiveMove)
		{
			effectiveMoveCount++;
		}

		if (isCriticalHit)
		{
			criticalHits++;
		}

		if (hasAppliedStatusEffect)
		{
			statusEffectsApplied++;
		}

		remainingHealth = remainingPlayerHealth;
	}
}