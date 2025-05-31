using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCPopulationController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField populationInputField; // TextMeshPro InputField
    public InputField legacyInputField;         // Legacy InputField (as backup)
    public Text feedbackText;                   // Shows current population status

    [Header("NPC Control")]
    public GameObject[] npcObjects; // Drag your NPCs here
    public string npcTag = "NPC"; // Or find NPCs by tag
    public bool findNPCsByTag = true; // Auto-find NPCs vs manual assignment

    [Header("Population Settings")]
    [Range(2, 50)]
    public int minPopulation = 2;
    [Range(2, 50)]
    public int maxPopulation = 50;
    public int defaultPopulation = 10;

    [Header("Reset Integration")]
    public NPCResetManager resetManager; // Drag your reset manager here
    public bool autoResetOnPopulationChange = true; // Auto-reset when population changes

    private GameObject[] allNPCs;
    private int currentPopulation;

    void Start()
    {
        // Find NPCs in scene
        if (findNPCsByTag)
        {
            allNPCs = GameObject.FindGameObjectsWithTag(npcTag);
            Debug.Log($"Found {allNPCs.Length} NPCs with tag '{npcTag}'");
        }
        else
        {
            allNPCs = npcObjects;
        }

        // Auto-find reset manager if not assigned
        if (resetManager == null)
        {
            resetManager = FindFirstObjectByType<NPCResetManager>();
        }

        // Validate we have enough NPCs
        if (allNPCs.Length < maxPopulation)
        {
            Debug.LogWarning($"Only {allNPCs.Length} NPCs found, but max population is {maxPopulation}. Consider adding more NPCs or lowering max population.");
        }

        // Setup input field - prefer TextMeshPro
        InputField activeInputField = null;
        if (populationInputField != null)
        {
            populationInputField.text = ""; // Empty field to show placeholder
            populationInputField.onEndEdit.AddListener(OnPopulationInputChanged);
            populationInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            activeInputField = null; // TMP doesn't inherit from InputField
        }
        else if (legacyInputField != null)
        {
            legacyInputField.text = defaultPopulation.ToString();
            legacyInputField.onEndEdit.AddListener(OnPopulationInputChanged);
            legacyInputField.contentType = InputField.ContentType.IntegerNumber;
            activeInputField = legacyInputField;
        }

        // Set initial population
        SetPopulation(defaultPopulation);
    }

    public void OnPopulationInputChanged(string inputValue)
    {
        // Try to parse the input
        if (int.TryParse(inputValue, out int requestedPopulation))
        {
            // Clamp to valid range
            int clampedPopulation = Mathf.Clamp(requestedPopulation, minPopulation, maxPopulation);

            // Update input field if it was clamped
            if (clampedPopulation != requestedPopulation)
            {
                UpdateInputFieldText(clampedPopulation.ToString());
                Debug.Log($"Population clamped from {requestedPopulation} to {clampedPopulation}");
            }

            SetPopulation(clampedPopulation);

            // Auto-reset if enabled
            if (autoResetOnPopulationChange && resetManager != null)
            {
                resetManager.ResetAllNPCs();
                Debug.Log($"Auto-reset triggered for population of {clampedPopulation}");
            }
        }
        else
        {
            // Invalid input - reset to current population
            Debug.LogWarning($"Invalid input: '{inputValue}'. Resetting to {currentPopulation}");
            UpdateInputFieldText(currentPopulation.ToString());
            UpdateFeedbackText($"Invalid input! Enter a number between {minPopulation} and {maxPopulation}", Color.red);
        }
    }

    private void SetPopulation(int targetPopulation)
    {
        if (allNPCs == null || allNPCs.Length == 0)
        {
            UpdateFeedbackText("No NPCs found in scene!", Color.red);
            return;
        }

        // Ensure we don't exceed available NPCs
        int actualPopulation = Mathf.Min(targetPopulation, allNPCs.Length);

        // Enable/disable NPCs
        for (int i = 0; i < allNPCs.Length; i++)
        {
            if (allNPCs[i] != null)
            {
                bool shouldBeActive = i < actualPopulation;
                allNPCs[i].SetActive(shouldBeActive);
            }
        }

        currentPopulation = actualPopulation;

        // Update feedback
        string message = $"Population: {actualPopulation} NPCs active";
        if (actualPopulation != targetPopulation)
        {
            message += $" (requested {targetPopulation}, limited by available NPCs)";
        }

        UpdateFeedbackText(message, Color.white);
        Debug.Log(message);
    }

    private void UpdateInputFieldText(string text)
    {
        if (populationInputField != null)
            populationInputField.text = text;
        else if (legacyInputField != null)
            legacyInputField.text = text;
    }

    private void UpdateFeedbackText(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }

    // Public methods for other scripts to call
    public void SetPopulationToMin()
    {
        UpdateInputFieldText(minPopulation.ToString());
        SetPopulation(minPopulation);
        if (autoResetOnPopulationChange && resetManager != null)
            resetManager.ResetAllNPCs();
    }

    public void SetPopulationToMax()
    {
        int maxPossible = Mathf.Min(maxPopulation, allNPCs.Length);
        UpdateInputFieldText(maxPossible.ToString());
        SetPopulation(maxPossible);
        if (autoResetOnPopulationChange && resetManager != null)
            resetManager.ResetAllNPCs();
    }

    public void ResetToDefault()
    {
        UpdateInputFieldText(defaultPopulation.ToString());
        SetPopulation(defaultPopulation);
        if (autoResetOnPopulationChange && resetManager != null)
            resetManager.ResetAllNPCs();
    }

    // Getter for current population
    public int GetCurrentPopulation()
    {
        return currentPopulation;
    }
}