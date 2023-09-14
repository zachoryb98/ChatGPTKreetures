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

	public IEnumerator UpdateHP()
	{
		if (_kreeture.HpChanged)
		{
			yield return hpBar.SetHPSmooth((float)_kreeture.HP / _kreeture.MaxHp);
			_kreeture.HpChanged = false;
		}
	}
}