using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> moveTexts;
    [SerializeField] Color highlightedColor;
    [SerializeField] InputActionAsset inputActions;

    int currentSelection = 0;


    private void Awake()
    {
        // Enable the Input Actions
        inputActions.Enable();
    }

    public void SetMoveData(List<AttackBase> currentMoves, AttackBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; ++i)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {

        var moveUpAction = inputActions["NavigateUp"];
        var moveDownAction = inputActions["NavigateDown"];
        var confirmAction = inputActions["Confirm"];
        var backAction = inputActions["Back"];

        if (moveUpAction.triggered)
		{
            --currentSelection;
        }
        else if (moveDownAction.triggered)
		{            
            ++currentSelection;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, KreetureBase.MaxNumOfMoves);

        UpdateMoveSelection(currentSelection);

        if (confirmAction.triggered)
            onSelected?.Invoke(currentSelection);
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < KreetureBase.MaxNumOfMoves + 1; i++)
        {
            if (i == selection)
                moveTexts[i].color = highlightedColor;
            else
                moveTexts[i].color = Color.white;
        }
    }
}
