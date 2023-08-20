using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance
    
    public Kreeture kreetureForBattle { get; set; }

    // Player-related data
    public List<Kreeture> playerTeam = new List<Kreeture>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the GameManager object when changing scenes
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate GameManager instances
        }
    }
}