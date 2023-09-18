using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    public float moveSpeed;

    public bool IsMoving { get; private set; }

    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Move(Vector3 targetPosition, Action OnMoveOver = null)
    {
        navMeshAgent.SetDestination(targetPosition);
        IsMoving = true;
    }


    public void LookTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f; // Keep the character upright in a 3D world
        transform.forward = direction.normalized;
    }
}
