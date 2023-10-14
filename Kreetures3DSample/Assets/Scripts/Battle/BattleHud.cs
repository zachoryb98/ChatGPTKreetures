using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleHud : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI levelText;
	[SerializeField] TextMeshProUGUI statusText;
	[SerializeField] HPBar hpBar;
	[SerializeField] GameObject expBar;

	[SerializeField] Color psnColor;
	[SerializeField] Color brnColor;
	[SerializeField] Color slpColor;
	[SerializeField] Color parColor;
	[SerializeField] Color frzColor;

	Kreeture _kreeture;
	Dictionary<ConditionID, Color> statusColors;

	public void SetData(Kreeture kreeture)
	{
		_kreeture = kreeture;

		nameText.text = kreeture.Base.Name;
		levelText.text = "Lvl " + kreeture.Level;
		hpBar.SetHP((float)kreeture.HP / kreeture.MaxHp);
		SetExp();

		statusColors = new Dictionary<ConditionID, Color>()
		{
			{ConditionID.psn, psnColor },
			{ConditionID.brn, brnColor },
			{ConditionID.slp, slpColor },
			{ConditionID.par, parColor },
			{ConditionID.frz, frzColor },
		};

		SetStatusText();
		_kreeture.OnStatusChanged += SetStatusText;
	}

	void SetStatusText()
	{
		if (_kreeture.Status == null)
		{
			statusText.text = "";
		}
		else
		{
			statusText.text = _kreeture.Status.Id.ToString().ToUpper();
			statusText.color = statusColors[_kreeture.Status.Id];
		}
	}

	public void SetExp()
	{
		if (expBar == null) return;

		float normalizedExp = GetNormalizedExp();
		expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
	}

	public IEnumerator SetExpSmooth()
	{
		if (expBar == null) yield break;

		float normalizedExp = GetNormalizedExp();
		yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
	}

	float GetNormalizedExp()
	{
		int currLevelExp = _kreeture.Base.GetExpForLevel(_kreeture.Level);
		int nextLevelExp = _kreeture.Base.GetExpForLevel(_kreeture.Level + 1);

		float normalizedExp = (float)(_kreeture.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
		return Mathf.Clamp01(normalizedExp);
	}

	public IEnumerator UpdateHP()
	{
		if (_kreeture.HpChanged)
		{
			yield return hpBar.SetHPSmooth((float)_kreeture.HP / _kreeture.MaxHp);
			_kreeture.HpChanged = false;
		}
	}
}