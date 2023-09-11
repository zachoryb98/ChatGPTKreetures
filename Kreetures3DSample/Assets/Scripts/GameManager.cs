using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle }

public class GameManager : MonoBehaviour{    
    public static GameManager Instance { get; private set; }// Singleton instance        

    public PlayerData playerData;

    //Wild creature
    public KreetureBase kreetureForBattle { get; set; }

    [SerializeField] public PlayerController playerController;

    // Player-related data
    public List<KreetureBase> playerTeam = new List<KreetureBase>();
    private string previousSceneName;

    public bool playerDefeated = false;

    private Vector3 playerPosition = new Vector3();
    private Quaternion playerRotation = new Quaternion();    

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

    public List<KreetureBase> GetPlayerTeam()
	{
        return playerTeam;
	}

    public List<KreetureBase> GetKreetureNames()
    {
        // Replace this with your actual logic to fetch Kreeture names
        // For example, you might have a list of Kreeture objects and you can extract names from them        
        List <KreetureBase> kreetures = new List<KreetureBase>();

        //Wild Kreeture is always going to be in the list first
        kreetures.Add(kreetureForBattle);

        //Player creature always should be second in the list
        kreetures.Add(playerTeam[0]);

        return kreetures;
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