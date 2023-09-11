using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class Healer : MonoBehaviour
{
    private PlayerInput playerControls;
    private bool playerInRange = false;

    private void Awake()
    {
        playerControls = new PlayerInput();        
    }

    private void OnEnable()
    {
        playerControls.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.PlayerControls.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        // Check for player input (the "Heal" action) and if the player is in range to heal.
        if (playerInRange && playerControls.PlayerControls.Heal.triggered)
        {
            HealParty();
        }
    }

    private void HealParty()
    {
        foreach (KreetureBase kreeture in GameManager.Instance.playerTeam)
        {
            // Set each Kreeture's currentHP to its baseHP or maximum health.
            //kreeture. = kreeture.MaxHp;
        }

        // You can also play a healing animation or sound effect here if desired.

        Debug.Log("Your party has been fully healed!");
        UpdateRespawnPoint();
    }

    public void UpdateRespawnPoint()
	{
        Scene scene = SceneManager.GetActiveScene();
        GameManager.Instance.SetLastHealScene(scene.name);

        Vector3 spawnlocation = this.gameObject.transform.position;

        spawnlocation.z = spawnlocation.z - 1;

        GameManager.Instance.SetPlayerLastHealLocation(spawnlocation);

    }
}
