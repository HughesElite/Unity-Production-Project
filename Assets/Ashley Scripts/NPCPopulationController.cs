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

    [Header("Smart Population Management")]
    [Tooltip("When enabled, prioritizes keeping infected NPCs active when changing population")]
    public bool smartPopulationManagement = true;

    [Header("Reset Integration")]
    public NPCResetManager resetManager; // Drag your reset manager here
    public UIResetManager uiResetManager; // Drag your UI reset manager here
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

        // Auto-find reset managers if not assigned
        if (resetManager == null)
        {
            resetManager = FindFirstObjectByType<NPCResetManager>();
        }

        if (uiResetManager == null)
        {
            uiResetManager = FindFirstObjectByType<UIResetManager>();
        }

        // Validate we have enough NPCs
        if (allNPCs.Length < maxPopulation)
        {
            Debug.LogWarning($"Only {allNPCs.Length} NPCs found, but max population is {maxPopulation}. Consider adding more NPCs or lowering max population.");
        }

        // Setup input field - prefer TextMeshPro
       // InputField activeInputField = null;
        if (populationInputField != null)
        {
            populationInputField.text = ""; // Empty field to show placeholder
            populationInputField.onEndEdit.AddListener(OnPopulationInputChanged);
            populationInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            //activeInputField = null; // TMP doesn't inherit from InputField
        }
        else if (legacyInputField != null)
        {
            legacyInputField.text = defaultPopulation.ToString();
            legacyInputField.onEndEdit.AddListener(OnPopulationInputChanged);
            legacyInputField.contentType = InputField.ContentType.IntegerNumber;
           // activeInputField = legacyInputField;
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
            if (autoResetOnPopulationChange)
            {
                // Reset NPCs (positions and virus states)
                if (resetManager != null)
                {
                    resetManager.ResetAllNPCs();
                    Debug.Log($"NPC reset triggered for population of {clampedPopulation}");
                }

                // Reset UI displays
                if (uiResetManager != null)
                {
                    uiResetManager.ResetStatistics(); // Only reset the stats, not everything
                    Debug.Log($"UI reset triggered for population of {clampedPopulation}");
                }
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

        // Choose population management method
        if (smartPopulationManagement && !autoResetOnPopulationChange)
        {
            SetPopulationSmart(actualPopulation);
        }
        else
        {
            // Use original simple method when auto-reset is enabled or smart management is disabled
            SetPopulationSimple(actualPopulation);
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

        // Force update UI displays immediately after population change
        if (uiResetManager != null)
        {
            uiResetManager.ResetStatistics(); // This will update PopulationCounter and VirusStatsDisplay
        }
    }

    private void SetPopulationSimple(int actualPopulation)
    {
        // Original simple method: enable first N NPCs
        for (int i = 0; i < allNPCs.Length; i++)
        {
            if (allNPCs[i] != null)
            {
                bool shouldBeActive = i < actualPopulation;
                allNPCs[i].SetActive(shouldBeActive);
            }
        }
    }

    private void SetPopulationSmart(int actualPopulation)
    {
        // Smart method: prioritize keeping infected NPCs active
        System.Collections.Generic.List<GameObject> infectedNPCs = new System.Collections.Generic.List<GameObject>();
        System.Collections.Generic.List<GameObject> recoveredNPCs = new System.Collections.Generic.List<GameObject>();
        System.Collections.Generic.List<GameObject> healthyNPCs = new System.Collections.Generic.List<GameObject>();

        // Categorize all NPCs by their current state
        foreach (GameObject npc in allNPCs)
        {
            if (npc != null)
            {
                VirusSimulation virusScript = npc.GetComponent<VirusSimulation>();
                if (virusScript != null)
                {
                    if (virusScript.IsInfected())
                    {
                        infectedNPCs.Add(npc);
                    }
                    else if (virusScript.IsRecovered())
                    {
                        recoveredNPCs.Add(npc);
                    }
                    else
                    {
                        healthyNPCs.Add(npc);
                    }
                }
                else
                {
                    // No virus script, assume healthy
                    healthyNPCs.Add(npc);
                }
            }
        }

        // First, deactivate all NPCs
        foreach (GameObject npc in allNPCs)
        {
            if (npc != null)
                npc.SetActive(false);
        }

        int slotsRemaining = actualPopulation;
        int infectedKept = 0;
        int recoveredKept = 0;
        int healthyKept = 0;

        // Priority 1: Keep infected NPCs (up to population limit)
        foreach (GameObject npc in infectedNPCs)
        {
            if (slotsRemaining > 0)
            {
                npc.SetActive(true);
                slotsRemaining--;
                infectedKept++;
            }
        }

        // Priority 2: Keep recovered NPCs
        foreach (GameObject npc in recoveredNPCs)
        {
            if (slotsRemaining > 0)
            {
                npc.SetActive(true);
                slotsRemaining--;
                recoveredKept++;
            }
        }

        // Priority 3: Fill remaining slots with healthy NPCs
        foreach (GameObject npc in healthyNPCs)
        {
            if (slotsRemaining > 0)
            {
                npc.SetActive(true);
                slotsRemaining--;
                healthyKept++;
            }
        }

        // Log the smart allocation results
        if (infectedNPCs.Count > 0 || recoveredNPCs.Count > 0)
        {
            string smartMessage = $"Smart population allocation: {infectedKept}/{infectedNPCs.Count} infected, " +
                                 $"{recoveredKept}/{recoveredNPCs.Count} recovered, {healthyKept} healthy";

            if (infectedNPCs.Count > actualPopulation)
            {
                int droppedInfected = infectedNPCs.Count - infectedKept;
                smartMessage += $" (WARNING: {droppedInfected} infected NPCs deactivated due to population limit)";
                Debug.LogWarning(smartMessage);
            }
            else
            {
                Debug.Log(smartMessage);
            }
        }
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

    // Toggle smart population management at runtime
    public void SetSmartPopulationManagement(bool enabled)
    {
        smartPopulationManagement = enabled;
        Debug.Log($"Smart population management: {(enabled ? "Enabled" : "Disabled")}");
    }

    // Get statistics about current NPC states for debugging
    [ContextMenu("Log NPC Distribution")]
    public void LogNPCDistribution()
    {
        if (allNPCs == null) return;

        int activeCount = 0, infectedCount = 0, recoveredCount = 0, healthyCount = 0;

        foreach (GameObject npc in allNPCs)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                activeCount++;
                VirusSimulation virusScript = npc.GetComponent<VirusSimulation>();
                if (virusScript != null)
                {
                    if (virusScript.IsInfected()) infectedCount++;
                    else if (virusScript.IsRecovered()) recoveredCount++;
                    else healthyCount++;
                }
                else
                {
                    healthyCount++;
                }
            }
        }

        Debug.Log($"Active NPCs: {activeCount} (Infected: {infectedCount}, Recovered: {recoveredCount}, Healthy: {healthyCount})");
    }
}