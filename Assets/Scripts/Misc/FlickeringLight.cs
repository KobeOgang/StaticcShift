using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Light Reference")]
    [Tooltip("The light component to flicker. If null, searches on this GameObject.")]
    public Light targetLight;

    [Header("Flicker Settings")]
    [Tooltip("Enable random flickering effect.")]
    public bool enableFlicker = true;

    [Range(0f, 10f)]
    public float minIntensity = 0.5f;

    [Range(0f, 10f)]
    public float maxIntensity = 1.5f;

    [Range(0.01f, 1f)]
    [Tooltip("How fast the flicker changes. Lower = more erratic.")]
    public float flickerSpeed = 0.1f;

    [Range(0f, 1f)]
    [Tooltip("Chance each frame to trigger a flicker. 0 = never, 1 = constant.")]
    public float flickerChance = 0.3f;

    [Header("Telekinesis Interaction")]
    [Tooltip("If enabled, the light turns off permanently when first manipulated by telekinesis.")]
    public bool turnOffOnManipulation = false;

    private float targetIntensity;
    private float currentIntensity;
    private float baseIntensity;
    private bool isLightOn = true;
    private InteractableObject interactableObject;
    private bool hasBeenManipulated = false;

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
            currentIntensity = baseIntensity;
            targetIntensity = baseIntensity;
        }

        interactableObject = GetComponentInParent<InteractableObject>();
    }

    private void Start()
    {
        if (targetLight == null)
        {
            enabled = false;
            return;
        }

        if (enableFlicker)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
        }
    }

    private void Update()
    {
        if (targetLight == null) return;

        if (turnOffOnManipulation && interactableObject != null)
        {
            CheckManipulationState();
        }

        if (!isLightOn) return;

        if (enableFlicker)
        {
            UpdateFlicker();
        }
    }

    private void CheckManipulationState()
    {
        if (!hasBeenManipulated && interactableObject.IsBeingManipulated)
        {
            hasBeenManipulated = true;
            TurnOffLight();
        }
    }

    private void UpdateFlicker()
    {
        if (Random.value < flickerChance * Time.deltaTime * 60f)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
        }

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, flickerSpeed);
        targetLight.intensity = currentIntensity;
    }

    private void TurnOffLight()
    {
        isLightOn = false;
        enableFlicker = false;
        targetLight.enabled = false;
    }

    public void SetFlickerEnabled(bool enabled)
    {
        enableFlicker = enabled;
        if (!enabled && targetLight != null)
        {
            targetLight.intensity = baseIntensity;
        }
    }
}
