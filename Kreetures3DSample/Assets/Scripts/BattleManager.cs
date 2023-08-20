using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
	public Transform playerSpawnPosition;
	public Transform enemySpawnPosition;

	private void Start()
	{
		List<Kreeture> playerTeam = GameManager.Instance.playerTeam;
		Kreeture enemyKreeture = GameManager.Instance.kreetureForBattle;

		if (playerTeam.Count > 0)
		{
			Kreeture playerKreeture = playerTeam[0];
			Instantiate(playerKreeture.modelPrefab, playerSpawnPosition.position, Quaternion.identity);
		}
		else
		{
			Debug.LogError("Player team is empty.");
		}

		if (enemyKreeture != null)
		{
			Instantiate(enemyKreeture.modelPrefab, enemySpawnPosition.position, Quaternion.Euler(0f, 180f, 0f));
		}
		else
		{
			Debug.LogError("Enemy Kreeture for battle is null.");
		}
	}
}