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
}