using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;

    public Color HighlightedColor => highlightedColor;

    public static GlobalSettings i { get; private set; }
	private void Awake()
	{
		if (i == null)
		{
			i = this;
			DontDestroyOnLoad(gameObject); // Keep the GameManager object when changing scenes
		}
		else
		{
			Destroy(gameObject); // Destroy any duplicate GameManager instances
		}
	}
}
