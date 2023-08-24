using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    public float encounterProbability = 0.2f;
    public List<Kreeture> possibleKreetures;
    [SerializeField] private string sceneToLoad = "TestScene";
    private bool hasReturnedFromBattle; // Flag to indicate if the player has returned from battle


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if the player has returned from battle
            if (!hasReturnedFromBattle)
            {
                hasReturnedFromBattle = true;
                return; // Prevent encounter logic
            }
            if (Random.value < encounterProbability)
            {
                Kreeture kreetureToEncounter = possibleKreetures[Random.Range(0, possibleKreetures.Count)];
                GameManager.Instance.kreetureForBattle = kreetureToEncounter;

                // Store player position and rotation
                Vector3 playerPosition = other.transform.position;
                Quaternion playerRotation = other.transform.rotation;
                GameManager.Instance.SetPlayerPosition(playerPosition, playerRotation);

                GameManager.Instance.SetPreviousScene(SceneManager.GetActiveScene().name);

                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    // Call this method when the player exits the battle scene
    public void SetReturnedFromBattleFlag(bool value)
    {
        hasReturnedFromBattle = value;
    }
}
