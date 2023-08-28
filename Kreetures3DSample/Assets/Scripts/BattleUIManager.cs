using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleUIManager : MonoBehaviour
{
	public TextMeshProUGUI mainText;
	public Button[] actionButtons;
	public Button[] attackButtons; // Added array for attack buttons
	public Color highlightedColor;

	private bool isAttackTurn = false;

	private bool typingCoroutineRunning = false;

	public BattleManager battleManager;

	public TextMeshProUGUI enemyName;
	public Slider enemyHPBar;

	public TextMeshProUGUI kreetureName;
	public TextMeshProUGUI hpText;
	public Slider playerHPBar;

	private Attack enemyAttack = null;
	private Attack playerSelectedAttack = null;

	private bool hasPlayerGone = false;
	private bool hasEnemyGone = false;

	private int currentButtonIndex = 0;
	private InputAction navigateUpAction;
	private InputAction navigateDownAction;
	private InputAction navigateLeftAction;
	private InputAction navigateRightAction;

	private enum BattleState
	{
		EnteringBattle,
		SendOutKreeture,
		WaitingForInput,
		SelectingAttack,
		PlayerTurn,
		EnemyTurn,
		EnemyKreetureDefeated,
		DisplayEffectiveness
	}

	private BattleState currentBattleState = BattleState.EnteringBattle;

	private string messageToDisplay = "";
	private int currentCharIndex = 0;
	private float typingSpeed = 0.05f;
	private float lastTypingTime;

	private void Start()
	{
		Kreeture enemyKreeture = battleManager.activeEnemyKreeture;
		string enemyKreetureName = enemyKreeture.kreetureName;

		enemyName.text = enemyKreetureName;
		SetMessageToDisplay("A wild " + enemyKreetureName + " appeared!");
		SetSliderValues(enemyHPBar, enemyKreeture);
		currentCharIndex = 0;

		Kreeture playerKreeture = battleManager.activeKreeture;

		kreetureName.text = playerKreeture.name;
		hpText.text = playerKreeture.currentHP + "/" + playerKreeture.baseHP;
		SetSliderValues(playerHPBar, playerKreeture);

		SetBattleState(BattleState.EnteringBattle);
		SetupButtons();
		SetupNavigationActions();
	}

	public void SetSliderValues(Slider slider, Kreeture kreeture)
	{
		slider.maxValue = kreeture.baseHP;
		slider.value = kreeture.currentHP;
	}

	private void Update()
	{
		if (Keyboard.current.enterKey.wasPressedThisFrame)
		{
			if (!isAttackTurn)
			{
				HandleEnterKeyPress();
			}

		}

		// Start the coroutine if the typing effect is not done
		if (currentCharIndex < messageToDisplay.Length)
		{
			if (!typingCoroutineRunning)
			{
				typingCoroutineRunning = true;
				StartCoroutine(TypeWriterCoroutine());
			}
		}
	}

	private void HandleInput()
	{
		if (Keyboard.current.enterKey.wasPressedThisFrame)
		{
			HandleEnterKeyPress();
		}
	}

	private void SetupButtons()
	{
		foreach (Button button in actionButtons)
		{
			button.onClick.AddListener(() => OnActionButtonClick(button));
		}

		DisableAttackButtons();

		// Highlight the first attack button initially
		HighlightButton(0);
	}

	private void DisableAttackButtons()
	{
		foreach (Button button in attackButtons)
		{
			button.gameObject.SetActive(false);
		}
	}

	private void EnableActionButtons()
	{
		foreach (Button button in actionButtons)
		{
			button.gameObject.SetActive(true);
		}
	}


	private void SetupNavigationActions()
	{
		navigateUpAction = new InputAction("NavigateUp", InputActionType.Button, "<Keyboard>/w", null);
		navigateDownAction = new InputAction("NavigateDown", InputActionType.Button, "<Keyboard>/s", null);
		navigateLeftAction = new InputAction("NavigateLeft", InputActionType.Button, "<Keyboard>/a", null);
		navigateRightAction = new InputAction("NavigateRight", InputActionType.Button, "<Keyboard>/d", null);

		navigateUpAction.AddBinding("<Keyboard>/w");
		navigateUpAction.AddBinding("<Keyboard>/upArrow");

		navigateDownAction.AddBinding("<Keyboard>/s");
		navigateDownAction.AddBinding("<Keyboard>/downArrow");

		navigateLeftAction.AddBinding("<Keyboard>/a");
		navigateLeftAction.AddBinding("<Keyboard>/leftArrow");

		navigateRightAction.AddBinding("<Keyboard>/d");
		navigateRightAction.AddBinding("<Keyboard>/rightArrow");

		navigateUpAction.performed += ctx => Navigate(-2); // Move up
		navigateDownAction.performed += ctx => Navigate(2); // Move down
		navigateLeftAction.performed += ctx => Navigate(-1); // Move left
		navigateRightAction.performed += ctx => Navigate(1); // Move right

		EnableNavigation();
	}

	public void EnableNavigation()
	{
		navigateUpAction.Enable();
		navigateDownAction.Enable();
		navigateLeftAction.Enable();
		navigateRightAction.Enable();
	}

	private void DisableNavigation()
	{
		navigateUpAction.Disable();
		navigateDownAction.Disable();
		navigateLeftAction.Disable();
		navigateRightAction.Disable();
	}

	private void HandleEnterKeyPress()
	{
		if (currentBattleState == BattleState.WaitingForInput)
		{
			actionButtons[currentButtonIndex].onClick.Invoke();
		}
		else if (currentBattleState == BattleState.SelectingAttack)
		{
			attackButtons[currentButtonIndex].onClick.Invoke();
		}
	}

	private void OnActionButtonClick(Button button)
	{
		if (button.name == "Fight")
		{
			mainText.text = "Select an attack!";
			currentBattleState = BattleState.SelectingAttack;

			SetupAttackButtons();
		}
		else if (button.name == "Run")
		{
			RunAction();
		}
	}

	public void RunAction()
	{
		string previousSceneName = GameManager.Instance.GetPreviousScene();

		SceneManager.LoadScene(previousSceneName);

		// Respawn the player at the encounter position
		Vector3 encounterPosition = GameManager.Instance.GetPlayerPosition();
		Quaternion encounterRotation = GameManager.Instance.GetPlayerRotation();
		PlayerSpawner playerSpawner = FindObjectOfType<PlayerSpawner>();
		if (playerSpawner != null)
		{
			navigateUpAction.Disable();
			navigateDownAction.Disable();
			navigateLeftAction.Disable();
			navigateRightAction.Disable();
			playerSpawner.SpawnPlayerAtPosition(encounterPosition, encounterRotation);
		}
	}

	private void Navigate(int direction)
	{
		if (currentBattleState == BattleState.WaitingForInput)
		{
			int maxIndex = actionButtons.Length - 1;

			if (direction == -1) // Left
			{
				if (currentButtonIndex == 1)
				{
					UnhighlightButton(currentButtonIndex);
					currentButtonIndex = 0;
				}
				else if (currentButtonIndex == 3)
				{
					UnhighlightButton(currentButtonIndex);
					currentButtonIndex = 2;
				}
			}
			else if (direction == 1) // Right
			{
				if (currentButtonIndex == 0)
				{
					UnhighlightButton(currentButtonIndex);
					currentButtonIndex = 1;
				}
				else if (currentButtonIndex == 2)
				{
					UnhighlightButton(currentButtonIndex);
					currentButtonIndex = 3;
				}
			}
			else if (direction == -2) // Up
			{
				if (currentButtonIndex == 2)
				{
					UnhighlightButton(currentButtonIndex);
					currentButtonIndex = 0;
				}
				else if (currentButtonIndex == 3)
				{
					UnhighlightButton(currentButtonIndex);
					currentButtonIndex = 1;
				}
			}
			else if (direction == 2) // Down
			{
				if (currentButtonIndex == 0)
				{
					UnhighlightButton(currentButtonIndex);
					currentButtonIndex = 2;
				}
				else if (currentButtonIndex == 1)
				{
					UnhighlightButton(currentButtonIndex);
					currentButtonIndex = 3;
				}
			}

			HighlightButton(currentButtonIndex);
		}
		else if (currentBattleState == BattleState.SelectingAttack)
		{
			int totalAttackButtons = attackButtons.Length;

			UnhighlightAttackButtons(); // Unhighlight the current attack button

			if (direction == -1) // Left
			{
				currentButtonIndex = (currentButtonIndex == 1 || currentButtonIndex == 3) ? currentButtonIndex - 1 : currentButtonIndex;
			}
			else if (direction == 1) // Right
			{
				currentButtonIndex = (currentButtonIndex == 0 || currentButtonIndex == 2) ? currentButtonIndex + 1 : currentButtonIndex;
			}
			else if (direction == -2) // Up
			{
				currentButtonIndex = (currentButtonIndex == 2 || currentButtonIndex == 3) ? currentButtonIndex - 2 : currentButtonIndex;
			}
			else if (direction == 2) // Down
			{
				currentButtonIndex = (currentButtonIndex == 0 || currentButtonIndex == 1) ? currentButtonIndex + 2 : currentButtonIndex;
			}

			HighlightAttackButton(currentButtonIndex); // Highlight the new attack button
		}
	}

	private void HighlightButton(int index)
	{
		actionButtons[index].image.color = highlightedColor;
	}

	private void UnhighlightButton(int index)
	{
		actionButtons[index].image.color = Color.white;
	}

	private void UnhighlightAttackButtons()
	{
		foreach (Button attackButton in attackButtons)
		{
			attackButton.image.color = Color.white;
		}
	}

	private IEnumerator SuperEffectiveCoroutine(string message)
	{
		int index = 0;

		while (index < message.Length)
		{
			if (Time.time - lastTypingTime > typingSpeed)
			{
				mainText.text += message[index];
				lastTypingTime = Time.time;
				index++;
			}
		}

		yield return new WaitForSeconds(0.4f); // Adjust the time as needed
	}

	private IEnumerator TypeWriterCoroutine()
	{
		while (currentCharIndex < messageToDisplay.Length)
		{
			if (Time.time - lastTypingTime > typingSpeed)
			{
				mainText.text += messageToDisplay[currentCharIndex];
				lastTypingTime = Time.time;
				currentCharIndex++;
			}

			yield return null; // Yielding null lets the coroutine continue from the next frame
		}

		//Pause for a moment so player can read message
		yield return new WaitForSeconds(0.4f); // Adjust the time as needed

		switch (currentBattleState)
		{
			case BattleState.WaitingForInput:
				isAttackTurn = false;
				break;
			case BattleState.EnteringBattle:
				SetBattleState(BattleState.SendOutKreeture);
				SetMessageToDisplay("Go " + battleManager.activeKreeture.kreetureName + "!");
				typingCoroutineRunning = false;
				break;
			case BattleState.SendOutKreeture:
				SetBattleState(BattleState.WaitingForInput);
				SetMessageToDisplay("What would you like to do?");
				typingCoroutineRunning = false;
				break;
			case BattleState.EnemyTurn:
				yield return new WaitForSeconds(.5f);
				StartCoroutine(PerformEnemyAttack());
				typingCoroutineRunning = false;
				break;
			case BattleState.PlayerTurn:
				yield return new WaitForSeconds(.5f);
				StartCoroutine(PerformPlayerAttack());
				break;
			case BattleState.DisplayEffectiveness:
				CheckIfRoundOver();
				typingCoroutineRunning = false;
				break;
			case BattleState.EnemyKreetureDefeated:
				
				break;
			default:
				break;
		}
		// Typing effect is done, perform any actions you need here
		// For example, transitioning to the next state, playing animations, etc.
	}


	private void CheckIfRoundOver()
	{

		bool battleOver = IsBattleOver(battleManager.playerTeam, battleManager.activeEnemyKreeture);
		if (battleOver)
		{
			if (currentBattleState == BattleState.EnemyKreetureDefeated)
			{
				SetMessageToDisplay(battleManager.activeEnemyKreeture.kreetureName + " was defeated!");
				typingCoroutineRunning = false;
				isAttackTurn = false;
			}
		}

		if (hasPlayerGone && !hasEnemyGone)
		{
			HandleEnemyTurn();
		}
		else if (hasEnemyGone && !hasPlayerGone)
		{
			HandlePlayerTurn();
		}
		else if (hasEnemyGone && hasPlayerGone && !battleOver)
		{
			hasEnemyGone = false;
			hasPlayerGone = false;
			Debug.Log("New round");
			SetBattleState(BattleState.WaitingForInput);
			SetMessageToDisplay("What would you like to do?");
			EnableActionButtons();
			EnableNavigation();
		}
	}

	private IEnumerator PerformPlayerAttack()
	{
		GameObject kreetureGameObject = battleManager.KreetureGameObject;
		Animator playerKreetureAnimator = kreetureGameObject.GetComponent<Animator>();

		var enemyKreeture = GameManager.Instance.kreetureForBattle;

		// Wait for the animation to finish (optional)		
		int damage = CalculateDamage(GameManager.Instance.playerTeam[0], enemyKreeture, playerSelectedAttack);

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

		StartCoroutine(UpdateHealthBarOverTime(enemyHPBar, enemyKreeture, damage, playerSelectedAttack));
	}
	private IEnumerator PerformEnemyAttack()
	{
		Kreeture enemyKreeture = battleManager.activeEnemyKreeture;

		GameObject enemyKreetureGameObject = battleManager.EnemyKreetureGameObject;

		Animator enemyAnimator = enemyKreetureGameObject.GetComponent<Animator>();

		Kreeture playerKreeture = battleManager.playerTeam[0];

		int enemyDamage = CalculateDamage(enemyKreeture, playerKreeture, enemyAttack);

		Debug.Log("Enemy hit player for " + enemyDamage + "Damage");

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
		StartCoroutine(UpdateHealthBarOverTime(playerHPBar, playerKreeture, enemyDamage, enemyAttack));
	}

	private void SetMessageToDisplay(string message)
	{
		mainText.text = "";
		messageToDisplay = message;
		currentCharIndex = 0;
	}

	private void SetupAttackButtons()
	{
		for (int i = 0; i < attackButtons.Length; i++)
		{
			attackButtons[i].gameObject.SetActive(true);
			Button attackButton = attackButtons[i];
			TextMeshProUGUI buttonText = attackButton.GetComponentInChildren<TextMeshProUGUI>();

			if (i < battleManager.activeKreeture.knownAttacks.Count)
			{
				Attack attack = battleManager.activeKreeture.knownAttacks[i];
				buttonText.text = attack.name; // Assuming attackName is a field in your Attack scriptable object
				attackButton.gameObject.SetActive(true);
			}
			else
			{
				attackButton.gameObject.SetActive(false);
				HighlightAttackButton(0);
			}
		}

		//Disable the action buttons and set highlighting on the attack buttons
		foreach (var button in actionButtons)
		{
			button.gameObject.SetActive(false);
		}
	}

	private void SetBattleState(BattleState battleState)
	{
		currentBattleState = battleState;
	}

	private void HighlightAttackButton(int index)
	{
		if (index >= 0 && index < attackButtons.Length)
		{
			UnhighlightAttackButtons();
			attackButtons[index].image.color = highlightedColor;
			currentButtonIndex = index;
		}
	}

	private void OnAttackButtonClick(Button button)
	{
		//If attack button is clicked and the gamestate is correct
		if (currentBattleState == BattleState.SelectingAttack)
		{
			int selectedAttackIndex = currentButtonIndex;
			playerSelectedAttack = GameManager.Instance.playerTeam[0].knownAttacks[selectedAttackIndex];

			Kreeture targetKreeture = GameManager.Instance.kreetureForBattle;

			bool isPlayerTurn = DetermineTurnOrder(battleManager.playerTeam[0], targetKreeture);

			DisableAttackButtons();
			DisableNavigation();

			if (isPlayerTurn)
			{

				//Set state based on text displayed								
				// Player's turn logic
				Debug.Log("Players turn");
				HandlePlayerTurn();

				if (!IsBattleOver(battleManager.playerTeam, battleManager.activeEnemyKreeture))
				{
					// Enemy's turn logic
					HandleEnemyTurn();
				}
			}
			else
			{
				// Enemy's turn logic first, then player's turn
				HandleEnemyTurn();
			}
		}
	}

	private void HandlePlayerTurn()
	{
		// Trigger the "AttackTrigger" animation
		//For now grab first one

		SetBattleState(BattleState.PlayerTurn);

		List<Kreeture> playerTeam = battleManager.GetPlayerTeam();
		var activeKreeture = playerTeam[0];

		// Play the attack sound effect		
		SetMessageToDisplay(activeKreeture.kreetureName + " Used " + playerSelectedAttack.name);
		typingCoroutineRunning = false;
	}

	private void HandleEnemyTurn()
	{
		SetBattleState(BattleState.EnemyTurn);
		// Play enemy attack animation

		var enemyKreeture = battleManager.activeEnemyKreeture;

		Attack enemySelectedAttack = DetermineEnemyAttack(enemyKreeture);

		enemyAttack = enemySelectedAttack;

		SetMessageToDisplay(enemyKreeture.kreetureName + " used " + enemySelectedAttack.name);
		typingCoroutineRunning = false;
	}


	private IEnumerator UpdateHealthBarOverTime(Slider healthBar, Kreeture kreeture, int damage, Attack selectedAttack)
	{
		isAttackTurn = true;
		float elapsedTime = 0f;
		float duration = 1.0f; // Adjust this duration to control the animation speed

		int initialHealth = kreeture.currentHP;

		Debug.Log(kreeture.kreetureName + " Initial Health " + initialHealth);

		int targetHealth = Mathf.Max(kreeture.currentHP - damage, 0);

		Debug.Log("target health" + targetHealth);

		while (elapsedTime < duration)
		{
			float normalizedTime = elapsedTime / duration;
			float newHealth = Mathf.Lerp(initialHealth, targetHealth, normalizedTime);

			if (Mathf.Abs(newHealth) < 0.001f) // Adjust the threshold as needed
			{
				newHealth = 0f;
			}


			// Update the slider value
			healthBar.value = newHealth;

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Ensure that the slider value is set to the target health after the animation
		healthBar.value = targetHealth;

		kreeture.TakeDamage(damage, selectedAttack.attackType, kreeture.Types);

		if (currentBattleState == BattleState.PlayerTurn)
		{
			hasPlayerGone = true;
		}
		else if (currentBattleState == BattleState.EnemyTurn)
		{
			hasEnemyGone = true;
		}

		typingCoroutineRunning = false;
		yield return new WaitForSeconds(1f); // Adjust the time as needed		
		if (battleManager.GetSuperEffectiveHit())
		{
			//if super effective set new state and store this state as the last state
			SetBattleState(BattleState.DisplayEffectiveness);
			SetMessageToDisplay("It was super effective!");
			battleManager.SetSuperEffectiveHit(false);
		}
		else
		{
			CheckIfRoundOver();
		}
	}

	private bool IsBattleOver(List<Kreeture> playerTeam, Kreeture enemyKreeture)
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
			typingCoroutineRunning = false;
			SetBattleState(BattleState.EnemyKreetureDefeated);
		}

		return playerTeamDefeated || enemyDefeated;
	}

	private bool DetermineTurnOrder(Kreeture playerKreeture, Kreeture enemyKreeture)
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

	private Attack DetermineEnemyAttack(Kreeture enemyKreeture)
	{
		// Choose a random attack from the enemy's known attacks
		int randomAttackIndex = Random.Range(0, enemyKreeture.knownAttacks.Count);
		Attack selectedAttack = enemyKreeture.knownAttacks[randomAttackIndex];

		return selectedAttack;
	}

	private int CalculateDamage(Kreeture attacker, Kreeture defender, Attack attack)
	{
		// Calculate damage based on attacker's stats, defender's stats, attack power, and type effectiveness
		float effectiveness = TypeEffectivenessCalculator.CalculateEffectiveness(attack.attackType, defender.Types);
		float attackPower = attacker.attack;
		float defensePower = defender.defense;

		if (effectiveness > 1)
		{
			battleManager.SetSuperEffectiveHit(true);
		}
		// Calculate the damage using a formula (you can adjust this formula based on your game mechanics)
		int damage = Mathf.RoundToInt((attackPower / defensePower) * attack.power * effectiveness);

		return damage;
	}
}