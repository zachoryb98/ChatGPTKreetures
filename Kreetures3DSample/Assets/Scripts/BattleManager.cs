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

	private float CalculateTypeMultiplier(KreetureType attackerType, KreetureType defenderType)
	{
		if (attackerType == KreetureType.Aqua)
		{
			if (defenderType == KreetureType.Flame || defenderType == KreetureType.Earth || defenderType == KreetureType.Stone)
			{
				return 1.5f; // Strong against Flame, Earth, Stone
			}
			else if (defenderType == KreetureType.Volt || defenderType == KreetureType.Terra)
			{
				return 0.75f; // Weak against Volt, Terra
			}
		}
		// Add similar checks for other types and interactions
		// ...

		return 1.0f; // Default case: no type advantage/disadvantage
	}

	public void UseAttack(Attack attackerMove, Kreeture attacker, Kreeture defender)
	{
		// Calculate type multiplier
		float typeMultiplier = CalculateTypeMultiplier((KreetureType)attackerMove.attackType, defender.kreetureType);

		// Calculate damage
		int damage = Mathf.FloorToInt(attackerMove.power * typeMultiplier);


		// Deal damage to defender and update UI
		// ...
	}
}