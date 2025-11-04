using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueData dialogueToPlay;

    [Header("Trigger Settings")]
    public string playerTag = "Player";
    public bool triggerOnce = true;
    public bool disableTriggerAfterUse = true;

    private DialogueManager dialogueManager;
    private bool hasTriggered = false;

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager not found in scene!");
        }

        if (dialogueToPlay == null)
        {
            Debug.LogWarning($"No DialogueData assigned to {gameObject.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (!other.CompareTag(playerTag)) return;

        if (dialogueManager != null && dialogueToPlay != null)
        {
            dialogueManager.PlayDialogue(dialogueToPlay);
            hasTriggered = true;

            if (disableTriggerAfterUse && triggerOnce)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        GetComponent<Collider>().enabled = true;
    }
}
