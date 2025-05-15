using UnityEngine;
using System.Collections;

public class EnhancedSkyboxFader : MonoBehaviour
{
    public Material[] skyboxes;
    public float changeInterval = 10f;
    public float fadeTime = 2f;
    
    // Fade color options
    public Color fadeOutColor = Color.black;  // First fade (from current skybox)
    public Color fadeInColor = Color.white;   // Second fade (to next skybox)
    
    // Whether to apply tint, exposure, or both
    public bool useTint = true;
    public bool useExposure = true;
    
    private int currentSkyboxIndex = 0;
    private float timer = 0f;
    
    void Start()
    {
        // Set initial skybox
        if (skyboxes.Length > 0)
        {
            RenderSettings.skybox = skyboxes[currentSkyboxIndex];
        }
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= changeInterval)
        {
            // Time to start transition
            StartCoroutine(FadeSkyboxes());
            timer = 0f;
        }
    }
    
    IEnumerator FadeSkyboxes()
    {
        // Determine which skybox to switch to next
        int nextSkyboxIndex = (currentSkyboxIndex + 1) % skyboxes.Length;
        
        // Store original values
        float originalExposure = 1f;
        Color originalTint = Color.white;
        
        if (useExposure)
        {
            originalExposure = RenderSettings.skybox.GetFloat("_Exposure");
            if (originalExposure == 0) originalExposure = 1; // Default exposure if not set
        }
        
        if (useTint && RenderSettings.skybox.HasProperty("_Tint"))
        {
            originalTint = RenderSettings.skybox.GetColor("_Tint");
        }
        
        // Fade out current skybox toward fadeOutColor
        float t = 0;
        while (t < fadeTime / 2)
        {
            t += Time.deltaTime;
            float normalizedTime = t / (fadeTime / 2);
            
            // Apply exposure fade
            if (useExposure)
            {
                float targetExposure = fadeOutColor.grayscale;
                float newExposure = Mathf.Lerp(originalExposure, targetExposure, normalizedTime);
                RenderSettings.skybox.SetFloat("_Exposure", newExposure);
            }
            
            // Apply tint fade
            if (useTint && RenderSettings.skybox.HasProperty("_Tint"))
            {
                Color newTint = Color.Lerp(originalTint, fadeOutColor, normalizedTime);
                RenderSettings.skybox.SetColor("_Tint", newTint);
            }
            
            yield return null;
        }
        
        // Switch to the next skybox
        RenderSettings.skybox = skyboxes[nextSkyboxIndex];
        currentSkyboxIndex = nextSkyboxIndex;
        
        // Store next skybox's original values
        float nextOriginalExposure = 1f;
        Color nextOriginalTint = Color.white;
        
        if (useExposure)
        {
            nextOriginalExposure = RenderSettings.skybox.GetFloat("_Exposure");
            if (nextOriginalExposure == 0) nextOriginalExposure = 1; // Default exposure if not set
            
            // Start with fade color exposure
            RenderSettings.skybox.SetFloat("_Exposure", fadeInColor.grayscale);
        }
        
        if (useTint && RenderSettings.skybox.HasProperty("_Tint"))
        {
            nextOriginalTint = RenderSettings.skybox.GetColor("_Tint");
            
            // Start with fade color
            RenderSettings.skybox.SetColor("_Tint", fadeInColor);
        }
        
        // Fade in from fadeInColor to the new skybox
        t = 0;
        while (t < fadeTime / 2)
        {
            t += Time.deltaTime;
            float normalizedTime = t / (fadeTime / 2);
            
            // Apply exposure fade
            if (useExposure)
            {
                float newExposure = Mathf.Lerp(fadeInColor.grayscale, nextOriginalExposure, normalizedTime);
                RenderSettings.skybox.SetFloat("_Exposure", newExposure);
            }
            
            // Apply tint fade
            if (useTint && RenderSettings.skybox.HasProperty("_Tint"))
            {
                Color newTint = Color.Lerp(fadeInColor, nextOriginalTint, normalizedTime);
                RenderSettings.skybox.SetColor("_Tint", newTint);
            }
            
            yield return null;
        }
        
        // Make sure we end with the correct values
        if (useExposure)
        {
            RenderSettings.skybox.SetFloat("_Exposure", nextOriginalExposure);
        }
        
        if (useTint && RenderSettings.skybox.HasProperty("_Tint"))
        {
            RenderSettings.skybox.SetColor("_Tint", nextOriginalTint);
        }
        
        // Update the environment to ensure reflections are updated
        DynamicGI.UpdateEnvironment();
    }
}