using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;
    public PlayerController playerController;

    [Header("Stair Smoothing")]
    [SerializeField] private float stairSmoothTime = 0.12f;

    private float currentYVelocity;

    private void Update()
    {
        if (cameraPosition == null) return;

        if (playerController != null && playerController.IsClimbingStairs)
        {
            float smoothedY = Mathf.SmoothDamp(
                transform.position.y,
                cameraPosition.position.y,
                ref currentYVelocity,
                stairSmoothTime
            );

            transform.position = new Vector3(
                cameraPosition.position.x,
                smoothedY,
                cameraPosition.position.z
            );
        }
        else
        {
            currentYVelocity = 0f;
            transform.position = cameraPosition.position;
        }
    }
}
