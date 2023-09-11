using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
	public GameObject playerPrefab; // Assign the player prefab
									//[SerializeField] Camera cameraTarget; // Assign the camera target transform

	public void SpawnPlayerAtPosition(Vector3 position, Quaternion rotation)
	{
		if (playerPrefab != null)
		{
			if (GameManager.Instance.playerDefeated)
			{
				GameObject player;

				if (GameManager.Instance.playerController == null)
				{
					player = Instantiate(playerPrefab, GameManager.Instance.GetPlayerLastHealPosition(), new Quaternion(0, 0, 0, 0));
					PlayerController playerController = player.GetComponent<PlayerController>();

					// Check if the player has the required component
					if (playerController != null)
					{
						GameManager.Instance.SetPlayer(playerController);

						Transform camLookAt = playerController.camLookAt.transform;

						GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
						CameraController cameraController = camera.GetComponent<CameraController>();

						if (cameraController != null)
						{
							cameraController.SetCameraTarget(camLookAt);
						}

						GameManager.Instance.playerDefeated = false;
						Debug.Log("Player spawned at " + position + " and connected to camera " + camera.gameObject.name);
					}
					else
					{
						Debug.LogWarning("Player prefab is missing PlayerController component.");
					}
				}
				else
				{
					GameManager.Instance.playerController.gameObject.SetActive(true);
				}
			}
			else
			{
				GameObject player;
				if (GameManager.Instance.playerController == null)
				{
					player = Instantiate(playerPrefab, position, rotation);

					// Check if the player has the required component
					PlayerController playerController = player.GetComponent<PlayerController>();
					if (playerController != null)
					{
						GameManager.Instance.SetPlayer(playerController);

						KreetureParty party = player.GetComponent<KreetureParty>();
						GameManager.Instance.SetPlayerTeam(party);

						Transform camLookAt = playerController.camLookAt.transform;

						GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
						CameraController cameraController = camera.GetComponent<CameraController>();

						if (cameraController != null)
						{
							cameraController.SetCameraTarget(camLookAt);
						}

						Debug.Log("Player spawned at " + position + " and connected to camera " + camera.gameObject.name);
					}
					else
					{
						Debug.LogWarning("Player prefab is missing PlayerController component.");
					}
				}
				else
				{
					var playerController = GameManager.Instance.playerController;
						
					playerController.gameObject.SetActive(true);
					
					Transform camLookAt = playerController.camLookAt.transform;

					GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
					CameraController cameraController = camera.GetComponent<CameraController>();

					if (cameraController != null)
					{
						cameraController.SetCameraTarget(camLookAt);
					}
				}				
			}
		}
		else
		{
			Debug.LogWarning("Player prefab is not assigned.");
		}
	}

	//Easier to just heal team and carry on after loss.
	private void HealKreetures()
	{
		//foreach (Kreeture kreeture in GameManager.Instance.playerTeam)
		//{
		//    // Set each Kreeture's currentHP to its baseHP or maximum health.
		//    //kreeture.currentHP = kreeture.MaxHp;
		//}

		// You can also play a healing animation or sound effect here if desired.

		Debug.Log("Your party has been fully healed!");
	}
}