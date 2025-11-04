using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NoteManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnShow = true;
    [SerializeField] private bool unlockCursorOnShow = true;
    [SerializeField] private bool disableCameraOnShow = true;
    [SerializeField] private bool autoFindPlayerCam = true;

    [Header("References")]
    [SerializeField] private PlayerCam playerCam;

    private NoteData currentNote;
    private bool isNoteOpen = false;
    private Action onNoteClosed;

    public bool IsNoteOpen => isNoteOpen;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseNote);
        }

        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }

        if (autoFindPlayerCam && playerCam == null)
        {
            playerCam = FindObjectOfType<PlayerCam>();

            if (playerCam == null && disableCameraOnShow)
            {
                Debug.LogWarning("NoteManager: PlayerCam not found in scene!");
            }
        }
    }

    public void ShowNote(NoteData noteData, Action onCloseCallback = null)
    {
        if (noteData == null)
        {
            Debug.LogWarning("NoteData is null! Cannot show note.");
            return;
        }

        currentNote = noteData;
        onNoteClosed = onCloseCallback;

        titleText.text = noteData.noteTitle;
        contentText.text = noteData.noteContent;

        notePanel.SetActive(true);
        isNoteOpen = true;

        if (pauseGameOnShow)
        {
            Time.timeScale = 0f;
        }

        if (unlockCursorOnShow)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (disableCameraOnShow && playerCam != null)
        {
            playerCam.IsCameraEnabled = false;
        }
    }

    public void CloseNote()
    {
        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }

        isNoteOpen = false;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCam != null)
        {
            playerCam.IsCameraEnabled = true;
        }

        currentNote = null;

        if (onNoteClosed != null)
        {
            onNoteClosed.Invoke();
            onNoteClosed = null;
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseNote);
        }
    }
}
