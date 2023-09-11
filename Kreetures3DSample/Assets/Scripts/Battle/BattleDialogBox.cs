using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
	[SerializeField] int lettersPerSecond;
	[SerializeField] Color highlightedColor;

	[SerializeField] TextMeshProUGUI dialogText;
	[SerializeField] GameObject actionSelector;
	[SerializeField] GameObject moveSelector;
	[SerializeField] GameObject moveDetails;

	[SerializeField] List<TextMeshProUGUI> actionTexts;
	[SerializeField] List<TextMeshProUGUI> attackTexts;

	[SerializeField] TextMeshProUGUI ppText;
	[SerializeField] TextMeshProUGUI typeText;

	public void SetDialog(string dialog)
	{
		dialogText.text = dialog;
	}

	public IEnumerator TypeDialog(string dialog)
	{
		dialogText.text = "";
		foreach (var letter in dialog.ToCharArray())
		{
			dialogText.text += letter;
			yield return new WaitForSeconds(1f / lettersPerSecond);
		}

		yield return new WaitForSeconds(1f);
	}

	public void EnableDialogText(bool enabled)
	{
		dialogText.enabled = enabled;
	}

	public void EnableActionSelector(bool enabled)
	{
		actionSelector.SetActive(enabled);
	}

	public void EnableMoveSelector(bool enabled)
	{
		moveSelector.SetActive(enabled);
		moveDetails.SetActive(enabled);
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
}
