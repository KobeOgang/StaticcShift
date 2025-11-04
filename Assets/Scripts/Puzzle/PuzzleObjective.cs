using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleObjective : MonoBehaviour
{
    [Header("Objective Settings")]
    public string objectiveName = "Reach the Exit";
    public bool isCompleted = false;

    [Header("Completion Requirements")]
    public PressurePlate[] requiredPressurePlates;
    public PuzzleDoor[] requiredOpenDoors;
    public Transform playerTarget; // Where player needs to go
    public float completionRadius = 2f;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnObjectiveCompleted;

    private Transform player;
    private bool hasTriggeredCompletion = false;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>()?.transform;

        // Subscribe to pressure plate events if needed
        foreach (var plate in requiredPressurePlates)
        {
            if (plate != null)
            {
                plate.OnActivated.AddListener(CheckObjectiveCompletion);
                plate.OnDeactivated.AddListener(CheckObjectiveCompletion);
            }
        }
    }

    private void Update()
    {
        CheckObjectiveCompletion();
    }

    private void CheckObjectiveCompletion()
    {
        if (isCompleted || hasTriggeredCompletion) return;

        bool allRequirementsMet = true;

        // Check pressure plates
        foreach (var plate in requiredPressurePlates)
        {
            if (plate != null && !plate.isActivated)
            {
                allRequirementsMet = false;
                break;
            }
        }

        // Check doors
        foreach (var door in requiredOpenDoors)
        {
            if (door != null && !door.isOpen)
            {
                allRequirementsMet = false;
                break;
            }
        }

        // Check player position
        if (playerTarget != null && player != null)
        {
            float distance = Vector3.Distance(player.position, playerTarget.position);
            if (distance > completionRadius)
            {
                allRequirementsMet = false;
            }
        }

        if (allRequirementsMet && !hasTriggeredCompletion)
        {
            CompleteObjective();
        }
    }

    private void CompleteObjective()
    {
        isCompleted = true;
        hasTriggeredCompletion = true;

        OnObjectiveCompleted.Invoke();

        Debug.Log($"Objective completed: {objectiveName}");

        // Optional: Show completion UI or transition to next level
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTarget != null)
        {
            Gizmos.color = isCompleted ? Color.green : Color.blue;
            Gizmos.DrawWireSphere(playerTarget.position, completionRadius);
        }
    }
}
