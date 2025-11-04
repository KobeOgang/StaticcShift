using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReadableNote : MonoBehaviour
{
    [Header("Note Settings")]
    [SerializeField] private NoteData noteData;
    [SerializeField] private NoteManager noteManager;

    [Header("Dialogue Settings")]
    [Tooltip("Optional: Dialogue to play after reading the note for the first time")]
    [SerializeField] private DialogueData dialogueAfterReading;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private bool autoFindDialogueManager = true;

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private bool autoFindNoteManager = true;
    [SerializeField] private LayerMask playerLayer = -1;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private TextMeshProUGUI interactionPrompt;
    [SerializeField] private string customPromptText = "Press [F] to Read";
    private TextMeshProUGUI promptTextComponent;

    private bool isPlayerLooking = false;
    private bool hasPlayedDialogue = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (autoFindNoteManager && noteManager == null)
        {
            noteManager = FindObjectOfType<NoteManager>();

            if (noteManager == null)
            {
                Debug.LogError($"ReadableNote on {gameObject.name}: No NoteManager found in scene!");
            }
        }

        if (autoFindDialogueManager && dialogueManager == null && dialogueAfterReading != null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();

            if (dialogueManager == null)
            {
                Debug.LogWarning($"ReadableNote on {gameObject.name}: DialogueAfterReading assigned but no DialogueManager found in scene!");
            }
        }

        if (noteData == null)
        {
            Debug.LogWarning($"ReadableNote on {gameObject.name}: No note data assigned!");
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.text = customPromptText;
            interactionPrompt.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckPlayerLookingAt();

        if (isPlayerLooking && Input.GetKeyDown(interactKey))
        {
            ReadNote();
        }
    }

    private void CheckPlayerLookingAt()
    {
        if (mainCamera == null || noteManager == null)
        {
            return;
        }

        if (noteManager.IsNoteOpen)
        {
            if (isPlayerLooking)
            {
                SetPlayerLooking(false);
            }
            return;
        }

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!isPlayerLooking)
                {
                    SetPlayerLooking(true);
                }
            }
            else
            {
                if (isPlayerLooking)
                {
                    SetPlayerLooking(false);
                }
            }
        }
        else
        {
            if (isPlayerLooking)
            {
                SetPlayerLooking(false);
            }
        }
    }

    private void SetPlayerLooking(bool looking)
    {
        isPlayerLooking = looking;

        if (interactionPrompt != null)
        {
            if (looking)
            {
                interactionPrompt.text = customPromptText;
            }
            interactionPrompt.gameObject.SetActive(looking);
        }
    }

    private void ReadNote()
    {
        if (noteData == null || noteManager == null)
        {
            return;
        }

        if (dialogueAfterReading != null && !hasPlayedDialogue)
        {
            noteManager.ShowNote(noteData, OnNoteClosedFirstTime);
        }
        else
        {
            noteManager.ShowNote(noteData);
        }
    }

    private void OnNoteClosedFirstTime()
    {
        if (dialogueAfterReading != null && dialogueManager != null && !hasPlayedDialogue)
        {
            hasPlayedDialogue = true;
            dialogueManager.PlayDialogue(dialogueAfterReading);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null) return;

        Gizmos.color = isPlayerLooking ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);

        if (dialogueAfterReading != null)
        {
            Gizmos.color = hasPlayedDialogue ? Color.gray : Color.magenta;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, Vector3.one * 0.3f);
        }
    }
}
