using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleUIManager : MonoBehaviour
{
	public static BattleUIManager Instance;

	public TextMeshProUGUI mainText;
	public Button[] actionButtons;
	public Button[] attackButtons; // Added array for attack buttons
	public Color highlightedColor;

	private bool isAttackTurn = false;

	private bool typingCoroutineRunning = false;

	public BattleManager battleManager;

	public TextMeshProUGUI enemyName;
	public TextMeshProUGUI enemyLvlText;
	public Slider enemyHPBar;

	public TextMeshProUGUI kreetureName;
	public TextMeshProUGUI hpText;
	public TextMeshProUGUI xpText;
	public TextMeshProUGUI lvlText;

	public Slider playerHPBar;
	public Slider playerXPBar;

	private int playerDamageDealtToEnemy;
	private Attack playerSelectedAttack = null;

	private bool hasPlayerGone = false;
	private bool hasEnemyGone = false;

	private int currentButtonIndex = 0;
	private InputAction navigateUpAction;
	private InputAction navigateDownAction;
	private InputAction navigateLeftAction;
	private InputAction navigateRightAction;
	
	private bool isWaitingForInput = false;

	public GameObject StatsDisplay;
	public GameObject PlayerInfo;
	public List<TextMeshProUGUI> currentStatsTextBoxes;
	public List<TextMeshProUGUI> newStatsTextBoxes;

	private string messageToDisplay = "";
	private int currentCharIndex = 0;
	private float typingSpeed = 0.05f;
	private float lastTypingTime;

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
	}

	private void Start()
	{		
		//Get enemy Kreeture
		Kreeture enemyKreeture = battleManager.activeEnemyKreeture;
		string enemyKreetureName = enemyKreeture.kreetureName;

		//Set enemy Kreeture UI Info
		enemyName.text = enemyKreetureName;
		SetMessageToDisplay("A wild " + enemyKreetureName + " appeared!");
		SetHPSliderValues(enemyHPBar, enemyKreeture);
		currentCharIndex = 0;
		enemyLvlText.text = "Lvl " + enemyKreeture.currentLevel;

		//Get Player Kreeture
		Kreeture playerKreeture = battleManager.activeKreeture;

		//Set Player Kreeture Info
		kreetureName.text = playerKreeture.name;
		hpText.text = playerKreeture.currentHP + "/" + playerKreeture.baseHP;
		SetHPSliderValues(playerHPBar, playerKreeture);
		SetXPSlider(playerXPBar, playerKreeture);
		lvlText.text = "Lvl " + playerKreeture.currentLevel;
		xpText.text = playerKreeture.currentXP + "/" + playerKreeture.xpRequiredForNextLevel;

		//Set up controls for UI. LIKELY NEEDS REDONE
		SetupButtons();
		SetupNavigationActions();
	}

	public void SetHPSliderValues(Slider slider, Kreeture kreeture)
	{
		slider.maxValue = kreeture.baseHP;
		slider.value = kreeture.currentHP;
	}

	public void SetXPSlider(Slider slider, Kreeture kreeture)
	{
		slider.maxValue = kreeture.xpRequiredForNextLevel;
		slider.value = kreeture.currentXP;
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

		//Start the coroutine if the typing effect is not done
		if (currentCharIndex < messageToDisplay.Length)
		{
			if (!typingCoroutineRunning)
			{
				typingCoroutineRunning = true;
				StartCoroutine(TypeWriterCoroutine());
			}
		}
		//else if (currentBattleState == BattleState.DisplayStats)
		//{
		//	PlayerInfo.SetActive(false);

		//	UpdateStatsUI();
		//	StatsDisplay.SetActive(true);

		//	battleManager.activeKreeture.UpdateStats();
		//	StartCoroutine(WaitThenGo());			
		//}
	}


	//METHOD NEEDS FIXED
	private IEnumerator WaitThenGo()
	{
		yield return new WaitForSeconds(3.0f);
		StatsDisplay.SetActive(false);
		PlayerInfo.SetActive(true);

		//SetBattleState(BattleState.IncreaseXP);
		StartCoroutine(UpdateXPBarOverTime(playerXPBar, battleManager.activeKreeture, battleManager.activeKreeture.xpTransfer));
	}

	private void UpdateStatsUI()
	{
		//Get current stats before level up
		List<int> currentStats = battleManager.activeKreeture.GetCurrentStats();
		List<int> newStats = battleManager.activeKreeture.GetNewStats();

		//Set current stats
		for (int i = 0; i < currentStatsTextBoxes.Count; i++)
		{
			currentStatsTextBoxes[i].text = currentStats[i].ToString();
		}

		//Set new stats
		for (int i = 0; i < newStatsTextBoxes.Count; i++)
		{
			newStatsTextBoxes[i].text = newStats[i].ToString();
		}

	}

	private void UpdateDisplayHealth(int currentHealth, int baseHealth)
	{
		hpText.text = currentHealth + "/" + baseHealth;
	}

	private void UpdateDisplayXP(int currentXP, int xpToLevelUp)
	{
		xpText.text = currentXP + "/" + xpToLevelUp;
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
		if (battleManager.GetBattleState() == BattleManager.BattleState.WaitingForInput)
		{
			actionButtons[currentButtonIndex].onClick.Invoke();
		}
		else if (battleManager.GetBattleState() == BattleManager.BattleState.SelectingAttack)
		{
			attackButtons[currentButtonIndex].onClick.Invoke();
		}
	}

	private void OnActionButtonClick(Button button)
	{
		if (button.name == "Fight")
		{
			mainText.text = "Select an attack!";
			battleManager.SetBattleState(BattleManager.BattleState.SelectingAttack);

			SetupAttackButtons();
		}
		else if (button.name == "Run")
		{
			ExitBattle();
		}
	}

	public void HandlePlayerLoss()
	{
		string lastHealScene = GameManager.Instance.GetLastHealScene();

		SceneManager.LoadScene(lastHealScene);

		GameManager.Instance.playerDefeated = true;

		// Respawn the player at the encounter position
		Vector3 spawnPosition = GameManager.Instance.GetPlayerLastHealPosition();
		Quaternion spawnRotation = new Quaternion(0,0,0,0);
		PlayerSpawner playerSpawner = FindObjectOfType<PlayerSpawner>();
		if (playerSpawner != null)
		{
			navigateUpAction.Disable();
			navigateDownAction.Disable();
			navigateLeftAction.Disable();
			navigateRightAction.Disable();
			playerSpawner.SpawnPlayerAtPosition(spawnPosition, spawnRotation);
		}
	}

	public void ExitBattle()
	{
		string previousSceneName = GameManager.Instance.GetPreviousScene();

		SceneManager.LoadScene(previousSceneName);		
	}

	private void Navigate(int direction)
	{
		if (battleManager.GetBattleState() == BattleManager.BattleState.WaitingForInput)
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
		else if (battleManager.GetBattleState() == BattleManager.BattleState.SelectingAttack)
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

		switch (battleManager.GetBattleState())
		{
			case BattleManager.BattleState.WaitingForInput:
				isAttackTurn = false;
				break;
			case BattleManager.BattleState.EnteringBattle:
				battleManager.SetBattleState(BattleManager.BattleState.SendOutKreeture);
				SetMessageToDisplay("Go " + battleManager.activeKreeture.kreetureName + "!");
				yield return new WaitForSeconds(.5f);
				typingCoroutineRunning = false;
				break;
			case BattleManager.BattleState.SendOutKreeture:
				battleManager.SetBattleState(BattleManager.BattleState.WaitingForInput);
				SetMessageToDisplay("What would you like to do?");
				typingCoroutineRunning = false;
				yield return new WaitForSeconds(.5f);
				break;
			case BattleManager.BattleState.EnemyTurn:
				yield return new WaitForSeconds(.5f);
				StartCoroutine(BattleManager.Instance.PerformEnemyAttack());
				typingCoroutineRunning = false;
				break;
			case BattleManager.BattleState.PlayerTurn:
				yield return new WaitForSeconds(.5f);
				StartCoroutine(PerformPlayerAttack());				
				break;
			case BattleManager.BattleState.DisplayEffectiveness:
				CheckIfRoundOver();
				typingCoroutineRunning = false;
				break;
			case BattleManager.BattleState.EnemyKreetureDefeated:
				//Play feint animation
				int xp = battleManager.CalculateXPForDefeatedKreeture(battleManager.activeKreeture, battleManager.activeKreeture);
				SetMessageToDisplay(battleManager.activeKreeture.kreetureName + " gained " + xp + "XP!");
				typingCoroutineRunning = false;
				battleManager.SetBattleState(BattleManager.BattleState.IncreaseXP);
				break;
			case BattleManager.BattleState.IncreaseXP:
				int xpGained = battleManager.CalculateXPForDefeatedKreeture(battleManager.activeKreeture, battleManager.activeKreeture);
				StartCoroutine(UpdateXPBarOverTime(playerXPBar, battleManager.activeKreeture, xpGained));
				battleManager.activeKreeture.GainXP(xpGained);
				if (battleManager.activeKreeture.leveledUp)
				{
					battleManager.SetBattleState(BattleManager.BattleState.LevelUp);
					SetMessageToDisplay(battleManager.activeKreeture.kreetureName + " Leveled up to level " + (battleManager.activeKreeture.currentLevel) + "!");
					Debug.Log("XP remaining: " + battleManager.activeEnemyKreeture.currentXP);
					typingCoroutineRunning = false;
					battleManager.activeKreeture.leveledUp = false;
				}				
				break;
			case BattleManager.BattleState.LevelUp:
				//Play level up effect

				//Set to 0 since we leveled up
				playerXPBar.value = 0;
				//Update to proper xp number after level up				

				//Update stats after level up
				battleManager.SetBattleState(BattleManager.BattleState.DisplayStats);
				//Determine if battle is done and exit scene.
				
				break;
			case BattleManager.BattleState.PlayerDefeated:

				yield return new WaitForSeconds(1.5f);

				Debug.Log("Player Lost!");

				HandlePlayerLoss();
				break;
			default:
				break;
		}
		// Typing effect is done, perform any actions you need here
		// For example, transitioning to the next state, playing animations, etc.
	}


	private void CheckIfRoundOver()
	{

		bool battleOver = battleManager.IsBattleOver(battleManager.playerTeam, battleManager.activeEnemyKreeture);
		if (battleOver)
		{
			if (battleManager.GetBattleState() == BattleManager.BattleState.EnemyKreetureDefeated)
			{
				SetMessageToDisplay(battleManager.activeEnemyKreeture.kreetureName + " was defeated!");
				typingCoroutineRunning = false;
				isAttackTurn = false;
			}

			if (BattleManager.Instance.DetermineHasPlayerLost())
			{
				SetMessageToDisplay("You let your Kreetures faint");
				typingCoroutineRunning = false;
				battleManager.SetBattleState(BattleManager.BattleState.PlayerDefeated);				
			}
		}

		if (hasPlayerGone && !hasEnemyGone && !battleOver)
		{
			BattleManager.Instance.HandleEnemyTurn();
		}
		else if (hasEnemyGone && !hasPlayerGone && !battleOver)
		{
			HandlePlayerTurn();
		}
		else if (hasEnemyGone && hasPlayerGone && !battleOver)
		{
			hasEnemyGone = false;
			hasPlayerGone = false;
			Debug.Log("New round");
			battleManager.SetBattleState(BattleManager.BattleState.WaitingForInput);
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
		int damage = battleManager.CalculateDamage(GameManager.Instance.playerTeam[0], enemyKreeture, playerSelectedAttack);

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

	public void SetMessageToDisplay(string message)
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
		if (battleManager.GetBattleState() == BattleManager.BattleState.SelectingAttack)
		{
			int selectedAttackIndex = currentButtonIndex;
			playerSelectedAttack = GameManager.Instance.playerTeam[0].knownAttacks[selectedAttackIndex];

			Kreeture targetKreeture = GameManager.Instance.kreetureForBattle;

			bool isPlayerTurn = battleManager.DetermineTurnOrder(battleManager.playerTeam[0], targetKreeture);

			DisableAttackButtons();
			DisableNavigation();

			if (isPlayerTurn)
			{

				//Set state based on text displayed								
				// Player's turn logic
				Debug.Log("Players turn");
				HandlePlayerTurn();

				if (!battleManager.IsBattleOver(battleManager.playerTeam, battleManager.activeEnemyKreeture))
				{
					// Enemy's turn logic
					BattleManager.Instance.HandleEnemyTurn();
				}
			}
			else
			{
				// Enemy's turn logic first, then player's turn
				BattleManager.Instance.HandleEnemyTurn();
			}
		}
	}

	private void HandlePlayerTurn()
	{
		// Trigger the "AttackTrigger" animation
		//For now grab first one

		battleManager.SetBattleState(BattleManager.BattleState.PlayerTurn);

		List<Kreeture> playerTeam = battleManager.GetPlayerTeam();
		var activeKreeture = playerTeam[0];

		// Play the attack sound effect		
		SetMessageToDisplay(activeKreeture.kreetureName + " Used " + playerSelectedAttack.name);
		typingCoroutineRunning = false;
	}

	private IEnumerator UpdateXPBarOverTime(Slider xpBar, Kreeture kreeture, int xp)
	{
		isAttackTurn = true;
		float elapsedTime = 0f;
		float duration = 1.0f; // Adjust this duration to control the animation speed

		int initialXP = kreeture.currentXP;

		Debug.Log(kreeture.kreetureName + " Initial XP " + initialXP);

		int targetXP = Mathf.Max(kreeture.currentXP + xp, 0);

		if(targetXP > kreeture.xpRequiredForNextLevel)
		{
			kreeture.xpTransfer = targetXP - kreeture.xpRequiredForNextLevel;
			targetXP = kreeture.xpRequiredForNextLevel;			
		}

		

		Debug.Log("target xp" + targetXP);

		while (elapsedTime < duration)
		{
			float normalizedTime = elapsedTime / duration;
			float newXP = Mathf.Lerp(initialXP, targetXP, normalizedTime);

			// Update the slider value
			xpBar.value = newXP;

			UpdateDisplayXP(Mathf.FloorToInt(newXP), kreeture.xpRequiredForNextLevel);

			elapsedTime += Time.deltaTime;

			yield return null;
		}

		yield return new WaitForSeconds(2.0f);

		var battleState = battleManager.GetBattleState();
;
		//Determine if more to battle in the future
		if (battleState != BattleManager.BattleState.LevelUp && battleState != BattleManager.BattleState.DisplayStats)
		{
			ExitBattle();
		}		
	}

	public IEnumerator UpdateHealthBarOverTime(Slider healthBar, Kreeture kreeture, int damage, Attack selectedAttack)
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

			if (battleManager.GetBattleState() == BattleManager.BattleState.EnemyTurn)
			{
				UpdateDisplayHealth(Mathf.FloorToInt(newHealth), kreeture.baseHP);
			}


			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Ensure that the slider value is set to the target health after the animation
		healthBar.value = targetHealth;

		kreeture.TakeDamage(damage, selectedAttack.attackType, kreeture.Types);

		var battleState = battleManager.GetBattleState();

		if (battleState == BattleManager.BattleState.PlayerTurn)
		{
			hasPlayerGone = true;
		}
		else if (battleState == BattleManager.BattleState.EnemyTurn)
		{
			hasEnemyGone = true;			
		}

		if(battleManager.activeKreeture.currentHP == 0)
		{
			battleManager.IsBattleOver(battleManager.playerTeam, battleManager.activeEnemyKreeture);
		}

		//If both have gone update battle stats
		if(hasPlayerGone && hasEnemyGone)
		{
			//IMPLEMENT CRITICAL HITS, and STATUS EFFECT AFTER GET SUPER EFFECTIVE METHOD
			battleManager.RecordBattleData(playerDamageDealtToEnemy, BattleManager.Instance.GetDamageDealtToPlayer(), battleManager.GetSuperEffectiveHit(), false, false, battleManager.activeKreeture.currentHP);
		}

		typingCoroutineRunning = false;
		yield return new WaitForSeconds(1f); // Adjust the time as needed		
		if (battleManager.GetSuperEffectiveHit())
		{
			//if super effective set new state and store this state as the last state
			battleManager.SetBattleState(BattleManager.BattleState.DisplayEffectiveness);
			SetMessageToDisplay("It was super effective!");
			battleManager.SetSuperEffectiveHit(false);
		}
		else
		{
			CheckIfRoundOver();
		}
	}

	public void SetTypeCoroutineValue(bool value)
	{
		typingCoroutineRunning = value;
	}
}