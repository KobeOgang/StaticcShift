using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorSystem : MonoBehaviour
{
    [Header("Anchor Settings")]
    public float maxAnchorDistance = 10f;
    public LayerMask anchorableLayers = -1;

    [Header("Audio Settings")]
    [Tooltip("Dedicated AudioSource for anchor sounds")]
    public AudioSource anchorAudioSource;
    public AudioClip anchorSFX;
    [Range(0f, 1f)]
    public float anchorVolume = 1f;

    private InteractableObject currentAnchor;
    private Camera playerCamera;

    public bool HasActiveAnchor => currentAnchor != null;
    public InteractableObject CurrentAnchor => currentAnchor;

    private void Awake()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        if (anchorAudioSource == null && anchorSFX != null)
        {
            Debug.LogWarning("AnchorSystem: Anchor SFX assigned but no AudioSource reference set!");
        }
    }

    public bool TryAnchorObject()
    {
        if (currentAnchor != null)
        {
            ReleaseAnchor();
            return false;
        }

        InteractableObject targetObject = GetAnchorableObjectAtCrosshair();

        if (targetObject != null && targetObject.TryAnchor())
        {
            currentAnchor = targetObject;

            if (anchorAudioSource != null && anchorSFX != null)
            {
                anchorAudioSource.PlayOneShot(anchorSFX, anchorVolume);
            }

            Debug.Log($"Anchored: {targetObject.name}");
            return true;
        }

        return false;
    }

    public void ReleaseAnchor()
    {
        if (currentAnchor != null)
        {
            currentAnchor.ReleaseAnchor();

            if (anchorAudioSource != null && anchorSFX != null)
            {
                anchorAudioSource.PlayOneShot(anchorSFX, anchorVolume);
            }

            Debug.Log($"Released anchor: {currentAnchor.name}");
            currentAnchor = null;
        }
    }

    private InteractableObject GetAnchorableObjectAtCrosshair()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, maxAnchorDistance, anchorableLayers))
        {
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();
            if (obj != null && obj.canBeAnchored && !obj.IsAnchored)
            {
                return obj;
            }
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.blue;
            Vector3 forward = playerCamera.transform.forward;
            Gizmos.DrawRay(playerCamera.transform.position, forward * maxAnchorDistance);
        }
    }
}
