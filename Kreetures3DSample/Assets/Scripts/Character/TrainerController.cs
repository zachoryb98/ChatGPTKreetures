using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    public string trainerID;
    public Transform trainerSpawnPosition;
    [SerializeField] string name;
    public float stoppingDistance = 2f;
    public float moveSpeed = 3f;
    private NavMeshAgent navMeshAgent;
    private bool isMoving = false;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    Animator animator;
    bool moveToPlayer = true;
    bool HasTrainerLost = false;

    public TrainerEncounterManager encounterManager;

    public string getSceneToLoad()
	{
        return encounterManager.sceneToLoad;
	}

    private void LookAtPlayer()
	{
        Transform player = GameManager.Instance.playerController.transform;
            // Calculate the direction from the NPC to the player
            Vector3 directionToPlayer = player.position - transform.position;

            // Rotate the NPC to look at the player, but only along the Y-axis (yaw)
            transform.rotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));        
    }

	private void Start()
    {
        PersistentObjectManager.CheckIfTrainerHasPassedThroughScene(this);
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();             
    }

    private void Update()
    {
        if (isMoving)
        {
            MoveToPlayer();
        }
    }

    public void BattleLost()
	{
        HasTrainerLost = true;
        encounterManager.GetComponent<Collider>().enabled = false;

        GameManager.Instance.SaveOnDemand();
    }

    public bool GetHasTrainerLost()
	{
        return HasTrainerLost;
	}

    public IEnumerator TriggerTrainerBattle()
    {
        GameManager.Instance.state = GameState.Dialog;
        // Show Exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        exclamation.SetActive(false);

        // Ensure the NPC is not already moving
        if (!isMoving)
        {
			if (moveToPlayer)
			{
                // Start moving towards the player
                isMoving = true;
                animator.SetBool("IsWalking", isMoving);
                navMeshAgent.speed = moveSpeed;
                navMeshAgent.SetDestination(GameManager.Instance.playerController.gameObject.transform.position);

                // Wait for the NPC to reach the player before continuing
                yield return new WaitUntil(() => !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= stoppingDistance);

                // Stop moving and trigger the dialogue or battle
                isMoving = false;
                animator.SetBool("IsWalking", isMoving);
            }

            //By default this is true, only set to false if player triggers interaction
            moveToPlayer = true;
            
            //GameManager.Instance.SetEnter
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
                          
            }));
        }        
    }

    private void MoveToPlayer()
    {
        if (isMoving && navMeshAgent.remainingDistance <= stoppingDistance)
        {
            // Stop both animation and movement when close to the player
            isMoving = false;
            navMeshAgent.speed = 0f;
            animator.SetBool("IsWalking", isMoving);
        }
    }

    public void Interact()
    {
        GameManager.Instance.state = GameState.Dialog;
		if (!HasTrainerLost)
		{
            moveToPlayer = false;
            GameManager.Instance.state = GameState.Wait;
            GameManager.Instance.playerController.DisablePlayerControls();
            GameManager.Instance.trainerController = this;
            GameManager.Instance.SetEnemyTeam(encounterManager.kreetures);
            GameManager.Instance.SetIsTrainerBattle(true);
            encounterManager.GetComponent<Collider>().enabled = false;
            StartCoroutine(TriggerTrainerBattle());
        }
		else
		{
            LookAtPlayer();
            //GameManager.Instance.SetEnter
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle, () =>
            {

            }));
        }
    }

	public object CaptureState()
	{
        return HasTrainerLost;
	}

	public void RestoreState(object state)
	{
        HasTrainerLost = (bool)state;

		if (HasTrainerLost)
		{
            encounterManager.GetComponent<Collider>().enabled = false;
        }
	}

	public string Name
    {
        get => name;
    }
    //public Sprite Sprite
    //{
    //    get => sprite;
    //}
}
