using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformParenting : MonoBehaviour
{
    [Header("Platform Reference")]
    public Transform platform;

    [Header("Parenting Settings")]
    public string playerTag = "Player";

    private Dictionary<Rigidbody, float> trackedRigidbodies = new Dictionary<Rigidbody, float>();
    private List<Transform> nonRigidbodyObjects = new List<Transform>();
    private Vector3 previousPlatformPosition;
    private Rigidbody platformRb;

    private void Start()
    {
        if (platform != null)
        {
            previousPlatformPosition = platform.position;
            platformRb = platform.GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        if (platform == null) return;

        Vector3 platformVelocity = (platform.position - previousPlatformPosition) / Time.fixedDeltaTime;

        foreach (var kvp in trackedRigidbodies)
        {
            Rigidbody rb = kvp.Key;
            if (rb != null)
            {
                float originalDrag = kvp.Value;

                rb.drag = 0f;

                Vector3 newVelocity = rb.velocity;
                newVelocity.x = platformVelocity.x;
                newVelocity.z = platformVelocity.z;

                rb.velocity = newVelocity;
            }
        }

        previousPlatformPosition = platform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (platform == null) return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
        {
            if (other.gameObject.CompareTag(playerTag) || other.GetComponent<InteractableObject>() != null)
            {
                if (!trackedRigidbodies.ContainsKey(rb))
                {
                    trackedRigidbodies.Add(rb, rb.drag);
                }
            }
        }
        else
        {
            if (other.gameObject.CompareTag(playerTag) || other.GetComponent<InteractableObject>() != null)
            {
                if (!nonRigidbodyObjects.Contains(other.transform))
                {
                    nonRigidbodyObjects.Add(other.transform);
                    other.transform.parent = platform;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
        {
            if (trackedRigidbodies.ContainsKey(rb))
            {
                rb.drag = trackedRigidbodies[rb];
                trackedRigidbodies.Remove(rb);
            }
        }
        else
        {
            if (nonRigidbodyObjects.Contains(other.transform))
            {
                nonRigidbodyObjects.Remove(other.transform);
                other.transform.parent = null;
            }
        }
    }
}
