using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
	[SerializeField] InputActionAsset inputActions;

	[SerializeField] BattleUnit playerUnit;
	[SerializeField] BattleUnit enemyUnit;
	[SerializeField] BattleHud playerHud;
	[SerializeField] BattleHud enemyHud;
	[SerializeField] BattleDialogBox dialogBox;
	[SerializeField] PartyScreen partyScreen;

	BattleState state;
	int currentAction;
	int currentAttack;

	private void Awake()
	{
		// Enable the Input Actions
		inputActions.Enable();
	}

	KreetureParty playerParty;
	Kreeture wildKreeture;

	private void Start()
	{
		playerParty = GameManager.Instance.GetPlayerTeam();
		wildKreeture = GameManager.Instance.GetWildKreeture();
		StartCoroutine(SetupBattle());
	}

	private void OnDisable()
	{
		// Disable the Input Actions when the script is disabled
		inputActions.Disable();
	}

	public IEnumerator SetupBattle()
	{
		playerUnit.Setup(playerParty.GetHealthyKreeture());
		enemyUnit.Setup(wildKreeture);
		playerHud.SetData(playerUnit.Kreeture);
		enemyHud.SetData(enemyUnit.Kreeture);

		partyScreen.Init();

		dialogBox.SetMoveNames(playerUnit.Kreeture.Attacks);

		yield return dialogBox.TypeDialog($"A wild {enemyUnit.Kreeture.Base.Name} appeared.");

		PlayerAction();
	}

	void PlayerAction()
	{
		state = BattleState.PlayerAction;
		dialogBox.SetDialog("Choose an action");
		dialogBox.EnableActionSelector(true);
	}

	void OpenPartyScreen()
	{
		partyScreen.SetPartyData(playerParty.kreetures);
		partyScreen.gameObject.SetActive(true);
	}

	void PlayerMove()
	{
		state = BattleState.PlayerMove;
		dialogBox.EnableActionSelector(false);
		dialogBox.EnableDialogText(false);
		dialogBox.EnableMoveSelector(true);
	}

	IEnumerator PerformPlayerMove()
	{
		state = BattleState.Busy;

		var attack = playerUnit.Kreeture.Attacks[currentAttack];
		attack.PP--;
		yield return dialogBox.TypeDialog($"{playerUnit.Kreeture.Base.Name} used {attack.Base.Name}");

		playerUnit.PlayAttackAnimation();
		yield return new WaitForSeconds(1f);

		enemyUnit.PlayHitAnimation();
		yield return new WaitForSeconds(1f);

		var damageDetails = enemyUnit.Kreeture.TakeDamage(attack, playerUnit.Kreeture);
		yield return enemyHud.UpdateHP();
		yield return ShowDamageDetails(damageDetails);

		if (damageDetails.Fainted)
		{
			enemyUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(1f);
			yield return dialogBox.TypeDialog($"{enemyUnit.Kreeture.Base.Name} Fainted");
			ExitBattle();
		}
		else
		{
			StartCoroutine(EnemyMove());
		}
	}

	IEnumerator EnemyMove()
	{
		state = BattleState.EnemyMove;

		var move = enemyUnit.Kreeture.GetRandomMove();
		move.PP--;
		yield return dialogBox.TypeDialog($"{enemyUnit.Kreeture.Base.Name} used {move.Base.Name}");

		enemyUnit.PlayAttackAnimation();
		yield return new WaitForSeconds(1f);

		playerUnit.PlayHitAnimation();
		var damageDetails = playerUnit.Kreeture.TakeDamage(move, playerUnit.Kreeture);
		yield return playerHud.UpdateHP();
		yield return ShowDamageDetails(damageDetails);

		if (damageDetails.Fainted)
		{
			yield return dialogBox.TypeDialog($"{playerUnit.Kreeture.Base.Name} Fainted");
			playerUnit.PlayFaintAnimation();

			yield return new WaitForSeconds(2f);

			playerUnit.DestroyFaintedModel();

			var nextKreeture = playerParty.GetHealthyKreeture();
			if (nextKreeture != null)
			{
				playerUnit.Setup(nextKreeture);
				playerHud.SetData(nextKreeture);

				dialogBox.SetMoveNames(nextKreeture.Attacks);

				yield return dialogBox.TypeDialog($"Go {nextKreeture.Base.Name}!");

				PlayerAction();
			}
			else
			{
				ExitBattle();
			}
		}
		else
		{
			PlayerAction();
		}
	}

	IEnumerator ShowDamageDetails(DamageDetails damageDetails)
	{
		if (damageDetails.Critical > 1f)
			yield return dialogBox.TypeDialog("A critical hit!");
		if (damageDetails.TypeEffectiveness > 1f)
			yield return dialogBox.TypeDialog("It's super effective!");
		else if (damageDetails.TypeEffectiveness < 1f)
			yield return dialogBox.TypeDialog("It's not very effective!");
	}

	private void Update()
	{
		if (state == BattleState.PlayerAction)
		{
			HandleActionSelection();
		}
		else if (state == BattleState.PlayerMove)
		{
			HandleMoveSelection();
		}
	}

	void HandleActionSelection()
	{
		var moveLeftAction = inputActions["NavigateLeft"];
		var moveRightAction = inputActions["NavigateRight"];
		var moveUpAction = inputActions["NavigateUp"];
		var moveDownAction = inputActions["NavigateDown"];
		var confirmAction = inputActions["Confirm"];


		if (moveRightAction.triggered)
		{
			currentAction++;
		}

		if (moveLeftAction.triggered)
		{
			--currentAction;
		}

		if (moveDownAction.triggered)
		{
			currentAction += 2;
		}

		if (moveUpAction.triggered)
		{
			currentAction -= 2;
		}

		currentAction = Mathf.Clamp(currentAction, 0, 3);

		dialogBox.UpdateActionSelection(currentAction);


		currentAction = Mathf.Clamp(currentAction, 0, 3);

		if (confirmAction.triggered)
		{
			if (currentAction == 0)
			{
				// Fight
				PlayerMove();
			}
			else if (currentAction == 1)
			{
				//Kreeture Party
				OpenPartyScreen();
			}
			else if (currentAction == 2)
			{
				//Inventory
			}
			else if (currentAction == 3)
			{
				// Run
			}
		}
	}

	void HandleMoveSelection()
	{
		var moveLeftAction = inputActions["NavigateLeft"];
		var moveRightAction = inputActions["NavigateRight"];
		var moveUpAction = inputActions["NavigateUp"];
		var moveDownAction = inputActions["NavigateDown"];
		var confirmAction = inputActions["Confirm"];
		var backAction = inputActions["Back"];


		if (moveRightAction.triggered)
		{
			if (currentAttack < playerUnit.Kreeture.Attacks.Count - 1)
				++currentAttack;
		}

		if (moveLeftAction.triggered)
		{
			if (currentAttack > 0)
				--currentAttack;
		}

		if (moveDownAction.triggered)
		{
			if (currentAttack < playerUnit.Kreeture.Attacks.Count - 2)
				currentAttack += 2;
		}

		if (moveUpAction.triggered)
		{
			if (currentAttack > 1)
				currentAttack -= 2;
		}

		currentAttack = Mathf.Clamp(currentAttack, 0, playerUnit.Kreeture.Attacks.Count - 1);

		dialogBox.UpdateMoveSelection(currentAttack, playerUnit.Kreeture.Attacks[currentAttack]);

		if (confirmAction.triggered)
		{
			dialogBox.EnableMoveSelector(false);
			dialogBox.EnableDialogText(true);
			StartCoroutine(PerformPlayerMove());
		}
		else if (backAction.triggered)
		{
			dialogBox.EnableMoveSelector(false);
			dialogBox.EnableDialogText(true);
			PlayerAction();
		}
	}

	public void ExitBattle()
	{
		string previousSceneName = GameManager.Instance.GetPreviousScene();

		SceneManager.LoadScene(previousSceneName);
	}
}
