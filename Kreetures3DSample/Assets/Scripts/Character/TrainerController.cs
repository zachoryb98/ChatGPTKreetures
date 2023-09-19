using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TrainerController : MonoBehaviour
{
    [SerializeField] string name;
    public float stoppingDistance = 2f;
    public float moveSpeed = 3f;
    private NavMeshAgent navMeshAgent;
    private bool isMoving = false;
    [SerializeField] Dialog dialog;
    [SerializeField] GameObject exclamation;
    Animator animator;
    private string sceneToLoad;


    public string getSceneToLoad()
	{
        return sceneToLoad;
	}

    private void Start()
    {
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

    public IEnumerator TriggerTrainerBattle(string _sceneToLoad)
    {
        sceneToLoad = _sceneToLoad;
        // Show Exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        exclamation.SetActive(false);

        // Ensure the NPC is not already moving
        if (!isMoving)
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

    public string Name
    {
        get => name;
    }
    //public Sprite Sprite
    //{
    //    get => sprite;
    //}
}
