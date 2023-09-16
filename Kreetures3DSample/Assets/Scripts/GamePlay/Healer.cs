using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class Healer : MonoBehaviour, Interactable
{
    private PlayerInput playerControls;

    private void HealParty()
    {
        foreach (Kreeture kreeture in GameManager.Instance.playerTeam.kreetures)
        {
            // Set each Kreeture's currentHP to its baseHP or maximum health.
            kreeture.HP = kreeture.MaxHp;
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

	public void Interact()
	{
        HealParty();
	}
}
