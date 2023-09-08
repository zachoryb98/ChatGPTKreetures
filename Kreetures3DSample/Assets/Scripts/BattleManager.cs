using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleManager : MonoBehaviour
{
	public static BattleManager Instance;

	[Header("Where to place the Kreetures")]
	public Transform playerSpawnPosition;
	public Transform enemySpawnPosition;

	[Header("Player Team")]
	public List<Kreeture> playerTeam = null;
	public Kreeture activeKreeture;
	public GameObject KreetureGameObject = null;

	[Header("Enemy Team")]
	public Kreeture activeEnemyKreeture = null;
	public GameObject EnemyKreetureGameObject = null;

	[Header("BattleState variables")]
	private bool hasPlayerLost = false;
	private bool SuperEffectiveHit = false;
	private BattleState currentBattleState = BattleState.EnteringBattle;
	private bool hasPlayerGone = false;
	private bool hasEnemyGone = false;

	[Header("Random Things I need stored")]
	private Attack playerSelectedAttack = null;
	private Attack enemyAttack = null;
	private int enemyDamageDealtToPlayer;
	private bool isAttackTurn = false;


	[Header("Variables To Calculate XP for battle conditions")]
	private int turnsTaken;
	private int totalDamageDealt;
	private int totalDamageReceived;
	private int effectiveMoveCount;
	private int criticalHits;
	private int statusEffectsApplied;
	private int remainingHealth;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;			
		}
		else
		{
			Destroy(gameObject); // Destroy any duplicate GameManager instances
		}

		playerTeam = GetPlayerTeam();
		//Will need to get first healthy kreeture at some point
		activeKreeture = playerTeam[0];


		//TODO make this a list of Kreetures
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

	private void Start()
	{
		//First battle state should be entering battle
		SetBattleState(BattleState.EnteringBattle);
	}

	public IEnumerator HandleTypingCompletionCoroutine()
	{
		switch (GetBattleState())
		{
			case BattleState.WaitingForInput:
				Instance.SetAttackTurn(false);
				break;
			case BattleState.EnteringBattle:
				SetBattleState(BattleState.SendOutKreeture);
				BattleUIManager.Instance.SetMessageToDisplay("Go " + activeKreeture.kreetureName + "!");
				yield return new WaitForSeconds(.5f);
				BattleUIManager.Instance.SetTypeCoroutineValue(false);
				break;
			case BattleState.SendOutKreeture:
				SetBattleState(BattleState.WaitingForInput);
				BattleUIManager.Instance.SetMessageToDisplay("What would you like to do?");
				BattleUIManager.Instance.SetTypeCoroutineValue(false);
				yield return new WaitForSeconds(.5f);
				break;
			case BattleState.EnemyTurn:
				yield return new WaitForSeconds(.5f);
				StartCoroutine(PerformEnemyAttack());
				BattleUIManager.Instance.SetTypeCoroutineValue(false);
				break;
			case BattleState.PlayerTurn:
				yield return new WaitForSeconds(.5f);
				StartCoroutine(PerformPlayerAttack());
				break;
			case BattleState.DisplayEffectiveness:
				CheckIfRoundOver();
				BattleUIManager.Instance.SetTypeCoroutineValue(false);
				break;
			case BattleState.EnemyKreetureDefeated:
				//Play feint animation
				int xp = CalculateXPForDefeatedKreeture(activeKreeture, activeKreeture);
				BattleUIManager.Instance.SetMessageToDisplay(activeKreeture.kreetureName + " gained " + xp + "XP!");
				BattleUIManager.Instance.SetTypeCoroutineValue(false);
				SetBattleState(BattleState.IncreaseXP);
				break;
			case BattleState.IncreaseXP:
				int xpGained = CalculateXPForDefeatedKreeture(activeKreeture, activeKreeture);
				StartCoroutine(BattleUIManager.Instance.UpdateXPBarOverTime(BattleUIManager.Instance.playerXPBar, activeKreeture, xpGained));
				activeKreeture.GainXP(xpGained);
				if (activeKreeture.leveledUp)
				{
					SetBattleState(BattleState.LevelUp);
					BattleUIManager.Instance.SetMessageToDisplay(activeKreeture.kreetureName + " Leveled up to level " + (activeKreeture.currentLevel) + "!");
					Debug.Log("XP remaining: " + activeEnemyKreeture.currentXP);
					BattleUIManager.Instance.SetTypeCoroutineValue(false);
					activeKreeture.leveledUp = false;
				}
				break;
			case BattleState.LevelUp:
				//Play level up effect

				//Set to 0 since we leveled up
				BattleUIManager.Instance.playerXPBar.value = 0;
				//Update to proper xp number after level up				

				//Update stats after level up
				SetBattleState(BattleState.DisplayStats);
				//Determine if battle is done and exit scene.

				break;
			case BattleState.PlayerDefeated:

				yield return new WaitForSeconds(1.5f);

				Debug.Log("Player Lost!");

				HandlePlayerLoss();
				break;
			default:
				break;
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

	public int GetDamageDealtToPlayer()
	{
		return enemyDamageDealtToPlayer;
	}

	public bool DetermineTurnOrder(Kreeture playerKreeture, Kreeture enemyKreeture)
	{
		// Calculate the speed of the player's Kreeture and the enemy's Kreeture
		int playerSpeed = playerKreeture.agility;
		int enemySpeed = enemyKreeture.agility;

		// Compare the speeds to determine the turn order
		if (playerSpeed > enemySpeed)
		{
			return true; // Player's Kreeture attacks first
		}
		else if (enemySpeed > playerSpeed)
		{
			return false; // Enemy's Kreeture attacks first
		}
		else
		{
			// If both speeds are equal, you can introduce randomness for variety
			return Random.value < 0.5f; // 50% chance for player's Kreeture to attack first
		}
	}

	public void SetHasPlayerGone(bool result)
	{
		hasPlayerGone = result;
	}

	public bool GetHasPlayerGone()
	{
		return hasPlayerGone;
	}

	public void SetHasEnemyGone(bool result)
	{
		hasEnemyGone = result;
	}

	public bool GetHasEnemyGone()
	{
		return hasEnemyGone;
	}

	public void SetAttackTurn(bool result)
	{
		isAttackTurn = result;
	}

	public bool GetAttackTurn()
	{
		return isAttackTurn;
	}

	public void SetPlayerAttack(Attack attack)
	{
		playerSelectedAttack = attack;
	}

	public Attack GetPlayerAttack()
	{
		return playerSelectedAttack;
	}

	public Attack DetermineEnemyAttack(Kreeture enemyKreeture)
	{
		// Choose a random attack from the enemy's known attacks
		int randomAttackIndex = Random.Range(0, enemyKreeture.knownAttacks.Count);
		Attack selectedAttack = enemyKreeture.knownAttacks[randomAttackIndex];

		return selectedAttack;
	}

	public IEnumerator PerformPlayerAttack()
	{
		Animator playerKreetureAnimator = KreetureGameObject.GetComponent<Animator>();

		var enemyKreeture = GameManager.Instance.kreetureForBattle;

		// Wait for the animation to finish (optional)		
		int damage = CalculateDamage(GameManager.Instance.playerTeam[0], enemyKreeture, BattleManager.Instance.GetPlayerAttack());

		playerKreetureAnimator.Play("Bite Attack");

		AnimationClip biteAttackClip = null; // Assign the actual animation clip here
		AnimationClip[] clips = playerKreetureAnimator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip clip in clips)
		{
			if (clip.name == "Bite Attack")
			{
				biteAttackClip = clip;
				break;
			}
		}

		// Wait for the animation to finish
		if (biteAttackClip != null)
		{
			yield return new WaitForSeconds(biteAttackClip.length);
		}
		else
		{
			Debug.LogWarning("Bite Attack animation clip not found!");
		}

		Debug.Log("player hit enemy for " + damage + " Damage");

		StartCoroutine(BattleUIManager.Instance.UpdateHealthBarOverTime(BattleUIManager.Instance.enemyHPBar, enemyKreeture, damage, BattleManager.Instance.GetPlayerAttack()));
	}

	public IEnumerator PerformEnemyAttack()
	{

		Animator enemyAnimator = EnemyKreetureGameObject.GetComponent<Animator>();

		int enemyDamage = CalculateDamage(activeKreeture, activeKreeture, enemyAttack);

		Debug.Log("Enemy hit player for " + enemyDamage + "Damage");

		enemyDamageDealtToPlayer = enemyDamage;

		enemyAnimator.Play("Bite Attack");

		AnimationClip biteAttackClip = null; // Assign the actual animation clip here
		AnimationClip[] clips = enemyAnimator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip clip in clips)
		{
			if (clip.name == "Bite Attack")
			{
				biteAttackClip = clip;
				break;
			}
		}

		// Wait for the animation to finish
		if (biteAttackClip != null)
		{
			yield return new WaitForSeconds(biteAttackClip.length);
		}
		else
		{
			Debug.LogWarning("Bite Attack animation clip not found!");
		}

		//Update Healthbar, deal damage, and switch turn
		StartCoroutine(BattleUIManager.Instance.UpdateHealthBarOverTime(BattleUIManager.Instance.playerHPBar, activeKreeture, enemyDamage, enemyAttack));
	}

	public void HandlePlayerTurn()
	{
		//Set BattleState
		SetBattleState(BattleState.PlayerTurn);

		// Play the attack sound effect		
		BattleUIManager.Instance.SetMessageToDisplay(activeKreeture.kreetureName + " Used " + playerSelectedAttack.name);
		BattleUIManager.Instance.SetTypeCoroutineValue(false); ;
	}

	public void HandleEnemyTurn()
	{
		SetBattleState(BattleManager.BattleState.EnemyTurn);
		// Play enemy attack animation

		var enemyKreeture = activeEnemyKreeture;

		Attack enemySelectedAttack = DetermineEnemyAttack(enemyKreeture);

		enemyAttack = enemySelectedAttack;

		BattleUIManager.Instance.SetMessageToDisplay(enemyKreeture.kreetureName + " used " + enemySelectedAttack.name);
		BattleUIManager.Instance.SetTypeCoroutineValue(false);
	}

	public int CalculateDamage(Kreeture attacker, Kreeture defender, Attack attack)
	{
		// Calculate damage based on attacker's stats, defender's stats, attack power, and type effectiveness
		float effectiveness = TypeEffectivenessCalculator.CalculateEffectiveness(attack.attackType, defender.Types);
		float attackPower = attacker.attack;
		float defensePower = defender.defense;

		if (effectiveness > 1)
		{
			SetSuperEffectiveHit(true);
		}
		// Calculate the damage using a formula (you can adjust this formula based on your game mechanics)
		int damage = Mathf.RoundToInt((attackPower / defensePower) * attack.power * effectiveness);

		return damage;
	}

	public int CalculateXPForDefeatedKreeture(Kreeture playerKreeture, Kreeture defeatedKreeture)
	{
		int baseXP = 50; // Base XP value

		int levelDifference = defeatedKreeture.currentLevel - playerKreeture.currentLevel;
		float levelScalingFactor = 1.0f + (levelDifference * 0.1f); // Adjust the scaling factor as needed

		int xp = Mathf.RoundToInt(baseXP * levelScalingFactor);

		// Apply additional modifiers (type effectiveness, battle performance, etc.)

		return xp;
	}

	public void SetBattleState(BattleState battleState)
	{
		currentBattleState = battleState;
	}

	public bool DetermineHasPlayerLost()
	{
		return hasPlayerLost;
	}

	public void CheckIfRoundOver()
	{

		bool battleOver = IsBattleOver(playerTeam, activeEnemyKreeture);
		if (battleOver)
		{
			if (GetBattleState() == BattleState.EnemyKreetureDefeated)
			{
				BattleUIManager.Instance.SetMessageToDisplay(activeEnemyKreeture.kreetureName + " was defeated!");
				BattleUIManager.Instance.SetTypeCoroutineValue(false);
				isAttackTurn = false;
			}

			if (BattleManager.Instance.DetermineHasPlayerLost())
			{
				BattleUIManager.Instance.SetMessageToDisplay("You let your Kreetures faint");
				BattleUIManager.Instance.SetTypeCoroutineValue(false);
				SetBattleState(BattleState.PlayerDefeated);
			}
		}

		if (hasPlayerGone && !hasEnemyGone && !battleOver)
		{
			BattleManager.Instance.HandleEnemyTurn();
		}
		else if (hasEnemyGone && !hasPlayerGone && !battleOver)
		{
			BattleManager.Instance.HandlePlayerTurn();
		}
		else if (hasEnemyGone && hasPlayerGone && !battleOver)
		{
			hasEnemyGone = false;
			hasPlayerGone = false;
			Debug.Log("New round");
			SetBattleState(BattleState.WaitingForInput);
			BattleUIManager.Instance.SetMessageToDisplay("What would you like to do?");
			BattleUIManager.Instance.EnableActionButtons();
			BattleUIManager.Instance.EnableNavigation();
		}
	}

	public bool IsBattleOver(List<Kreeture> playerTeam, Kreeture enemyKreeture)
	{
		bool playerTeamDefeated = true;

		foreach (Kreeture kreeture in playerTeam)
		{
			if (kreeture.currentHP > 0)
			{
				playerTeamDefeated = false;
				break;
			}
		}

		bool enemyDefeated = enemyKreeture.currentHP <= 0;

		if (enemyDefeated)
		{
			BattleUIManager.Instance.SetTypeCoroutineValue(false);
			SetBattleState(BattleManager.BattleState.EnemyKreetureDefeated);
		}

		if (playerTeamDefeated)
		{
			hasPlayerLost = true;
		}

		return playerTeamDefeated || enemyDefeated;
	}

	public void HandlePlayerLoss()
	{
		string lastHealScene = GameManager.Instance.GetLastHealScene();

		SceneManager.LoadScene(lastHealScene);

		GameManager.Instance.playerDefeated = true;

		// Respawn the player at the encounter position
		Vector3 spawnPosition = GameManager.Instance.GetPlayerLastHealPosition();
		Quaternion spawnRotation = new Quaternion(0, 0, 0, 0);
		PlayerSpawner playerSpawner = FindObjectOfType<PlayerSpawner>();
		if (playerSpawner != null)
		{
			BattleUIManager.Instance.DisableNavigation();
			playerSpawner.SpawnPlayerAtPosition(spawnPosition, spawnRotation);
		}
	}

	public void ExitBattle()
	{
		string previousSceneName = GameManager.Instance.GetPreviousScene();

		SceneManager.LoadScene(previousSceneName);
	}

	public BattleState GetBattleState()
	{
		return currentBattleState;
	}

	public enum BattleState
	{
		EnteringBattle,
		SendOutKreeture,
		WaitingForInput,
		SelectingAttack,
		PlayerTurn,
		EnemyTurn,
		EnemyKreetureDefeated,
		DisplayEffectiveness,
		IncreaseXP,
		LevelUp,
		PlayerDefeated,
		DisplayStats
	}
}
