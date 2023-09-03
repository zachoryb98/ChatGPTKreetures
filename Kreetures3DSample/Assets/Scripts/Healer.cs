using UnityEngine;
using UnityEngine.InputSystem;

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
        foreach (Kreeture kreeture in GameManager.Instance.playerTeam)
        {
            // Set each Kreeture's currentHP to its baseHP or maximum health.
            kreeture.currentHP = kreeture.baseHP;
        }

        // You can also play a healing animation or sound effect here if desired.

        Debug.Log("Your party has been fully healed!");
    }
}
