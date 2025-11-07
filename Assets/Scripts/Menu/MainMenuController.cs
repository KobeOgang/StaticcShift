using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Button References")]
    public Button playButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";

    [Header("Credits Panel")]
    public GameObject creditsPanel;
    public float creditsDuration = 4f;

    private Coroutine creditsCoroutine;
    private BGMHandler bgmHandler;

    private void Start()
    {
        bgmHandler = FindObjectOfType<BGMHandler>();

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void OnPlayClicked()
    {
        if (bgmHandler != null)
        {
            bgmHandler.PlayGameBGM();
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnCreditsClicked()
    {
        if (creditsPanel == null) return;

        if (creditsCoroutine != null)
        {
            StopCoroutine(creditsCoroutine);
        }

        creditsCoroutine = StartCoroutine(ShowCreditsPanel());
    }

    public void OnQuitClicked()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    private IEnumerator ShowCreditsPanel()
    {
        creditsPanel.SetActive(true);

        yield return new WaitForSeconds(creditsDuration);

        creditsPanel.SetActive(false);
        creditsCoroutine = null;
    }
}
