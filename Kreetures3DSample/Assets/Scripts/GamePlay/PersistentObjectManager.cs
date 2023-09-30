using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObjectManager : MonoBehaviour
{
	public static PersistentObjectManager Instance;

	// List to store registered persistent objects
	public List<GameObject> persistentObjects = new List<GameObject>();

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		// Register the object to persist
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Re-register all registered objects in the new scene
		foreach (var obj in persistentObjects)
		{
			DontDestroyOnLoad(obj);
		}
	}

	public void RegisterObject(GameObject obj)
	{
		if (!persistentObjects.Contains(obj))
		{
			persistentObjects.Add(obj);
			DontDestroyOnLoad(obj);
		}
	}

	public static void UnregisterObject(GameObject obj)
	{
		Instance.persistentObjects.Remove(obj);
	}

	public static bool IsObjectRegistered(GameObject obj)
	{
		return Instance.persistentObjects.Contains(obj);
	}

	public static void CheckIfTrainerHasPassedThroughScene(TrainerController trainer)
	{		
		foreach(var obj in Instance.persistentObjects)
		{
			TrainerController _trainer = obj.GetComponent<TrainerController>();
			if(_trainer != null)
			{
				if(trainer.trainerID == _trainer.trainerID)
				{
					Destroy(trainer.gameObject);
					UnregisterObject(trainer.gameObject);
				}
			}
		}		
	}
}