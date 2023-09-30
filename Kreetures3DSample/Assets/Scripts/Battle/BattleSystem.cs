using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver, AboutToUse }

public enum BattleAction { Move, SwitchKreeture, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
	[SerializeField] InputActionAsset inputActions;

	[SerializeField] BattleUnit playerUnit;
	[SerializeField] BattleUnit enemyUnit;

	[SerializeField] BattleDialogBox dialogBox;
	[SerializeField] PartyScreen partyScreen;

	BattleState state;
	BattleState? prevState;
	int currentAction;
	int currentAttack;
	int currentMember;
	bool aboutToUseChoice = true;

	private void Awake()
	{
		// Enable the Input Actions
		inputActions.Enable();
	}

	KreetureParty playerParty;
	KreetureParty trainerParty;
	Kreeture wildKreeture;

	bool isTrainerBattle = false;
	PlayerController player;
	TrainerController trainer;

	private void OnDisable()
	{
		// Disable the Input Actions when the script is disabled
		inputActions.Disable();
	}
	private void Start()
	{
		if (GameManager.Instance.GetIsTrainerBattle())
		{
			trainer = GameManager.Instance.GetTrainer();
			StartTrainerBattle(GameManager.Instance.GetPlayerTeam(), GameManager.Instance.GetEnemyTeam());
		}
		else
		{
			StartBattle(GameManager.Instance.GetPlayerTeam(), GameManager.Instance.GetWildKreeture());
		}
	}

	public void StartBattle(KreetureParty playerParty, Kreeture wildKreeture)
	{
		this.playerParty = playerParty;
		this.wildKreeture = wildKreeture;
		StartCoroutine(SetupBattle());
	}

	public IEnumerator SetupBattle()
	{
		playerUnit.Clear();
		enemyUnit.Clear();

		if (!isTrainerBattle)
		{
			// Wild Kreeture Battle
			playerUnit.Setup(playerParty.GetHealthyKreeture());
			enemyUnit.Setup(wildKreeture);

			dialogBox.SetMoveNames(playerUnit.Kreeture.Attacks);
			yield return dialogBox.TypeDialog($"A wild {enemyUnit.Kreeture.Base.Name} appeared.");
		}
		else
		{
			// Trianer Battle

			// Show trainer and player sprites
			playerUnit.gameObject.SetActive(false);
			enemyUnit.gameObject.SetActive(false);

			//playerImage.gameObject.SetActive(true);
			//trainerImage.gameObject.SetActive(true);
			//playerImage.sprite = player.Sprite;
			//trainerImage.sprite = trainer.Sprite;

			yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

			// Send out first Kreeture of the trainer
			//trainerImage.gameObject.SetActive(false);
			enemyUnit.gameObject.SetActive(true);
			var enemyKreeture = trainerParty.GetHealthyKreeture();
			enemyUnit.Setup(enemyKreeture);
			yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyKreeture.Base.Name}");

			// Send out first Kreeture of the player
			//playerImage.gameObject.SetActive(false);
			playerUnit.gameObject.SetActive(true);
			var playerKreeture = playerParty.GetHealthyKreeture();
			playerUnit.Setup(playerKreeture);
			yield return dialogBox.TypeDialog($"Go {playerKreeture.Base.Name}!");
			dialogBox.SetMoveNames(playerUnit.Kreeture.Attacks);
		}

		partyScreen.Init();
		ActionSelection();
	}

	public void StartTrainerBattle(KreetureParty playerParty, KreetureParty trainerParty)
	{
		this.playerParty = playerParty;
		this.trainerParty = trainerParty;

		isTrainerBattle = true;
		player = playerParty.GetComponent<PlayerController>();

		StartCoroutine(SetupBattle());
	}

	void BattleOver(bool playerWon)
	{

		//use playerWon to spawn player at health center

		state = BattleState.BattleOver;
		playerParty.Kreetures.ForEach(k => k.OnBattleOver());

		if (GameManager.Instance.GetIsTrainerBattle())
		{
			GameManager.Instance.SetTrainerLoss();
		}
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
		state = BattleState.MoveSelection;
		dialogBox.EnableActionSelector(false);
		dialogBox.EnableDialogText(false);
		dialogBox.EnableMoveSelector(true);
	}

	IEnumerator AboutToUse(Kreeture newKreeture)
	{
		state = BattleState.Busy;
		yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newKreeture.Base.Name}. Do you want to change kreetues?");		

		state = BattleState.AboutToUse;
		dialogBox.EnableChoiceBox(true);
	}

	IEnumerator RunTurns(BattleAction playerAction)
	{
		state = BattleState.RunningTurn;

		if (playerAction == BattleAction.Move)
		{
			playerUnit.Kreeture.CurrentAttack = playerUnit.Kreeture.Attacks[currentAttack];
			enemyUnit.Kreeture.CurrentAttack = enemyUnit.Kreeture.GetRandomMove();

			int playerMovePriority = playerUnit.Kreeture.CurrentAttack.Base.Priority;
			int enemyMovePriority = enemyUnit.Kreeture.CurrentAttack.Base.Priority;

			// Check who goes first
			bool playerGoesFirst = true;
			if (enemyMovePriority > playerMovePriority)
				playerGoesFirst = false;
			else if (enemyMovePriority == playerMovePriority)
				playerGoesFirst = playerUnit.Kreeture.Speed >= enemyUnit.Kreeture.Speed;

			var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
			var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

			var secondKreeture = secondUnit.Kreeture;

			// First Turn
			yield return RunMove(firstUnit, secondUnit, firstUnit.Kreeture.CurrentAttack);
			yield return RunAfterTurn(firstUnit);
			if (state == BattleState.BattleOver) yield break;

			if (secondKreeture.HP > 0)
			{
				// Second Turn
				yield return RunMove(secondUnit, firstUnit, secondUnit.Kreeture.CurrentAttack);
				yield return RunAfterTurn(secondUnit);
				if (state == BattleState.BattleOver) yield break;
			}
		}
		else
		{
			if (playerAction == BattleAction.SwitchKreeture)
			{
				var selectedKreeture = playerParty.Kreetures[currentMember];
				state = BattleState.Busy;
				yield return SwitchKreeture(selectedKreeture);
			}

			// Enemy Turn
			var enemyMove = enemyUnit.Kreeture.GetRandomMove();
			yield return RunMove(enemyUnit, playerUnit, enemyMove);
			yield return RunAfterTurn(enemyUnit);
			if (state == BattleState.BattleOver) yield break;
		}

		if (state != BattleState.BattleOver)
			ActionSelection();
	}

	IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Attack attack)
	{
		bool canRunMove = sourceUnit.Kreeture.OnBeforeMove();
		if (!canRunMove)
		{
			yield return ShowStatusChanges(sourceUnit.Kreeture);
			yield return sourceUnit.Hud.UpdateHP();
			yield break;
		}
		yield return ShowStatusChanges(sourceUnit.Kreeture);

		attack.PP--;
		yield return dialogBox.TypeDialog($"{sourceUnit.Kreeture.Base.Name} used {attack.Base.Name}");

		if (CheckIfMoveHits(attack, sourceUnit.Kreeture, targetUnit.Kreeture))
		{

			sourceUnit.PlayAttackAnimation();
			yield return new WaitForSeconds(1f);
			targetUnit.PlayHitAnimation();

			if (attack.Base.Category == MoveCategory.Status)
			{
				RunMoveEffects(attack.Base.Effects, sourceUnit.Kreeture, targetUnit.Kreeture, attack.Base.Target);
			}
			else
			{
				var damageDetails = targetUnit.Kreeture.TakeDamage(attack, sourceUnit.Kreeture);
				yield return targetUnit.Hud.UpdateHP();
				yield return ShowDamageDetails(damageDetails);
			}

			if (attack.Base.Secondaries != null && attack.Base.Secondaries.Count > 0 && targetUnit.Kreeture.HP > 0)
			{
				foreach (var secondary in attack.Base.Secondaries)
				{
					var rnd = UnityEngine.Random.Range(1, 101);
					if (rnd <= secondary.Chance)
						yield return RunMoveEffects(secondary, sourceUnit.Kreeture, targetUnit.Kreeture, secondary.Target);
				}
			}

			if (targetUnit.Kreeture.HP <= 0)
			{
				yield return dialogBox.TypeDialog($"{targetUnit.Kreeture.Base.Name} Fainted");
				targetUnit.PlayFaintAnimation();
				yield return new WaitForSeconds(2f);

				targetUnit.DestroyFaintedModel();

				CheckForBattleOver(targetUnit);
			}

			// Statuses like burn or psn will hurt the Kreeture after the turn
			sourceUnit.Kreeture.OnAfterTurn();
			yield return ShowStatusChanges(sourceUnit.Kreeture);
			yield return sourceUnit.Hud.UpdateHP();
			if (sourceUnit.Kreeture.HP <= 0)
			{
				yield return dialogBox.TypeDialog($"{sourceUnit.Kreeture.Base.Name} Fainted");
				sourceUnit.PlayFaintAnimation();
				yield return new WaitForSeconds(2f);

				CheckForBattleOver(sourceUnit);
			}
		}
		else
		{
			yield return dialogBox.TypeDialog($"{sourceUnit.Kreeture.Base.Name}'s attack missed");
		}
	}

	IEnumerator RunMoveEffects(MoveEffects effects, Kreeture source, Kreeture target, AttackTarget moveTarget)
	{
		// Stat Boosting
		if (effects.Boosts != null)
		{
			if (moveTarget == AttackTarget.Self)
				source.ApplyBoosts(effects.Boosts);
			else
				target.ApplyBoosts(effects.Boosts);
		}

		// Status Condition
		if (effects.Status != ConditionID.none)
		{
			target.SetStatus(effects.Status);
		}

		// Volatile Status Condition
		if (effects.VolatileStatus != ConditionID.none)
		{
			target.SetVolatileStatus(effects.VolatileStatus);
		}

		yield return ShowStatusChanges(source);
		yield return ShowStatusChanges(target);
	}

	IEnumerator RunAfterTurn(BattleUnit sourceUnit)
	{
		if (state == BattleState.BattleOver) yield break;
		yield return new WaitUntil(() => state == BattleState.RunningTurn);

		// Statuses like burn or psn will hurt the Kreeture after the turn
		sourceUnit.Kreeture.OnAfterTurn();
		yield return ShowStatusChanges(sourceUnit.Kreeture);
		yield return sourceUnit.Hud.UpdateHP();
		if (sourceUnit.Kreeture.HP <= 0)
		{
			yield return dialogBox.TypeDialog($"{sourceUnit.Kreeture.Base.Name} Fainted");
			sourceUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);

			CheckForBattleOver(sourceUnit);
			yield return new WaitUntil(() => state == BattleState.RunningTurn);
		}
	}

	bool CheckIfMoveHits(Attack attack, Kreeture source, Kreeture target)
	{
		if (attack.Base.AlwaysHits)
			return true;

		float moveAccuracy = attack.Base.Accuracy;

		int accuracy = source.StatBoosts[Stat.Accuracy];
		int evasion = target.StatBoosts[Stat.Evasion];

		var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

		if (accuracy > 0)
			moveAccuracy *= boostValues[accuracy];
		else
			moveAccuracy /= boostValues[-accuracy];

		if (evasion > 0)
			moveAccuracy /= boostValues[evasion];
		else
			moveAccuracy *= boostValues[-evasion];

		return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
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
			if (nextKreeture != null)
				OpenPartyScreen();
			else
				BattleOver(false);
		}
		else
		{
			if (!isTrainerBattle)
			{
				BattleOver(true);
			}
			else
			{
				var nextKreeture = trainerParty.GetHealthyKreeture();
				if (nextKreeture != null)
					StartCoroutine(AboutToUse(nextKreeture));
				else
					BattleOver(true);
			}
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
		else if (state == BattleState.MoveSelection)
		{
			HandleMoveSelection();
		}
		else if (state == BattleState.PartyScreen)
		{
			HandlePartySelection();
		}
		else if (state == BattleState.AboutToUse)
		{
			HandleAboutToUse();
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
				prevState = state;
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
			var move = playerUnit.Kreeture.Attacks[currentAttack];
			if (move.PP == 0) return;

			dialogBox.EnableMoveSelector(false);
			dialogBox.EnableDialogText(true);
			StartCoroutine(RunTurns(BattleAction.Move));
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
				partyScreen.SetMessageText("You can't send out a fainted Kreeture");
				return;
			}
			if (selectedMember == playerUnit.Kreeture)
			{
				partyScreen.SetMessageText("You can't switch with the same Kreeture");
				return;
			}

			partyScreen.gameObject.SetActive(false);
			if (prevState == BattleState.ActionSelection)
			{
				
				prevState = null;
				StartCoroutine(RunTurns(BattleAction.SwitchKreeture));
			}
			else
			{
				state = BattleState.Busy;
				StartCoroutine(SwitchKreeture(selectedMember));
			}
		}
		else if (backAction.triggered)
		{
			partyScreen.gameObject.SetActive(false);
			ActionSelection();
		}
	}

	void HandleAboutToUse()
	{
		var moveUpAction = inputActions["NavigateUp"];
		var moveDownAction = inputActions["NavigateDown"];
		var confirmAction = inputActions["Confirm"];
		var backAction = inputActions["Back"];

		if (moveUpAction.triggered || moveDownAction.triggered)
			aboutToUseChoice = !aboutToUseChoice;

		dialogBox.UpdateChoiceBox(aboutToUseChoice);

		if (confirmAction.triggered)
		{
			dialogBox.EnableChoiceBox(false);
			if (aboutToUseChoice == true)
			{
				// Yes Option
				prevState = BattleState.AboutToUse;
				OpenPartyScreen();
			}
			else
			{
				// No Option
				StartCoroutine(SendNextTrainerKreeture());
			}
		}
		else if (backAction.triggered)
		{
			dialogBox.EnableChoiceBox(false);
			StartCoroutine(SendNextTrainerKreeture());
		}
	}

	IEnumerator SwitchKreeture(Kreeture newKreeture)
	{
		if (playerUnit.Kreeture.HP > 0)
		{
			yield return dialogBox.TypeDialog($"Come back {playerUnit.Kreeture.Base.Name}");
			playerUnit.PlayFaintAnimation();
			yield return new WaitForSeconds(2f);

			playerUnit.DestroyFaintedModel();
		}

		playerUnit.Setup(newKreeture);
		dialogBox.SetMoveNames(newKreeture.Attacks);
		yield return dialogBox.TypeDialog($"Go {newKreeture.Base.Name}!");

		if (prevState == null)
		{
			state = BattleState.RunningTurn;
		}
		else if (prevState == BattleState.AboutToUse)
		{
			prevState = null;
			StartCoroutine(SendNextTrainerKreeture());
		}
	}

	IEnumerator SendNextTrainerKreeture()
	{
		state = BattleState.Busy;

		
		var nextKreeture = trainerParty.GetHealthyKreeture();
		enemyUnit.Setup(nextKreeture);
		yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextKreeture.Base.Name}!");

		state = BattleState.RunningTurn;
	}

	public void ExitBattle()
	{
		string previousSceneName = GameManager.Instance.GetPreviousScene();

		SceneManager.LoadScene(previousSceneName);
	}
}
