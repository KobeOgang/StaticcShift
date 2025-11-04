using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public DoorType doorType = DoorType.Sliding;
    public float openDistance = 3f;
    public Vector3 slideDirection = Vector3.up;  // For sliding doors
    public Vector3 rotationAxis = Vector3.up;   // For rotating doors  
    public float rotationAngle = 90f;
    public Vector3 pivotOffset = Vector3.zero;
    public float openSpeed = 2f;
    public bool startsOpen = false;

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("Visual Effects")]
    public GameObject openEffect;
    public GameObject closeEffect;

    [HideInInspector] public bool isOpen;
    private bool isMoving = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Quaternion closedRotation;  
    private Quaternion openRotation;     
    private Vector3 closedScale;        
    private Vector3 openScale;
    private Vector3 pivotPoint;
    private AudioSource audioSource;

    public enum DoorType
    {
        Sliding,
        Rotating,
        Scaling
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        closedPosition = transform.position;
        closedRotation = transform.rotation;  
        closedScale = transform.localScale;   

        CalculateOpenPosition();

        isOpen = startsOpen;
        if (startsOpen)
        {
            // Set initial state based on door type
            switch (doorType)
            {
                case DoorType.Sliding:
                    transform.position = openPosition;
                    break;
                case DoorType.Rotating:
                    transform.rotation = openRotation;
                    break;
                case DoorType.Scaling:
                    transform.localScale = openScale;
                    break;
            }
        }
    }

    private void CalculateOpenPosition()
    {
        switch (doorType)
        {
            case DoorType.Sliding:
                openPosition = closedPosition + (slideDirection.normalized * openDistance);
                break;
            case DoorType.Rotating:
                // Calculate the pivot point in world space using the CLOSED position
                pivotPoint = closedPosition + transform.TransformDirection(pivotOffset);

                // Store the closed rotation
                closedRotation = transform.rotation;
                openRotation = closedRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis);
                break;
            case DoorType.Scaling:
                closedScale = transform.localScale;
                openScale = Vector3.zero;
                break;
        }
    }

    public void OpenDoor()
    {
        if (isOpen || isMoving) return;

        StartCoroutine(MoveDoor(true));
    }

    public void CloseDoor()
    {
        if (!isOpen || isMoving) return;

        StartCoroutine(MoveDoor(false));
    }

    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    private System.Collections.IEnumerator MoveDoor(bool opening)
    {
        isMoving = true;

        // Play sound and effects
        PlaySound(opening ? openSound : closeSound);
        PlayEffect(opening ? openEffect : closeEffect);

        float journey = 0f;

        // Store starting values
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * openSpeed;
            float easeValue = EaseInOut(journey);

            switch (doorType)
            {
                case DoorType.Sliding:
                    Vector3 targetPos = opening ? openPosition : closedPosition;
                    transform.position = Vector3.Lerp(startPos, targetPos, easeValue);
                    break;

                case DoorType.Rotating:
                    Quaternion targetRot = opening ? openRotation : closedRotation;

                    // If we have a pivot offset, we need to rotate around the pivot point
                    if (pivotOffset != Vector3.zero)
                    {
                        // Calculate the original offset ONCE at the start (not every frame)
                        Vector3 originalOffset = startPos - pivotPoint;

                        // Interpolate rotation
                        Quaternion currentRot = Quaternion.Lerp(startRot, targetRot, easeValue);

                        // Calculate new position by rotating the original offset
                        Vector3 rotatedOffset = (currentRot * Quaternion.Inverse(startRot)) * originalOffset;

                        // Apply both rotation and position
                        transform.rotation = currentRot;
                        transform.position = pivotPoint + rotatedOffset;
                    }
                    else
                    {
                        // Standard rotation around center
                        transform.rotation = Quaternion.Lerp(startRot, targetRot, easeValue);
                    }
                    break;

                case DoorType.Scaling:
                    Vector3 targetScale = opening ? openScale : closedScale;
                    transform.localScale = Vector3.Lerp(startScale, targetScale, easeValue);
                    break;
            }

            yield return null;
        }

        isOpen = opening;
        isMoving = false;

        Debug.Log($"Door {(opening ? "opened" : "closed")} using {doorType} mechanism");
    }

    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayEffect(GameObject effect)
    {
        if (effect != null)
        {
            Instantiate(effect, transform.position, transform.rotation);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);

        switch (doorType)
        {
            case DoorType.Sliding:
                Gizmos.color = Color.yellow;
                Vector3 slideTarget = transform.position + (slideDirection.normalized * openDistance);
                Gizmos.DrawWireCube(slideTarget, transform.localScale);
                Gizmos.DrawLine(transform.position, slideTarget);

                // Show slide direction arrow
                Gizmos.color = Color.blue;
                Vector3 arrowEnd = transform.position + (slideDirection.normalized * (openDistance * 0.5f));
                Gizmos.DrawLine(transform.position, arrowEnd);
                break;

            case DoorType.Rotating:
                Gizmos.color = Color.cyan;
                // Draw rotation axis
                Gizmos.DrawLine(
                    transform.position - rotationAxis * 1f,
                    transform.position + rotationAxis * 1f
                );

                // Draw pivot point if offset is set
                if (pivotOffset != Vector3.zero)
                {
                    Gizmos.color = Color.red;
                    Vector3 worldPivot = transform.position + transform.TransformDirection(pivotOffset);
                    Gizmos.DrawSphere(worldPivot, 0.1f);

                    // Draw line from door center to pivot
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, worldPivot);
                }
                break;

            case DoorType.Scaling:
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
                break;
        }
    }
}
