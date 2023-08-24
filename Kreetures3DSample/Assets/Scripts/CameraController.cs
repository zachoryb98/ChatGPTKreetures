using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTarget; // Assign the camera target transform in the Inspector
    public Vector3 cameraOffset = new Vector3(0f, 10f, 5f); // Adjust the camera offset
    public float cameraAngle = 45f; // Adjust the camera angle

    public void SetCameraTarget(Transform transform)
	{
        cameraTarget = transform;
	}

    private void Update()
    {
        if (cameraTarget != null)
        {
            // Calculate the desired camera position based on the player's position and offset
            Vector3 desiredCameraPos = cameraTarget.position + cameraOffset;
            transform.position = desiredCameraPos;

            // Calculate the camera rotation
            Quaternion cameraRotation = Quaternion.Euler(cameraAngle, cameraTarget.eulerAngles.y, 0f);
            transform.rotation = cameraRotation;

            // Look at the player
            transform.LookAt(cameraTarget.position);
        }
    }
}