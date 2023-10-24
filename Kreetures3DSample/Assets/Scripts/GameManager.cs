using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Dialog, Wait }

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }// Singleton instance            

	//Wild creature
	private Kreeture wildKreeture { get; set; }

	[SerializeField] public PlayerController playerController;
	[SerializeField] public TrainerController trainerController;

	// Player-related data
	public KreetureParty playerTeam = new KreetureParty();
	public KreetureParty enemyTrainerParty = new KreetureParty();
	private bool enterTrainerBattle = false;

	private string previousSceneName;

	public bool playerDefeated = false;

	private Vector3 playerPosition = new Vector3();
	private Quaternion playerRotation = new Quaternion();

	public GameState state;

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

	private void Update()
	{
		if(state == GameState.FreeRoam)
		{

			if (Input.GetKeyDown(KeyCode.S))
			{
				SavingSystem.i.Save("saveSlot1");
			}

			if (Input.GetKeyDown(KeyCode.L))
			{
				SavingSystem.i.Load("saveSlot1");
			}
		}
	}

	public void SetTrainerLoss()
	{
		trainerController.BattleLost();
		SetIsTrainerBattle(false);
		playerController.SetContinueDialog(false);
	}

	public Kreeture GetWildKreeture()
	{
		return wildKreeture;
	}

	private void Start()
	{
		DialogManager.Instance.OnShowDialog += () =>
		{
			state = GameState.Dialog;
			playerController.DisablePlayerControls();
			playerController.EnableUIControls();
		};

		DialogManager.Instance.OnCloseDialog += () =>
		{
			if (state == GameState.Dialog)
				state = GameState.FreeRoam;
			playerController.EnablePlayerControls();
			playerController.DisableUIControls();
			if (enterTrainerBattle)
			{
				if (trainerController != null)
				{
					PersistentObjectManager.Instance.RegisterObject(trainerController.gameObject);
					TransitionToTrainerBattle(trainerController.getSceneToLoad());
				}
			}
		};
	}

	public void TransitionToBattle(string sceneToLoad)
	{
		playerController.gameObject.SetActive(false);
		enterTrainerBattle = false;
		OpenBattleScene(sceneToLoad);
	}

	public void TransitionToTrainerBattle(string _sceneToLoad)
	{
		playerController.gameObject.SetActive(false);
		OpenBattleScene(_sceneToLoad);
	}

	public TrainerController GetTrainer()
	{
		return trainerController;
	}

	public bool GetIsTrainerBattle()
	{
		return enterTrainerBattle;
	}

	public void SetIsTrainerBattle(bool result)
	{
		enterTrainerBattle = result;
	}

	internal void SetWildKreeture(Kreeture _wildKreeture)
	{
		wildKreeture = _wildKreeture;
	}

	public KreetureParty GetPlayerTeam()
	{
		return playerTeam;
	}

	public KreetureParty GetEnemyTeam()
	{
		return enemyTrainerParty;
	}

	public void SetEnemyTeam(KreetureParty party)
	{
		enemyTrainerParty = party;
	}

	public void SetPlayerTeam(KreetureParty party)
	{
		playerTeam = party;
	}

	public void SetPlayer(PlayerController _playerController)
	{
		playerController = _playerController;
	}

	public void SetPreviousScene(string sceneName)
	{
		previousSceneName = sceneName;
	}

	public string GetPreviousScene()
	{
		return previousSceneName;
	}

	public void SetPlayerPosition(Vector3 position, Quaternion rotation)
	{
		playerPosition = position;
		playerRotation = rotation;
	}

	public Vector3 GetPlayerPosition()
	{
		return playerPosition;
	}

	public Quaternion GetPlayerRotation()
	{
		return playerRotation;
	}

	public string GetLastHealScene()
	{
		return PlayerPrefs.GetString("playerSpawnScene");
	}

	public void SetLastHealScene(string healScene)
	{
		PlayerPrefs.SetString("playerSpawnScene", healScene);
	}

	public Vector3 GetPlayerLastHealPosition()
	{
		Vector3 lastHealLocation = new Vector3();
		lastHealLocation.x = PlayerPrefs.GetFloat("playerHealPositionX");
		lastHealLocation.y = PlayerPrefs.GetFloat("playerHealPositionY");
		lastHealLocation.z = PlayerPrefs.GetFloat("playerHealPositionZ");

		return lastHealLocation;
	}

	public void SetPlayerLastHealLocation(Vector3 currentPosition)
	{
		PlayerPrefs.SetFloat("playerHealPositionX", currentPosition.x);
		PlayerPrefs.SetFloat("playerHealPositionY", currentPosition.y);
		PlayerPrefs.SetFloat("playerHealPositionZ", currentPosition.z);
	}

	public GameObject GetPlayerController()
	{
		GameObject playerController = GameObject.Find("Player");

		return playerController;
	}

	public void OpenBattleScene(string sceneToLoad)
	{
		Instance.SetPreviousScene(SceneManager.GetActiveScene().name);

		SceneManager.LoadScene(sceneToLoad);
	}
}

[System.Serializable]
public class PlayerData
{
	public Vector3 position;
	// Add more data you want to carry over
}