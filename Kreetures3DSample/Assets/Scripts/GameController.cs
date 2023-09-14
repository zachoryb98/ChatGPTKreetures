using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }// Singleton instance    

    [SerializeField] public PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    private void Awake()
    {
        ConditionsDB.Init();
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

    GameState state;

    private void Start()
    {
        //playerController.OnEncountered += StartBattle;
        //battleSystem.OnBattleOver += EndBattle;
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        //battleSystem.StartBattle();
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }
}
