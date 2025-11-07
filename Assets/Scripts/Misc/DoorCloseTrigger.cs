using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCloseTrigger : MonoBehaviour
{
    [Header("Door Reference")]
    [Tooltip("The PuzzleDoor that will be forced to close when player enters.")]
    public PuzzleDoor targetDoor;

    [Header("Trigger Settings")]
    [Tooltip("Tag of the player GameObject.")]
    public string playerTag = "Player";

    [Tooltip("Should the trigger only work once?")]
    public bool triggerOnce = true;

    [Header("Options")]
    [Tooltip("Release any active anchors when triggered.")]
    public bool releaseAnchors = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag(playerTag))
        {
            TriggerDoorClose(other.gameObject);
        }
    }

    private void TriggerDoorClose(GameObject player)
    {
        if (targetDoor != null)
        {
            targetDoor.CloseDoor();
            Debug.Log($"Trigger activated: Closing door '{targetDoor.name}'");
        }
        else
        {
            Debug.LogWarning("DoorCloseTrigger: No target door assigned!");
        }

        if (releaseAnchors)
        {
            AnchorSystem anchorSystem = player.GetComponent<AnchorSystem>();
            if (anchorSystem == null)
            {
                anchorSystem = player.GetComponentInChildren<AnchorSystem>();
            }

            if (anchorSystem != null && anchorSystem.HasActiveAnchor)
            {
                anchorSystem.ReleaseAnchor();
                Debug.Log("Trigger activated: Released active anchor");
            }
        }

        hasTriggered = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = hasTriggered ? Color.gray : Color.red;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null && boxCollider.isTrigger)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null && sphereCollider.isTrigger)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null && capsuleCollider.isTrigger)
        {
            Gizmos.color = hasTriggered ? Color.gray : Color.red;
            Gizmos.DrawWireSphere(transform.position, capsuleCollider.radius);
        }

        if (targetDoor != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetDoor.transform.position);
        }
    }
}
