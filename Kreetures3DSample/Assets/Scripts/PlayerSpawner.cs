using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab; // Assign the player prefab
    //[SerializeField] Camera cameraTarget; // Assign the camera target transform

    public void SpawnPlayer()
    {
        if (GameManager.Instance.playerData != null)
        {
            GameObject player = Instantiate(playerPrefab, GameManager.Instance.playerData.position, Quaternion.identity);

            // Set the player transform for the camera controller
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");

            Transform camLookAt = player.GetComponent<PlayerController>().camLookAt.transform;

            camera.GetComponent<CameraController>().SetCameraTarget(camLookAt);

            Debug.Log("Player connected to camera " + camera.gameObject.name);

            // You might need to set other data on the player GameObject if necessary
        }
    }

    public void SpawnPlayerAtPosition(Vector3 position, Quaternion rotation)
    {
        if (GameManager.Instance.playerData != null)
        {
            if (playerPrefab != null)
            {
                GameObject player = Instantiate(playerPrefab, position, rotation);

                // Check if the player has the required component
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
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
                Debug.LogWarning("Player prefab is not assigned.");
            }
        }
    }
}