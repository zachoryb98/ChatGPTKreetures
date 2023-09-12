using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum BattleState { Start, ActionSelection, MovelSelection, PerformMove, Busy, PartyScreen, BattleOver }

public class BattleSystem : MonoBehaviour
{
	[SerializeField] InputActionAsset inputActions;

	[SerializeField] BattleUnit playerUnit;
	[SerializeField] BattleUnit enemyUnit;
	
	[SerializeField] BattleDialogBox dialogBox;
	[SerializeField] PartyScreen partyScreen;

	BattleState state;
	int currentAction;
	int currentAttack;
	int currentMember;

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

		partyScreen.Init();

		dialogBox.SetMoveNames(playerUnit.Kreeture.Attacks);

		yield return dialogBox.TypeDialog($"A wild {enemyUnit.Kreeture.Base.Name} appeared.");

		ActionSelection();
	}

	void ChooseFirstTurn()
	{
		if (playerUnit.Kreeture.Speed >= enemyUnit.Kreeture.Speed)
			ActionSelection();
		else
			StartCoroutine(EnemyMove());
	}

	void BattleOver()
	{
		state = BattleState.BattleOver;
		playerParty.Kreetures.ForEach(k => k.OnBattleOver());
		ExitBattle();
	}

	void ActionSelection()
	{
		state = BattleState.ActionSelection;
		dialogBox.SetDialog("Choose an action");
		dialogBox.EnableActionSelector(true);
	}

	void OpenPartyScreen()
	{
		state = BattleState.PartyScreen;
		partyScreen.SetPartyData(playerParty.kreetures);
		partyScreen.gameObject.SetActive(true);
	}

	void MoveSelection()
	{
		state = BattleState.MovelSelection;
		dialogBox.EnableActionSelector(false);
		dialogBox.EnableDialogText(false);
		dialogBox.EnableMoveSelector(true);
	}

	IEnumerator PlayerMove()
	{
		state = BattleState.PerformMove;

		var attack = playerUnit.Kreeture.Attacks[currentAttack];

		yield return RunMove(playerUnit, enemyUnit, attack);

		if(state == BattleState.PerformMove)
		{
			StartCoroutine(EnemyMove());
		}
	}

	IEnumerator EnemyMove()
	{
		state = BattleState.PerformMove;

		var move = enemyUnit.Kreeture.GetRandomMove();

		yield return RunMove(enemyUnit, playerUnit, move);

		if(state == BattleState.PerformMove)
		{
			ActionSelection();
		}
	}

	IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Attack attack)
	{
		attack.PP--;
		yield return dialogBox.TypeDialog($"{sourceUnit.Kreeture.Base.Name} used {attack.Base.Name}");

		sourceUnit.PlayAttackAnimation();
		yield return new WaitForSeconds(1f);
		targetUnit.PlayHitAnimation();

		if (attack.Base.Category == MoveCategory.Status)
		{
			yield return RunMoveEffects(attack, sourceUnit.Kreeture, targetUnit.Kreeture);
		}
		else
		{
			var damageDetails = targetUnit.Kreeture.TakeDamage(attack, sourceUnit.Kreeture);
			yield return targetUnit.Hud.UpdateHP();
			yield return ShowDamageDetails(damageDetails);
		}

		if (targetUnit.Kreeture.HP <= 0)
		{
			yield return dialogBox.TypeDialog($"{targetUnit.Kreeture.Base.Name} Fainted");
			targetUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);

			CheckForBattleOver(targetUnit);
		}
	}

	IEnumerator RunMoveEffects(Attack attack, Kreeture source, Kreeture target)
	{
		var effects = attack.Base.Effects;
		if (effects.Boosts != null)
		{
			if (attack.Base.Target == MoveTarget.Self)
				source.ApplyBoosts(effects.Boosts);
			else
				target.ApplyBoosts(effects.Boosts);
		}

		yield return ShowStatusChanges(source);
		yield return ShowStatusChanges(target);
	}

	IEnumerator ShowStatusChanges(Kreeture kreeture)
	{
		while (kreeture.StatusChanges.Count > 0)
		{
			var message = kreeture.StatusChanges.Dequeue();
			yield return dialogBox.TypeDialog(message);
		}
	}

	void CheckForBattleOver(BattleUnit faintedUnit)
	{
		if (faintedUnit.IsPlayerUnit)
		{
			var nextKreeture = playerParty.GetHealthyKreeture();
			if(nextKreeture != null)
			{
				OpenPartyScreen();
			}
		}
		else
		{
			BattleOver();
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
		if (state == BattleState.ActionSelection)
		{
			HandleActionSelection();
		}
		else if (state == BattleState.MovelSelection)
		{
			HandleMoveSelection();
		}
		else if (state == BattleState.PartyScreen)
		{
			HandlePartySelection();
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
				MoveSelection();
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
			StartCoroutine(PlayerMove());
		}
		else if (backAction.triggered)
		{
			dialogBox.EnableMoveSelector(false);
			dialogBox.EnableDialogText(true);
			ActionSelection();
		}
	}

	void HandlePartySelection()
	{
		var moveLeftAction = inputActions["NavigateLeft"];
		var moveRightAction = inputActions["NavigateRight"];
		var moveUpAction = inputActions["NavigateUp"];
		var moveDownAction = inputActions["NavigateDown"];
		var confirmAction = inputActions["Confirm"];
		var backAction = inputActions["Back"];


		if (moveRightAction.triggered)
			++currentMember;
		else if (moveLeftAction.triggered)
			--currentMember;
		else if (moveDownAction.triggered)
			currentMember += 2;
		else if (moveUpAction.triggered)
			currentMember -= 2;

		currentMember = Mathf.Clamp(currentMember, 0, playerParty.Kreetures.Count - 1);

		partyScreen.UpdateMemberSelection(currentMember);

		if (confirmAction.triggered)
		{
			var selectedMember = playerParty.Kreetures[currentMember];
			if (selectedMember.HP <= 0)
			{
				partyScreen.SetMessageText("You can't send out a fainted pokemon");
				return;
			}
			if (selectedMember == playerUnit.Kreeture)
			{
				partyScreen.SetMessageText("You can't switch with the same pokemon");
				return;
			}

			partyScreen.gameObject.SetActive(false);
			state = BattleState.Busy;
			StartCoroutine(SwitchKreeture(selectedMember));
		}
		else if (backAction.triggered)
		{
			partyScreen.gameObject.SetActive(false);
			ActionSelection();
		}
	}

	IEnumerator SwitchKreeture(Kreeture newKreeture)
	{
		bool currentKreetureFainted = true;
		if (playerUnit.Kreeture.HP > 0)
		{
			currentKreetureFainted = false;
			yield return dialogBox.TypeDialog($"Come back {playerUnit.Kreeture.Base.Name}");
			playerUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);

			playerUnit.DestroyFaintedModel();
		}

		playerUnit.Setup(newKreeture);		
		dialogBox.SetMoveNames(newKreeture.Attacks);
		yield return dialogBox.TypeDialog($"Go {newKreeture.Base.Name}!");

		if (currentKreetureFainted)
			ChooseFirstTurn();
		else
			StartCoroutine(EnemyMove());
	}

	public void ExitBattle()
	{
		string previousSceneName = GameManager.Instance.GetPreviousScene();

		SceneManager.LoadScene(previousSceneName);
	}
}
