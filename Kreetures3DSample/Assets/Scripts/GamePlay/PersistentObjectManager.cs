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
		// Create a copy of the persistentObjects list
		List<GameObject> objectsCopy = new List<GameObject>(Instance.persistentObjects);

		foreach (var obj in objectsCopy)
		{
			TrainerController _trainer = obj.GetComponent<TrainerController>();
			if (_trainer != null && trainer.trainerID == _trainer.trainerID)
			{
				trainer.gameObject.transform.position = obj.transform.position;
				trainer.gameObject.transform.rotation = obj.transform.rotation;

				// Modify the original collection
				SavingSystem.i.LogGameState();
				Instance.persistentObjects.Remove(obj);
				Destroy(obj);

				trainer.BattleLost();
			}
		}
		GameManager.Instance.RestoreSavableEntities();
	}
}