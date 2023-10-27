using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance { get; private set; }// Singleton instance  

    [SerializeField] GameObject menu;

    public event Action<int> onMenuSelected;
    public event Action onBack;

    List<TextMeshProUGUI> menuItems;

    int selectedItem = 0;

    private void Awake()
    {
        if(Instance == null)
		{
            menuItems = menu.GetComponentsInChildren<TextMeshProUGUI>().ToList();
            DontDestroyOnLoad(this);
        }
        else
		{
            Destroy(this);
		}
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);

        if (prevSelection != selectedItem)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
            CloseMenu();
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == selectedItem)
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            else
                menuItems[i].color = Color.white;
        }
    }
}
