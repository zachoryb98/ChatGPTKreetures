using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private Animator animator;
    public GameObject camLookAt;    

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
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
    }

    public void SetPosition(Vector3 position)
    {
        // Update the player's position
        transform.position = position;
    }    
}