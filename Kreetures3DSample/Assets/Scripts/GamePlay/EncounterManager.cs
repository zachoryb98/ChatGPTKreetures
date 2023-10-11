using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    public float encounterProbability = 0.2f;
    public List<Kreeture> wildKreetures;    

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
                Kreeture wildKreeture = GetRandomWildKreeture();
                var wildKreetureCopy = new Kreeture(wildKreeture.Base, wildKreeture.Level);

                GameManager.Instance.SetWildKreeture(wildKreetureCopy);                                

                // Store player position and rotation
                Vector3 playerPosition = other.transform.position;
                Quaternion playerRotation = other.transform.rotation;

                GameManager.Instance.SetPlayerPosition(playerPosition, playerRotation);

                GameManager.Instance.TransitionToBattle(sceneToLoad);                                                
            }
        }
    }

    public Kreeture GetRandomWildKreeture()
    {
        var wildKreeture = wildKreetures[Random.Range(0, wildKreetures.Count)];
        wildKreeture.Init();
        return wildKreeture;
    }

    // Call this method when the player exits the battle scene
    public void SetReturnedFromBattleFlag(bool value)
    {
        hasReturnedFromBattle = value;
    }
}