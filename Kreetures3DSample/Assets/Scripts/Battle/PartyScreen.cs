using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;
    List<Kreeture> kreetures;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Kreeture> kreetures)
    {
        this.kreetures = kreetures;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < kreetures.Count)
			{
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(kreetures[i]);
            }                
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Choose a Kreeture";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < kreetures.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
