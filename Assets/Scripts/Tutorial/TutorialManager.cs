using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image imageDisplay;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private Button understandButton;

    [Header("Video Player")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Settings")]
    [SerializeField] private bool pauseGameOnShow = true;
    [SerializeField] private bool unlockCursorOnShow = true;
    [SerializeField] private bool disableCameraOnShow = true;
    [SerializeField] private bool autoFindPlayerCam = true;

    [Header("References")]
    [SerializeField] private PlayerCam playerCam;

    private TutorialData currentTutorial;
    private RenderTexture videoRenderTexture;

    private void Start()
    {
        if (understandButton != null)
        {
            understandButton.onClick.AddListener(OnUnderstandClicked);
        }

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (autoFindPlayerCam && playerCam == null)
        {
            playerCam = FindObjectOfType<PlayerCam>();

            if (playerCam == null && disableCameraOnShow)
            {
                Debug.LogWarning("TutorialManager: PlayerCam not found in scene!");
            }
        }

        SetupVideoPlayer();
    }

    private void SetupVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        if (videoRenderTexture == null)
        {
            videoRenderTexture = new RenderTexture(1920, 1080, 0);
        }

        videoPlayer.targetTexture = videoRenderTexture;

        if (videoDisplay != null)
        {
            videoDisplay.texture = videoRenderTexture;
        }
    }

    public void ShowTutorial(TutorialData tutorialData)
    {
        if (tutorialData == null)
        {
            Debug.LogWarning("TutorialData is null! Cannot show tutorial.");
            return;
        }

        currentTutorial = tutorialData;

        titleText.text = tutorialData.tutorialTitle;
        descriptionText.text = tutorialData.tutorialDescription;

        HandleVisualContent(tutorialData);

        tutorialPanel.SetActive(true);

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

    private void HandleVisualContent(TutorialData tutorialData)
    {
        if (imageDisplay != null)
        {
            imageDisplay.gameObject.SetActive(false);
        }

        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
        }

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        switch (tutorialData.contentType)
        {
            case TutorialData.VisualContentType.Image:
                if (tutorialData.tutorialImage != null && imageDisplay != null)
                {
                    imageDisplay.sprite = tutorialData.tutorialImage;
                    imageDisplay.gameObject.SetActive(true);
                }
                break;

            case TutorialData.VisualContentType.Video:
                if (tutorialData.tutorialVideo != null && videoPlayer != null && videoDisplay != null)
                {
                    videoPlayer.clip = tutorialData.tutorialVideo;
                    videoDisplay.gameObject.SetActive(true);
                    videoPlayer.Play();
                }
                break;

            case TutorialData.VisualContentType.None:
                break;
        }
    }

    private void OnUnderstandClicked()
    {
        CloseTutorial();
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCam != null)
        {
            playerCam.IsCameraEnabled = true;
        }

        currentTutorial = null;
    }

    private void OnDestroy()
    {
        if (understandButton != null)
        {
            understandButton.onClick.RemoveListener(OnUnderstandClicked);
        }

        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
        }
    }
}
