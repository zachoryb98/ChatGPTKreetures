using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
	public TextMeshProUGUI mainText;
	public Button[] actionButtons;
	public Button[] attackButtons; // Added array for attack buttons
	public Color highlightedColor;

	private int currentButtonIndex = 0;
	private InputAction navigateUpAction;
	private InputAction navigateDownAction;
	private InputAction navigateLeftAction;
	private InputAction navigateRightAction;
	private List<Kreeture> kreetures;

	private enum BattleState
	{
		EnteringBattle,
		WaitingForInput,
		SelectingAttack
	}

	private BattleState currentBattleState = BattleState.EnteringBattle;

	private string messageToDisplay = "";
	private int currentCharIndex = 0;
	private float typingSpeed = 0.05f;
	private float lastTypingTime;

	private void Start()
	{
		kreetures = GameManager.Instance.GetKreetureNames();
		SetMessageToDisplay("A wild " + kreetures[0].name + " appeared!");
		currentCharIndex = 0; // Reset the typing effect index

		SetupButtons();
		SetupNavigationActions();
	}

	private void Update()
	{
		if (Keyboard.current.enterKey.wasPressedThisFrame)
		{
			HandleEnterKeyPress();
		}

		TypeWriterEffect();
		UpdateMessageDisplay();
	}

	private void SetupButtons()
	{
		foreach (Button button in actionButtons)
		{
			button.onClick.AddListener(() => OnActionButtonClick(button));
		}

		foreach (Button button in attackButtons)
		{
			button.gameObject.SetActive(false);
		}

		// Highlight the first attack button initially
		HighlightButton(0);
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

		navigateUpAction.Enable();
		navigateDownAction.Enable();
		navigateLeftAction.Enable();
		navigateRightAction.Enable();
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
		else
		{
			// Handle other action button clicks here
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

	private void TypeWriterEffect()
	{
		if (currentCharIndex < messageToDisplay.Length)
		{
			if (Time.time - lastTypingTime > typingSpeed)
			{
				mainText.text += messageToDisplay[currentCharIndex];
				lastTypingTime = Time.time;
				currentCharIndex++;
			}
		}
	}

	private void UpdateMessageDisplay()
	{
		if (mainText.text.Equals("Go " + kreetures[1] + "!") && currentBattleState == BattleState.WaitingForInput)
		{
			SetMessageToDisplay("Select an action");
		}

		if (currentCharIndex == messageToDisplay.Length && currentBattleState == BattleState.EnteringBattle)
		{
			SetMessageToDisplay("Go " + kreetures[1].name + "!");
			SetBattleState(BattleState.WaitingForInput);
		}
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

			if (i < kreetures[1].knownAttacks.Count)
			{
				Attack attack = kreetures[1].knownAttacks[i];
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
}