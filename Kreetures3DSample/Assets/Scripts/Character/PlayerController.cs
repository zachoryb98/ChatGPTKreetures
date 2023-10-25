using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
	private PlayerInput playerControls;
	public float moveSpeed = 5.0f;
	private Animator animator;
	public GameObject camLookAt;
	public LayerMask interactableLayer;
	public LayerMask fovLayer;
	public float interactDistance = 2.0f; // Set your desired default distance here
	public event Action<Collider> OnEnterTrainersView;
	public bool ContinueDialog = false;

	public KreetureParty playerParty;

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

	private void Start()
	{
		DontDestroyOnLoad(this.gameObject);
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (GameManager.Instance.state == GameState.FreeRoam)
		{
			EnablePlayerControls();

			float horizontalInput = Input.GetAxis("Horizontal");
			float verticalInput = Input.GetAxis("Vertical");

			Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput);
			if (movement != Vector3.zero)
			{
				Quaternion newRotation = Quaternion.LookRotation(movement);
				transform.rotation = newRotation;

				// Set IsMoving parameter for the Animator
				animator.SetBool("IsMoving", true);
			}
			else
			{
				// Set IsMoving parameter for the Animator
				animator.SetBool("IsMoving", false);
			}

			// Move the character based on input
			transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

			if (playerControls.PlayerControls.Interaction.triggered)
			{
				Interact();
			}
		}
		else if (GameManager.Instance.state == GameState.Dialog)
		{
			EnableUIControls();
			animator.SetBool("IsMoving", false);
			if (playerControls.OverWorldUI.Continue.triggered)
			{
				SetContinueDialog(true);
				DisableUIControls();
			}
		}
		else
		{
			animator.SetBool("IsMoving", false);
		}
	}

	void Interact()
	{
		var facingDir = transform.forward;
		var chestOffset = new Vector3(0f, 0.5f, 0f); // Adjust the Y value as needed
		var interactPos = transform.position + facingDir * interactDistance + chestOffset;

		Debug.DrawLine(transform.position + chestOffset, interactPos, Color.green, 0.5f);

		RaycastHit hit;

		if (Physics.Raycast(transform.position + chestOffset, facingDir, out hit, interactDistance))
		{
			Interactable interactable = hit.collider.GetComponent<Interactable>();
			if (interactable != null)
			{
				interactable.Interact();
			}
		}
	}

	public bool GetContinueDialog()
	{
		return ContinueDialog;
	}

	public void SetContinueDialog(bool value)
	{
		ContinueDialog = value;
	}

	public void DisableUIControls()
	{
		playerControls.OverWorldUI.Disable();
	}

	public void EnableUIControls()
	{
		playerControls.OverWorldUI.Enable();
	}

	public void DisablePlayerControls()
	{
		playerControls.PlayerControls.Disable();
	}

	public void EnablePlayerControls()
	{
		playerControls.PlayerControls.Enable();
	}

	public void SetPosition(Vector3 position)
	{
		// Update the player's position
		transform.position = position;
	}

	public object CaptureState()
	{
		var saveData = new PlayerSaveData()
		{
			position = new float[] { transform.position.x, transform.position.y, transform.position.z },
			kreetures = GetComponent<KreetureParty>().Kreetures.Select(k => k.GetSaveData()).ToList()
		};

		return saveData;
	}

	public void RestoreState(object state)
	{
		var saveData = (PlayerSaveData)state;

		//Restore position
		var position = saveData.position;
		transform.position = new Vector3(position[0], position[1], position[2]);

		//Restore Party
		GetComponent<KreetureParty>().Kreetures = saveData.kreetures.Select(s => new Kreeture(s)).ToList();
	}
}

[Serializable]
public class PlayerSaveData
{
	public float[] position;
	public List<KreetureSaveData> kreetures;
}