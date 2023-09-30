using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainerEncounterManager : MonoBehaviour
{
	public float encounterProbability = 0.2f;
	public KreetureParty kreetures;
	public TrainerController trainer;

	[SerializeField] public string sceneToLoad = "TestScene";

	private bool hasReturnedFromBattle; // Flag to indicate if the player has returned from battle


	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			GameManager.Instance.state = GameState.Wait;
			GameManager.Instance.playerController.DisablePlayerControls();
			GameManager.Instance.trainerController = trainer;
			GameManager.Instance.SetEnemyTeam(kreetures);
			GameManager.Instance.SetIsTrainerBattle(true);
			StartCoroutine(trainer.TriggerTrainerBattle());

			// Store player position and rotation
			//Vector3 playerPosition = other.transform.position;
			//Quaternion playerRotation = other.transform.rotation;
			//GameManager.Instance.SetPlayerPosition(playerPosition, playerRotation);

			//GameManager.Instance.playerController.gameObject.SetActive(false);

			//GameManager.Instance.OpenBattleScene(sceneToLoad);
		}
	}
}