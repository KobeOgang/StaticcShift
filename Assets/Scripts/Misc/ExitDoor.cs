using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ExitDoor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The key to press to exit.")]
    public KeyCode interactKey = KeyCode.F;

    [Tooltip("Maximum distance player can interact from.")]
    public float interactionDistance = 3f;

    [Tooltip("Name of the scene to load. Must match the scene name exactly.")]
    public string nextSceneName;

    [Header("UI Settings")]
    [Tooltip("UI text to show the prompt. Will be created automatically if null.")]
    public TextMeshProUGUI promptText;

    public string promptMessage = "Press [F] to exit";

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Audio Settings")]
    public bool triggerEndingBGM = true;

    private Camera playerCamera;
    private bool isPlayerLookingAtDoor = false;
    private SceneFadeController fadeController;

    private void Awake()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        fadeController = FindObjectOfType<SceneFadeController>();
        if (fadeController == null)
        {
            GameObject fadeObj = new GameObject("SceneFadeController");
            fadeController = fadeObj.AddComponent<SceneFadeController>();
        }
    }

    private void Start()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckPlayerLookingAtDoor();

        if (isPlayerLookingAtDoor && Input.GetKeyDown(interactKey))
        {
            ExitScene();
        }
    }

    private void CheckPlayerLookingAtDoor()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                if (!isPlayerLookingAtDoor)
                {
                    isPlayerLookingAtDoor = true;
                    ShowPrompt(true);
                }
                return;
            }
        }

        if (isPlayerLookingAtDoor)
        {
            isPlayerLookingAtDoor = false;
            ShowPrompt(false);
        }
    }

    private void ShowPrompt(bool show)
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(show);
            if (show)
            {
                promptText.text = promptMessage;
            }
        }
    }

    private void ExitScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            return;
        }

        if (triggerEndingBGM)
        {
            BGMHandler bgmHandler = FindObjectOfType<BGMHandler>();
            if (bgmHandler != null)
            {
                bgmHandler.PlayEndingBGM();
            }
        }

        ShowPrompt(false);
        StartCoroutine(fadeController.FadeAndLoadScene(nextSceneName, fadeDuration));
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
