using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
	private PlayerInput UIControls;
	[SerializeField] GameObject dialogBox;
	[SerializeField] TextMeshProUGUI dialogText;
	[SerializeField] int lettersPerSecond;

	public event Action OnShowDialog;
	public event Action OnCloseDialog;

	public static DialogManager Instance { get; private set; }
	private void Awake()
	{
		Instance = this;
		UIControls = new PlayerInput();
	}

	Dialog dialog;
	Action onDialogFinished;

	int currentLine = 0;
	bool isTyping;

	public bool IsShowing { get; private set; }

	public IEnumerator ShowDialog(Dialog dialog, Action onFinished = null)
	{
		yield return new WaitForEndOfFrame();

		OnShowDialog?.Invoke();

		IsShowing = true;
		this.dialog = dialog;
		onDialogFinished = onFinished;

		dialogBox.SetActive(true);
		StartCoroutine(TypeDialog(dialog.Lines[0]));

	}


	private void Update()
	{
		if (GameManager.Instance.state == GameState.Dialog)
		{
			if (GameManager.Instance.playerController.GetContinueDialog() && !isTyping)
			{
				++currentLine;
				if (currentLine < dialog.Lines.Count)
				{
					StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
				}
				else
				{
					currentLine = 0;
					dialogBox.SetActive(false);
					UIControls.OverWorldUI.Disable();
					UIControls.PlayerControls.Enable();
					OnCloseDialog?.Invoke();
				}
			}
		}
	}

	public IEnumerator TypeDialog(string line)
	{
		isTyping = true;
		dialogText.text = "";
		foreach (var letter in line.ToCharArray())
		{
			dialogText.text += letter;
			yield return new WaitForSeconds(1f / lettersPerSecond);
		}
		isTyping = false;
		GameManager.Instance.playerController.SetContinueDialog(false);		
	}	
}