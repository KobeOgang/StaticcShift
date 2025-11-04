using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;

    [Header("Typewriter Settings")]
    [Range(0.01f, 0.2f)]
    public float typewriterSpeed = 0.05f;

    [Tooltip("Characters per second when auto-calculating duration")]
    public float charactersPerSecond = 20f;

    [Header("Input")]
    public KeyCode skipKey = KeyCode.Q;

    private Coroutine currentDialogueCoroutine;
    private bool isTyping = false;
    private bool isDialoguePlaying = false;

    public bool IsDialoguePlaying => isDialoguePlaying;

    private void Awake()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false);
        }
    }

    public void PlayDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null || dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning("DialogueData is null or has no lines!");
            return;
        }

        if (isDialoguePlaying)
        {
            Debug.LogWarning("Dialogue is already playing. Stop current dialogue first.");
            return;
        }

        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        currentDialogueCoroutine = StartCoroutine(PlayDialogueSequence(dialogueData));
    }

    public void StopDialogue()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        isDialoguePlaying = false;
        isTyping = false;

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayDialogueSequence(DialogueData dialogueData)
    {
        isDialoguePlaying = true;
        dialogueText.gameObject.SetActive(true);

        foreach (DialogueLine line in dialogueData.dialogueLines)
        {
            yield return StartCoroutine(TypeLine(line.dialogueText));

            float duration = line.displayDuration > 0
                ? line.displayDuration
                : CalculateAutoDuration(line.dialogueText);

            float timer = 0f;
            while (timer < duration)
            {
                if (dialogueData.canSkip && Input.GetKeyDown(skipKey))
                {
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            dialogueText.text = "";
        }

        dialogueText.gameObject.SetActive(false);
        isDialoguePlaying = false;
        currentDialogueCoroutine = null;
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char character in line)
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
    }

    private float CalculateAutoDuration(string text)
    {
        return text.Length / charactersPerSecond;
    }
}
