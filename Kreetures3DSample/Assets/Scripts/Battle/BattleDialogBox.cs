using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleDialogBox : MonoBehaviour
{
	Transform startPos;
	[SerializeField] int lettersPerSecond;
	[SerializeField] Color highlightedColor;

	[SerializeField] TextMeshProUGUI dialogText;
	[SerializeField] GameObject actionSelector;
	[SerializeField] GameObject moveSelector;
	[SerializeField] GameObject moveDetails;
	[SerializeField] GameObject choiceBox;

	[SerializeField] List<TextMeshProUGUI> actionTexts;
	[SerializeField] List<TextMeshProUGUI> attackTexts;

	[SerializeField] TextMeshProUGUI ppText;
	[SerializeField] TextMeshProUGUI typeText;

	[SerializeField] TextMeshProUGUI yesText;
	[SerializeField] TextMeshProUGUI noText;

	private void Awake()
	{
		startPos = this.transform;
		GetComponent<RectTransform>().DOAnchorPos(new Vector2(-130, 0), .25f);
	}

	public void SetDialog(string dialog)
	{
		dialogText.text = dialog;
	}

	public IEnumerator TypeDialog(string dialog)
	{
		StartCoroutine(ShowDialog());
		dialogText.text = "";
		foreach (var letter in dialog.ToCharArray())
		{
			dialogText.text += letter;
			yield return new WaitForSeconds(1f / lettersPerSecond);
		}

		yield return new WaitForSeconds(1f);
		StartCoroutine(hideDialog());
	}

	public IEnumerator hideDialog()
	{
		yield return GetComponent<RectTransform>().DOAnchorPos(new Vector2(-150, 400), .25f);
	}
	private IEnumerator ShowDialog()
	{
		yield return this.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-150, 300), .25f);
	}

	public void EnableDialogText(bool enabled)
	{
		dialogText.enabled = enabled;
	}

	public void EnableActionSelector(bool enabled)
	{
		if (!enabled)
		{
			StartCoroutine(HideActionButtons());
		}
		else
		{
			StartCoroutine(ShowActionButtons()); 
		}
	}

	public void EnableChoiceBox(bool enabled)
	{
		choiceBox.SetActive(enabled);
	}

	public void EnableMoveSelector(bool enabled)
	{
		if (!enabled)
		{
			StartCoroutine(HideAttackButtons());
		}
		else
		{
			StartCoroutine(ShowAttackButtons());
		}
	}

	public void UpdateActionSelection(int selectedAction)
	{
		for (int i = 0; i < actionTexts.Count; ++i)
		{
			if (i == selectedAction)
				actionTexts[i].color = highlightedColor;
			else
				actionTexts[i].color = Color.white;
		}
	}

	public void UpdateChoiceBox(bool yesSelected)
	{
		if (yesSelected)
		{
			yesText.color = highlightedColor;
			noText.color = Color.white;
		}
		else
		{			
			noText.color = highlightedColor;
			yesText.color = Color.white;
		}
	}

	public void UpdateMoveSelection(int selectedMove, Attack move)
	{
		for (int i = 0; i < attackTexts.Count; ++i)
		{
			if (i == selectedMove)
				attackTexts[i].color = highlightedColor;
			else
				attackTexts[i].color = Color.white;
		}

		ppText.text = $"PP {move.PP}/{move.Base.PP}";
		typeText.text = move.Base.Type.ToString();

		if (move.PP == 0)
			ppText.color = Color.red;
		else
			ppText.color = Color.white;
	}

	public void SetMoveNames(List<Attack> attack)
	{
		for (int i = 0; i < attackTexts.Count; ++i)
		{
			if (i < attack.Count)
				attackTexts[i].text = attack[i].Base.Name;
			else
				attackTexts[i].text = "-";
		}
	}

	public IEnumerator ShowActionButtons()
	{
		yield return actionSelector.GetComponent<RectTransform>().DOAnchorPos(new Vector2(265, 5), .25f);
		actionSelector.SetActive(true);
	}

	public IEnumerator HideActionButtons()
	{
		yield return actionSelector.GetComponent<RectTransform>().DOAnchorPos(new Vector2(265, -200), .25f);

		actionSelector.SetActive(false);
	}

	public IEnumerator ShowAttackButtons()
	{
		yield return moveSelector.GetComponent<RectTransform>().DOAnchorPos(new Vector2(265, 5), .25f);
		moveSelector.SetActive(true);
		moveDetails.SetActive(true);
	}

	public IEnumerator HideAttackButtons()
	{
		yield return moveSelector.GetComponent<RectTransform>().DOAnchorPos(new Vector2(350, 5), .25f);

		moveSelector.SetActive(false);
		moveDetails.SetActive(false);
	}
}
