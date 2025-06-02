using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NPCPopulationController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField populationInputField; 
    public InputField legacyInputField;         
    public Text feedbackText;                   

    [Header("Infection Control UI")]
    public TMP_InputField infectedCountInputField; 
    public Text infectionFeedbackText;             

    [Header("NPC Control")]
    public GameObject[] npcObjects; 
    public string npcTag = "NPC"; 
    public bool findNPCsByTag = true; 

    [Header("Population Settings")]
    [Range(2, 50)]
    public int minPopulation = 2;
    [Range(2, 50)]
    public int maxPopulation = 50;
    public int defaultPopulation = 10;

    [Header("Infection Settings")]
    [Range(0, 50)]
    public int defaultInfectedCount = 1;
    public bool randomizeInfectedSelection = true; // Randomly select which NPCs to infect

    [Header("Reset Integration")]
    public NPCResetManager resetManager; // place our reset manager here
    public UIResetManager uiResetManager; // place our UI reset manager here

    private GameObject[] allNPCs;
    private int currentPopulation;
    private int currentInfectedCount;

    void Start()
    {
        // Locate NPCs in the scene using tag or manual array
        if (findNPCsByTag)
        {
            allNPCs = GameObject.FindGameObjectsWithTag(npcTag);
            Debug.Log($"Found {allNPCs.Length} NPCs with tag '{npcTag}'");
        }
        else
        {
            allNPCs = npcObjects;
        }

        // Auto-locate reset managers if not manually assigned
        if (resetManager == null)
        {
            resetManager = FindFirstObjectByType<NPCResetManager>();
        }

        if (uiResetManager == null)
        {
            uiResetManager = FindFirstObjectByType<UIResetManager>();
        }

        // Validate we have enough NPCs for the maximum population setting
        if (allNPCs.Length < maxPopulation)
        {
            Debug.LogWarning($"Only {allNPCs.Length} NPCs found, but max population is {maxPopulation}. Consider adding more NPCs or lowering max population.");
        }

        // Configure population input field with validation and default value
        if (populationInputField != null)
        {
            populationInputField.text = defaultPopulation.ToString();
            populationInputField.onEndEdit.AddListener(OnPopulationInputChanged);
            populationInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        }
        else if (legacyInputField != null)
        {
            legacyInputField.text = defaultPopulation.ToString();
            legacyInputField.onEndEdit.AddListener(OnPopulationInputChanged);
            legacyInputField.contentType = InputField.ContentType.IntegerNumber;
        }

        // Configure infected count input field with validation
        if (infectedCountInputField != null)
        {
            infectedCountInputField.text = defaultInfectedCount.ToString();
            infectedCountInputField.onEndEdit.AddListener(OnInfectedCountChanged);
            infectedCountInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        }

        // Apply initial settings to start the simulation
        SetPopulation(defaultPopulation);
        SetInfectedCount(defaultInfectedCount);
    }

    public void OnPopulationInputChanged(string inputValue)
    {
        // Parse and validate user input for population size
        if (int.TryParse(inputValue, out int requestedPopulation))
        {
            // Enforce population limits to prevent simulation issues
            int clampedPopulation = Mathf.Clamp(requestedPopulation, minPopulation, maxPopulation);

            // Update UI if value was adjusted to fit within limits
            if (clampedPopulation != requestedPopulation)
            {
                UpdatePopulationInputFieldText(clampedPopulation.ToString());
                Debug.Log($"Population clamped from {requestedPopulation} to {clampedPopulation}");
            }

            // Reset entire simulation with new population to ensure clean state
            ResetSimulationWithNewValues(clampedPopulation, Mathf.Min(currentInfectedCount, clampedPopulation));
        }
        else
        {
            // Handle invalid input by reverting to current valid value
            Debug.LogWarning($"Invalid input: '{inputValue}'. Resetting to {currentPopulation}");
            UpdatePopulationInputFieldText(currentPopulation.ToString());
            UpdateFeedbackText($"Invalid input! Enter a number between {minPopulation} and {maxPopulation}", Color.red);
        }
    }

    public void OnInfectedCountChanged(string inputValue)
    {
        if (int.TryParse(inputValue, out int requestedInfected))
        {
            // Infected count cannot exceed current population
            int clampedInfected = Mathf.Clamp(requestedInfected, 0, currentPopulation);

            // Update UI if value was adjusted to fit within limits
            if (clampedInfected != requestedInfected)
            {
                UpdateInfectedInputFieldText(clampedInfected.ToString());
                Debug.Log($"Infected count clamped from {requestedInfected} to {clampedInfected}");
            }

            // Reset simulation to apply new infection distribution
            ResetSimulationWithNewValues(currentPopulation, clampedInfected);
        }
        else
        {
            // Handle invalid input by reverting to current valid value
            Debug.LogWarning($"Invalid infected count input: '{inputValue}'. Resetting to {currentInfectedCount}");
            UpdateInfectedInputFieldText(currentInfectedCount.ToString());
            UpdateInfectionFeedbackText($"Invalid input! Enter a number between 0 and {currentPopulation}", Color.red);
        }
    }

    private void ResetSimulationWithNewValues(int newPopulation, int newInfectedCount)
    {
        Debug.Log($"Resetting simulation with Population: {newPopulation}, Infected: {newInfectedCount}");

        // Update defaults before reset to preserve user's settings
        defaultPopulation = newPopulation;
        defaultInfectedCount = newInfectedCount;

        // Temporarily prevent UI reset manager from overriding our new population
        bool originalResetSetting = false;
        if (uiResetManager != null)
        {
            originalResetSetting = uiResetManager.resetToDefaultPopulation;
            uiResetManager.resetToDefaultPopulation = false;
        }

        // Reset all UI elements (clock, statistics, graphs, etc.)
        if (uiResetManager != null)
        {
            uiResetManager.ResetAllUI();
        }

        // Restore original reset setting for future use
        if (uiResetManager != null)
        {
            uiResetManager.resetToDefaultPopulation = originalResetSetting;
        }

        // Reset all NPCs to their starting positions and movement states
        if (resetManager != null)
        {
            resetManager.ResetNPCPositions();
        }

        // Clear all virus states (set everyone to healthy)
        VirusSimulation.ResetAllNPCs();

        // Apply the new population and infection settings
        SetPopulation(newPopulation);
        SetInfectedCount(newInfectedCount);

        // Update UI displays to reflect the new values
        UpdatePopulationInputFieldText(newPopulation.ToString());
        UpdateInfectedInputFieldText(newInfectedCount.ToString());
    }

    private void SetPopulation(int targetPopulation)
    {
        if (allNPCs == null || allNPCs.Length == 0)
        {
            UpdateFeedbackText("No NPCs found in scene!", Color.red);
            return;
        }

        // Limit population to available NPCs in the scene
        int actualPopulation = Mathf.Min(targetPopulation, allNPCs.Length);

        // Activate/deactivate NPCs to match desired population size
        for (int i = 0; i < allNPCs.Length; i++)
        {
            if (allNPCs[i] != null)
            {
                bool shouldBeActive = i < actualPopulation;

                // Properly stop NavMeshAgent before deactivating to prevent errors
                if (!shouldBeActive && allNPCs[i].activeInHierarchy)
                {
                    UnityEngine.AI.NavMeshAgent agent = allNPCs[i].GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null && agent.enabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                    }
                }

                bool wasActive = allNPCs[i].activeInHierarchy;
                allNPCs[i].SetActive(shouldBeActive);

                // Reinitialize newly activated NPCs to prevent movement issues
                if (shouldBeActive && !wasActive)
                {
                    StartCoroutine(ReinitializeNPC(allNPCs[i]));
                }
            }
        }

        currentPopulation = actualPopulation;

        // Provide feedback about the population change
        string message = $"Population: {actualPopulation} NPCs active";
        if (actualPopulation != targetPopulation)
        {
            message += $" (requested {targetPopulation}, limited by available NPCs)";
        }

        UpdateFeedbackText(message, Color.white);
        Debug.Log(message);
    }

    private void SetInfectedCount(int targetInfected)
    {
        currentInfectedCount = targetInfected;

        // Build list of currently active NPCs for infection assignment
        List<GameObject> activeNPCs = new List<GameObject>();
        foreach (GameObject npc in allNPCs)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                activeNPCs.Add(npc);
            }
        }

        // Start with a clean slate - reset all active NPCs to healthy
        foreach (GameObject npc in activeNPCs)
        {
            VirusSimulation virusScript = npc.GetComponent<VirusSimulation>();
            if (virusScript != null)
            {
                virusScript.ResetToHealthy();
            }
        }

        // Track infected NPCs for reinitialisation
        List<GameObject> infectedNPCs = new List<GameObject>();

        // Apply infections to the specified number of NPCs
        if (targetInfected > 0 && activeNPCs.Count > 0)
        {
            if (randomizeInfectedSelection)
            {
                // Shuffle the NPC list to randomize infection distribution
                List<GameObject> shuffledNPCs = new List<GameObject>(activeNPCs);
                for (int i = 0; i < shuffledNPCs.Count; i++)
                {
                    GameObject temp = shuffledNPCs[i];
                    int randomIndex = Random.Range(i, shuffledNPCs.Count);
                    shuffledNPCs[i] = shuffledNPCs[randomIndex];
                    shuffledNPCs[randomIndex] = temp;
                }

                // Infect the first N NPCs from the shuffled list
                for (int i = 0; i < Mathf.Min(targetInfected, shuffledNPCs.Count); i++)
                {
                    VirusSimulation virusScript = shuffledNPCs[i].GetComponent<VirusSimulation>();
                    if (virusScript != null)
                    {
                        virusScript.ForceInfection();
                        infectedNPCs.Add(shuffledNPCs[i]);
                    }
                }
            }
            else
            {
                // Infect NPCs in order (first N active NPCs)
                for (int i = 0; i < Mathf.Min(targetInfected, activeNPCs.Count); i++)
                {
                    VirusSimulation virusScript = activeNPCs[i].GetComponent<VirusSimulation>();
                    if (virusScript != null)
                    {
                        virusScript.ForceInfection();
                        infectedNPCs.Add(activeNPCs[i]);
                    }
                }
            }
        }

        // Reinitialise all NPCs to prevent movement issues after state changes
        foreach (GameObject npc in activeNPCs)
        {
            StartCoroutine(ReinitializeNPC(npc));
        }

        // Update UI feedback to show infection distribution
        UpdateInfectionFeedbackText($"Infected NPCs: {targetInfected}/{currentPopulation}", Color.green);
        Debug.Log($"Set {targetInfected} NPCs as infected");

        // Refresh UI statistics after a frame delay
        if (uiResetManager != null)
        {
            StartCoroutine(DelayedUIUpdate());
        }
    }

    private IEnumerator DelayedUIUpdate()
    {
        // Wait for virus states to update before refreshing UI statistics
        yield return null;
        if (uiResetManager != null)
        {
            uiResetManager.ResetStatistics();
        }
    }

    private void ResetAllNPCsToHealthy()
    {
        // Clear all virus states across active NPCs
        foreach (GameObject npc in allNPCs)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                VirusSimulation virusScript = npc.GetComponent<VirusSimulation>();
                if (virusScript != null)
                {
                    virusScript.ResetToHealthy();
                }
            }
        }
    }

    private IEnumerator ReinitializeNPC(GameObject npc)
    {
        // Wait for GameObject activation to complete
        yield return null;

        // Force NPCs to stay at ground level to prevent floating
        float groundY = -2.7f;

        // Reset NavMeshAgent to prevent pathfinding issues
        UnityEngine.AI.NavMeshAgent agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            // Ensure NPC position is at ground level
            Vector3 desiredPos = npc.transform.position;
            desiredPos.y = groundY;

            // Find valid NavMesh position near desired location
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(desiredPos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Force Y position to ground level and teleport NPC
                Vector3 finalPos = hit.position;
                finalPos.y = groundY;

                agent.Warp(finalPos);
                agent.isStopped = false;
            }
            else
            {
                Debug.LogWarning($"{npc.name} couldn't find NavMesh position nearby!");
            }
        }

        // Restart movement scripts to clear any stuck states
        var moveToObjects = npc.GetComponent<PlayerMoveToObjects>();
        var moveRandomly = npc.GetComponent<PlayerMoveRandomly>();

        if (moveToObjects != null && moveToObjects.enabled)
        {
            // Stop all movement coroutines and restart the component
            moveToObjects.StopAllCoroutines();
            moveToObjects.enabled = false;
            yield return null;
            moveToObjects.enabled = true;
        }

        if (moveRandomly != null && moveRandomly.enabled)
        {
            // Stop all movement coroutines and restart the component
            moveRandomly.StopAllCoroutines();
            moveRandomly.enabled = false;
            yield return null;
            moveRandomly.enabled = true;
        }
    }

    // Helper methods to update UI input fields safely
    private void UpdatePopulationInputFieldText(string text)
    {
        if (populationInputField != null)
            populationInputField.text = text;
        else if (legacyInputField != null)
            legacyInputField.text = text;
    }

    private void UpdateInfectedInputFieldText(string text)
    {
        if (infectedCountInputField != null)
            infectedCountInputField.text = text;
    }

    private void UpdateFeedbackText(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }

    private void UpdateInfectionFeedbackText(string message, Color color)
    {
        if (infectionFeedbackText != null)
        {
            infectionFeedbackText.text = message;
            infectionFeedbackText.color = color;
        }
    }

    // Public methods for quick population adjustments
    public void SetPopulationToMin()
    {
        UpdatePopulationInputFieldText(minPopulation.ToString());
        ResetSimulationWithNewValues(minPopulation, 1);
    }

    public void SetPopulationToMax()
    {
        int maxPossible = Mathf.Min(maxPopulation, allNPCs.Length);
        UpdatePopulationInputFieldText(maxPossible.ToString());
        ResetSimulationWithNewValues(maxPossible, 1);
    }

    public void ResetToDefault()
    {
        // Restore simulation to initial default settings
        UpdatePopulationInputFieldText(defaultPopulation.ToString());
        SetPopulation(defaultPopulation);
        ResetAllNPCsToHealthy();
        SetInfectedCount(defaultInfectedCount);
    }

    // Getter methods for external scripts
    public int GetCurrentPopulation()
    {
        return currentPopulation;
    }

    public int GetCurrentInfectedCount()
    {
        return currentInfectedCount;
    }

    // Quick infection preset buttons
    public void SetAllHealthy()
    {
        UpdateInfectedInputFieldText("0");
        ResetSimulationWithNewValues(currentPopulation, 0);
    }

    public void SetHalfInfected()
    {
        int halfPop = currentPopulation / 2;
        UpdateInfectedInputFieldText(halfPop.ToString());
        ResetSimulationWithNewValues(currentPopulation, halfPop);
    }

    public void SetAllInfected()
    {
        UpdateInfectedInputFieldText(currentPopulation.ToString());
        ResetSimulationWithNewValues(currentPopulation, currentPopulation);
    }

    // Debug and maintenance methods
    [ContextMenu("Log NPC Distribution")]
    public void LogNPCDistribution()
    {
        // Analyze current NPC states for debugging
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

    [ContextMenu("Fix Stuck NPCs")]
    public void FixStuckNPCs()
    {
        // Repair NPCs that aren't moving properly
        GameObject[] npcs = GameObject.FindGameObjectsWithTag(npcTag);
        int fixedCount = 0;

        foreach (GameObject npc in npcs)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                UnityEngine.AI.NavMeshAgent agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && agent.enabled && !agent.hasPath && !agent.pathPending)
                {
                    StartCoroutine(ReinitializeNPC(npc));
                    fixedCount++;
                }
            }
        }

        Debug.Log($"Attempted to fix {fixedCount} stuck NPCs");
    }
}