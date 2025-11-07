using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMHandler : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("BGM Tracks")]
    public AudioClip menuBGM;
    public AudioClip gameBGM;
    public AudioClip endingBGM;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    public float fadeOutDuration = 1f;
    public float fadeInDuration = 1f;

    private static BGMHandler instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.loop = true;
        audioSource.volume = volume;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayMenuBGM();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu")
        {
            PlayMenuBGM();
        }
    }

    public void PlayMenuBGM()
    {
        if (menuBGM != null && audioSource.clip != menuBGM)
        {
            CrossfadeToClip(menuBGM);
        }
    }

    public void PlayGameBGM()
    {
        if (gameBGM != null && audioSource.clip != gameBGM)
        {
            CrossfadeToClip(gameBGM);
        }
    }

    public void PlayEndingBGM()
    {
        if (endingBGM != null && audioSource.clip != endingBGM)
        {
            CrossfadeToClip(endingBGM);
        }
    }

    private void CrossfadeToClip(AudioClip newClip)
    {
        StopAllCoroutines();
        StartCoroutine(CrossfadeCoroutine(newClip));
    }

    private System.Collections.IEnumerator CrossfadeCoroutine(AudioClip newClip)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeOutDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, volume, t / fadeInDuration);
            yield return null;
        }

        audioSource.volume = volume;
    }
}
