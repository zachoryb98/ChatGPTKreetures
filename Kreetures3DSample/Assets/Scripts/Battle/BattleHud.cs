using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;

    Kreeture _kreeture;

    public void SetData(Kreeture kreeture)
    {
        _kreeture = kreeture;

        nameText.text = kreeture.Base.Name;
        levelText.text = "Lvl " + kreeture.Level;
        hpBar.SetHP((float)kreeture.HP / kreeture.MaxHp);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)_kreeture.HP / _kreeture.MaxHp);
    }
}