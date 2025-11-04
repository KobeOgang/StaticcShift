using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Standard,    // Can be freely manipulated
    Anchorable,  // Can be anchored in space
    Heavy        // Requires an anchor to manipulate
}

public class InteractableObject : MonoBehaviour
{
    [Header("Object Properties")]
    public ObjectType objectType = ObjectType.Standard;
    public float mass = 1f;
    public bool canBeAnchored = true;

    [Header("Manipulation Settings")]
    [Tooltip("Optional: Custom point for telekinesis manipulation. If null, uses object's transform position.")]
    public Transform customManipulationCenter;

    [Tooltip("If enabled, the object starts as kinematic and becomes non-kinematic when first manipulated.")]
    public bool startKinematic = false;

    [Header("Audio Settings")]
    [Tooltip("Sound effect played when this object collides with something")]
    public AudioClip collisionSFX;

    [Tooltip("Minimum collision velocity to trigger sound (prevents tiny bumps from making noise)")]
    [Range(0.1f, 5f)]
    public float minCollisionVelocity = 0.5f;

    [Tooltip("Auto-find AudioSource on this GameObject if not assigned")]
    public bool autoFindAudioSource = true;

    private AudioSource audioSource;

    [Header("Visual Feedback")]
    public Color highlightColor = Color.yellow;
    public Color anchoredColor = Color.blue;

    private Rigidbody rb;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isHighlighted = false;
    private bool isAnchored = false;
    private bool isBeingManipulated = false;
    private bool hasBeenManipulated = false;

    public bool IsAnchored => isAnchored;
    public bool IsBeingManipulated => isBeingManipulated;
    public Rigidbody Rigidbody => rb;
    public Vector3 ManipulationCenter => customManipulationCenter != null ? customManipulationCenter.position : transform.position;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
            originalColor = objectRenderer.material.color;

        if (objectType == ObjectType.Heavy && rb != null)
        {
            rb.mass = Mathf.Max(rb.mass, 10f);
        }

        if (startKinematic && rb != null)
        {
            rb.isKinematic = true;
        }

        if (autoFindAudioSource)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null && collisionSFX != null)
            {
                Debug.LogWarning($"InteractableObject on {gameObject.name}: CollisionSFX assigned but no AudioSource found!");
            }
        }
    }

    public void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted || objectRenderer == null) return;

        isHighlighted = highlighted;
        objectRenderer.material.color = highlighted ? highlightColor : originalColor;
    }

    public bool TryAnchor()
    {
        if (!canBeAnchored || isAnchored) return false;

        isAnchored = true;

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (objectRenderer != null)
            objectRenderer.material.color = anchoredColor;

        return true;
    }

    public void ReleaseAnchor()
    {
        if (!isAnchored) return;

        isAnchored = false;

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (objectRenderer != null)
            objectRenderer.material.color = originalColor;
    }

    public void SetManipulated(bool manipulated)
    {
        isBeingManipulated = manipulated;

        if (rb != null && manipulated)
        {
            if (startKinematic && !hasBeenManipulated)
            {
                rb.isKinematic = false;
                hasBeenManipulated = true;
            }

            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
        }
        else if (rb != null)
        {
            rb.drag = 1f;
            rb.angularDrag = 1f;
        }
    }

    public bool CanBeManipulated(bool hasAnchor)
    {
        if (isAnchored) return false;

        switch (objectType)
        {
            case ObjectType.Standard:
            case ObjectType.Anchorable:
                return true;
            case ObjectType.Heavy:
                return hasAnchor;
            default:
                return false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionSFX == null || audioSource == null) return;

        float impactVelocity = collision.relativeVelocity.magnitude;

        if (impactVelocity >= minCollisionVelocity)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(collisionSFX);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (customManipulationCenter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(customManipulationCenter.position, 0.2f);
            Gizmos.DrawLine(transform.position, customManipulationCenter.position);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(customManipulationCenter.position, 0.1f);
        }
    }
}
