using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    public float encounterProbability = 0.2f; // Probability of encountering a Kreeture
    public List<Kreeture> possibleKreetures; // List of possible Kreetures to encounter

    [SerializeField] private string sceneToLoad = "TestScene"; // Add this serialized field

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Random.value < encounterProbability)
            {
                Kreeture kreetureToEncounter = possibleKreetures[Random.Range(0, possibleKreetures.Count)];

                // Set the kreetureForBattle variable in GameManager
                GameManager.Instance.kreetureForBattle = kreetureToEncounter;

                // Load the battle scene
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}