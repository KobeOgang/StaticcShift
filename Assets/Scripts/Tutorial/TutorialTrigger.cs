using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private TutorialData tutorialToShow;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool autoFindTutorialManager = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Optional: Disable After Trigger")]
    [SerializeField] private bool disableTriggerAfterUse = true;

    private bool hasTriggered = false;

    private void Start()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} is not set as trigger! Setting it now.");
            triggerCollider.isTrigger = true;
        }

        if (autoFindTutorialManager && tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<TutorialManager>();

            if (tutorialManager == null)
            {
                Debug.LogError($"TutorialTrigger on {gameObject.name}: No TutorialManager found in scene!");
            }
        }

        if (tutorialToShow == null)
        {
            Debug.LogWarning($"TutorialTrigger on {gameObject.name}: No tutorial data assigned!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (tutorialToShow == null || tutorialManager == null)
        {
            Debug.LogWarning($"TutorialTrigger on {gameObject.name}: Missing tutorial data or manager!");
            return;
        }

        tutorialManager.ShowTutorial(tutorialToShow);

        hasTriggered = true;

        if (disableTriggerAfterUse && triggerOnce)
        {
            GetComponent<Collider>().enabled = false;
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        GetComponent<Collider>().enabled = true;
    }
}
