using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [Header("Pressure Plate Settings")]
    public float requiredWeight = 5f;
    public float activationDepth = 0.1f;
    public LayerMask detectionLayers = -1;

    [Header("Visual Settings")]
    public Transform plateTransform;
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.green;

    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip deactivationSound;

    [Header("Events")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    [HideInInspector] public bool isActivated = false;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private Renderer plateRenderer;
    private AudioSource audioSource;
    private float currentWeight = 0f;

    private void Awake()
    {
        if (plateTransform == null)
            plateTransform = transform;

        originalPosition = plateTransform.localPosition;
        pressedPosition = originalPosition - Vector3.up * activationDepth;

        plateRenderer = plateTransform.GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        CheckWeight();
        UpdatePlatePosition();
    }

    private void CheckWeight()
    {
        currentWeight = 0f;

        // Get all rigidbodies that are colliding with this pressure plate
        Collider[] overlapping = Physics.OverlapBox(
            transform.position + Vector3.up * 0.1f,  // Slightly above the plate
            new Vector3(1f, 0.1f, 1f),  // Detection area size
            transform.rotation,
            detectionLayers
        );

        foreach (Collider col in overlapping)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Simple check: if object is close to the plate and not moving much
                float heightDifference = col.bounds.min.y - transform.position.y;
                if (heightDifference <= 0.5f && rb.velocity.magnitude < 1f)
                {
                    currentWeight += rb.mass;
                }
            }
        }

        // Debug the weight detection
        //Debug.Log($"Current weight on plate: {currentWeight}kg (Required: {requiredWeight}kg)");

        // Check activation state
        bool shouldBeActivated = currentWeight >= requiredWeight;

        if (shouldBeActivated && !isActivated)
        {
            Activate();
        }
        else if (!shouldBeActivated && isActivated)
        {
            Deactivate();
        }
    }

    private bool IsObjectRestingOnPlate(Collider objectCollider)
    {
        // Raycast downward from the object to see if it's actually resting on the plate
        Vector3 objectBottom = objectCollider.bounds.center - Vector3.up * (objectCollider.bounds.size.y / 2f);

        if (Physics.Raycast(objectBottom, Vector3.down, out RaycastHit hit, 0.5f))
        {
            return hit.collider == GetComponent<Collider>();
        }

        return false;
    }

    private void Activate()
    {
        isActivated = true;
        OnActivated.Invoke();
        UpdateVisuals();
        PlaySound(activationSound);

        Debug.Log($"Pressure plate activated! Weight: {currentWeight:F1}kg (Required: {requiredWeight}kg)");
    }

    private void Deactivate()
    {
        isActivated = false;
        OnDeactivated.Invoke();
        UpdateVisuals();
        PlaySound(deactivationSound);

        Debug.Log($"Pressure plate deactivated! Weight: {currentWeight:F1}kg");
    }

    private void UpdatePlatePosition()
    {
        if (plateTransform == null) return;

        Vector3 targetPosition = isActivated ? pressedPosition : originalPosition;
        plateTransform.localPosition = Vector3.Lerp(
            plateTransform.localPosition,
            targetPosition,
            Time.fixedDeltaTime * 10f
        );
    }

    private void UpdateVisuals()
    {
        if (plateRenderer != null)
        {
            plateRenderer.material.color = isActivated ? activeColor : inactiveColor;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);

        Gizmos.color = isActivated ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.2f);
    }
}
