using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button goToMainButton;

    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    public string mainMenuSceneName = "Main Menu";

    private bool isPaused = false;
    private PlayerCam playerCam;

    private void Start()
    {
        playerCam = FindObjectOfType<PlayerCam>();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (goToMainButton != null)
            goToMainButton.onClick.AddListener(GoToMainMenu);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        ResumeGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (playerCam != null)
            playerCam.IsCameraEnabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (playerCam != null)
            playerCam.IsCameraEnabled = true;

        StartCoroutine(LockCursorNextFrame());
    }

    private IEnumerator LockCursorNextFrame()
    {
        yield return null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        BGMHandler bgmHandler = FindObjectOfType<BGMHandler>();
        if (bgmHandler != null)
        {
            bgmHandler.PlayMenuBGM();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
