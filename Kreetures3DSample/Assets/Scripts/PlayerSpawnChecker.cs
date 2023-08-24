using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnChecker : MonoBehaviour
{
    public PlayerSpawner playerSpawner;

    Vector3 SpawnPosition = new Vector3();
    Quaternion SpawnRotation = new Quaternion();


    private void Start()
	{
        SpawnPosition = GameManager.Instance.GetPlayerPosition();
        SpawnRotation = GameManager.Instance.GetPlayerRotation();
    }

	// Update is called once per frame
	void Update()
    {
        GameObject player = null;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
		{
            playerSpawner.SpawnPlayerAtPosition(SpawnPosition, SpawnRotation);
            Destroy(this.gameObject);
		}
    }
}
