using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsOverlay : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup settingsOverlay;
    public Button closeButton;         // Close button inside settings panel

    [Header("Settings Controls")]
    public TMP_InputField populationInputField;   // NEW: Population input field
    public NPCPopulationController npcController; // NEW: Reference to NPC controller

    private bool isSettingsOpen = false;

    void Start()
    {
        // Connect close button
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);

        // The NPCPopulationController will automatically handle the input field
        // No additional setup needed here!

        // Ensure settings start closed
        CloseSettings();
    }

    public void ToggleSettings()
    {
        if (isSettingsOpen)
            CloseSettings();
        else
            OpenSettings();
    }

    public void OpenSettings()
    {
        settingsOverlay.alpha = 1f;
        settingsOverlay.interactable = true;
        settingsOverlay.blocksRaycasts = true;
        isSettingsOpen = true;

        Debug.Log("Settings panel opened");

        // Optional: Pause simulation
        // Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        settingsOverlay.alpha = 0f;
        settingsOverlay.interactable = false;
        settingsOverlay.blocksRaycasts = false;
        isSettingsOpen = false;

        Debug.Log("Settings panel closed");

        // Optional: Resume simulation
        // Time.timeScale = 1f;
    }
}