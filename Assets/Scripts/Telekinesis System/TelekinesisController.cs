using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinesisController : MonoBehaviour
{
    [Header("Telekinesis Settings")]
    public float maxGrabDistance = 5f;
    public float holdDistance = 3f;
    public float minHoldDistance = 1f;
    public float maxHoldDistance = 8f;
    public float distanceScrollSpeed = 2f;
    public float moveForce = 50f;
    public float maxVelocity = 10f;
    public float dampingFactor = 5f;
    public LayerMask interactableLayers = -1;

    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // degrees per second
    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;

    [Header("Input")]
    public KeyCode grabKey = KeyCode.Mouse0;
    public KeyCode anchorKey = KeyCode.Mouse1;

    [Header("Heavy Object Settings")]
    public float heavyObjectForceMultiplier = 1.5f;
    public float movementSlowdown = 0.5f;

    private Camera playerCamera;
    private AnchorSystem anchorSystem;
    private PlayerController playerController;
    private InteractableObject currentObject;
    private InteractableObject highlightedObject;
    private bool isManipulating = false;

    // Rotation tracking
    private Vector3 currentRotationInput;
    private float currentHoldDistance;

    public bool IsManipulatingHeavyObject => isManipulating && currentObject != null && currentObject.objectType == ObjectType.Heavy;

    private void Awake()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        anchorSystem = GetComponent<AnchorSystem>();
        if (anchorSystem == null)
            anchorSystem = gameObject.AddComponent<AnchorSystem>();

        playerController = GetComponent<PlayerController>();

        // Initialize hold distance
        currentHoldDistance = holdDistance;
    }

    private void Update()
    {
        HandleHighlighting();
        HandleInput();
        HandleDistanceControl();
        HandleRotationInput();
    }

    private void FixedUpdate()
    {
        HandleManipulation();
        HandleRotation();
    }

    private void HandleHighlighting()
    {
        InteractableObject newHighlight = GetInteractableObjectAtCrosshair();

        if (newHighlight != highlightedObject)
        {
            // Remove old highlight
            if (highlightedObject != null && highlightedObject != currentObject)
                highlightedObject.SetHighlight(false);

            // Apply new highlight
            highlightedObject = newHighlight;
            if (highlightedObject != null && highlightedObject != currentObject)
                highlightedObject.SetHighlight(true);
        }
    }

    private void HandleInput()
    {
        // Grab/Release object
        if (Input.GetKeyDown(grabKey))
        {
            if (!isManipulating)
                TryGrabObject();
            else
                ReleaseObject();
        }

        // Anchor system
        if (Input.GetKeyDown(anchorKey))
        {
            bool anchored = anchorSystem.TryAnchorObject();

            // If we successfully anchored something AND we're currently holding it, release it
            if (anchored && isManipulating && currentObject != null)
            {
                InteractableObject anchoredObject = anchorSystem.CurrentAnchor;
                if (anchoredObject == currentObject)
                {
                    Debug.Log($"Anchoring held object: {currentObject.name}. Releasing telekinesis.");
                    ReleaseObject();
                }
            }
        }
    }

    private void HandleDistanceControl()
    {
        // Only allow distance control when manipulating an object
        if (!isManipulating) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            currentHoldDistance += scrollInput * distanceScrollSpeed;
            currentHoldDistance = Mathf.Clamp(currentHoldDistance, minHoldDistance, maxHoldDistance);

            Debug.Log($"Hold distance: {currentHoldDistance:F1}m");
        }
    }

    private void HandleRotationInput()
    {
        // Only allow rotation when manipulating an object
        if (!isManipulating) return;

        currentRotationInput = Vector3.zero;

        // Horizontal rotation (Y-axis)
        if (Input.GetKey(rotateLeftKey))
        {
            currentRotationInput.y = -1f;
        }
        else if (Input.GetKey(rotateRightKey))
        {
            currentRotationInput.y = 1f;
        }

        // Optional: Add vertical rotation with other keys (you can extend this)
        // if (Input.GetKey(KeyCode.R))
        //     currentRotationInput.x = 1f;
        // if (Input.GetKey(KeyCode.F))
        //     currentRotationInput.x = -1f;
    }

    private void HandleManipulation()
    {
        if (!isManipulating || currentObject == null || currentObject.Rigidbody == null) return;

        Vector3 targetPosition = playerCamera.transform.position +
                               playerCamera.transform.forward * currentHoldDistance;

        Vector3 currentPosition = currentObject.ManipulationCenter;
        Vector3 direction = (targetPosition - currentPosition);
        float distance = direction.magnitude;

        if (distance > 0.1f)
        {
            Rigidbody rb = currentObject.Rigidbody;
            float forceMultiplier = moveForce * rb.mass;

            if (currentObject.objectType == ObjectType.Heavy)
            {
                forceMultiplier *= heavyObjectForceMultiplier;
            }

            Vector3 velocity = direction.normalized * Mathf.Min(distance * dampingFactor, maxVelocity);
            rb.velocity = Vector3.Lerp(rb.velocity, velocity, Time.fixedDeltaTime * dampingFactor);

            if (currentRotationInput.magnitude < 0.1f)
            {
                rb.angularVelocity *= 0.8f;
            }
        }
        else
        {
            currentObject.Rigidbody.velocity *= 0.8f;

            if (currentRotationInput.magnitude < 0.1f)
            {
                currentObject.Rigidbody.angularVelocity *= 0.8f;
            }
        }
    }

    private void HandleRotation()
    {
        if (!isManipulating || currentObject == null || currentObject.Rigidbody == null) return;

        if (currentRotationInput.magnitude > 0.1f)
        {
            // Apply rotation based on camera orientation for intuitive controls
            Vector3 rotationAxis = Vector3.zero;

            // Y-axis rotation (left/right) - always relative to world up
            if (Mathf.Abs(currentRotationInput.y) > 0.1f)
            {
                rotationAxis += Vector3.up * currentRotationInput.y;
            }

            // X-axis rotation (up/down) - relative to camera right
            if (Mathf.Abs(currentRotationInput.x) > 0.1f)
            {
                rotationAxis += playerCamera.transform.right * currentRotationInput.x;
            }

            // Apply angular velocity for smooth rotation
            Vector3 targetAngularVelocity = rotationAxis * rotationSpeed * Mathf.Deg2Rad;
            currentObject.Rigidbody.angularVelocity = Vector3.Lerp(
                currentObject.Rigidbody.angularVelocity,
                targetAngularVelocity,
                Time.fixedDeltaTime * 5f
            );
        }
    }

    private void TryGrabObject()
    {
        InteractableObject targetObject = GetInteractableObjectAtCrosshair();

        if (targetObject != null && targetObject.CanBeManipulated(anchorSystem.HasActiveAnchor))
        {
            currentObject = targetObject;
            isManipulating = true;

            if (currentObject.objectType == ObjectType.Heavy && playerController != null)
            {
                playerController.SetMovementRestriction(true, movementSlowdown);
            }

            float grabDistance = Vector3.Distance(playerCamera.transform.position, currentObject.ManipulationCenter);
            currentHoldDistance = Mathf.Clamp(grabDistance, minHoldDistance, maxHoldDistance);

            currentObject.SetManipulated(true);
            currentObject.SetHighlight(true);

            if (currentObject.Rigidbody != null)
            {
                currentObject.Rigidbody.drag = 2f;
                currentObject.Rigidbody.angularDrag = 2f;
            }

            Debug.Log($"Grabbed: {currentObject.name} at {currentHoldDistance:F1}m (Type: {currentObject.objectType})");
        }
    }

    private void ReleaseObject()
    {
        if (currentObject != null)
        {
            currentObject.SetManipulated(false);
            currentObject.SetHighlight(false);

            // Restore normal physics properties
            if (currentObject.Rigidbody != null)
            {
                currentObject.Rigidbody.drag = 1f;
                currentObject.Rigidbody.angularDrag = 1f;
            }

            Debug.Log($"Released: {currentObject.name}");
            currentObject = null;
        }

        isManipulating = false;
        if (playerController != null)
        {
            playerController.SetMovementRestriction(false, 1f);
        }
        currentRotationInput = Vector3.zero;
    }

    private InteractableObject GetInteractableObjectAtCrosshair()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, interactableLayers))
        {
            return hit.collider.GetComponent<InteractableObject>();
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Vector3 forward = playerCamera.transform.forward;
            Gizmos.DrawRay(playerCamera.transform.position, forward * maxGrabDistance);

            // Show current hold distance
            Gizmos.color = Color.red;
            Vector3 holdPos = playerCamera.transform.position + forward * currentHoldDistance;
            Gizmos.DrawWireSphere(holdPos, 0.2f);

            // Show min/max hold distance range
            Gizmos.color = Color.yellow;
            Vector3 minPos = playerCamera.transform.position + forward * minHoldDistance;
            Vector3 maxPos = playerCamera.transform.position + forward * maxHoldDistance;
            Gizmos.DrawWireSphere(minPos, 0.1f);
            Gizmos.DrawWireSphere(maxPos, 0.1f);
            Gizmos.DrawLine(minPos, maxPos);
        }
    }

    private void OnGUI()
    {
        // Debug UI to show current controls when manipulating
        if (isManipulating && currentObject != null)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label($"Manipulating: {currentObject.name}");
            GUILayout.Label($"Hold Distance: {currentHoldDistance:F1}m");
            GUILayout.Label("Scroll: Adjust distance");
            GUILayout.Label("Q/E: Rotate left/right");
            GUILayout.EndArea();
        }
    }
}
