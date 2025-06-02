using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NPCPopulationController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField populationInputField; // TextMeshPro InputField
    public InputField legacyInputField;         // Legacy InputField (as backup)
    public Text feedbackText;                   // Shows current population status

    [Header("Infection Control UI")]
    public TMP_InputField infectedCountInputField; // Input for number of infected NPCs
    public Text infectionFeedbackText;              // Shows infection status

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

    [Header("Infection Settings")]
    [Range(0, 50)]
    public int defaultInfectedCount = 1;
    public bool randomizeInfectedSelection = true; // Randomly select which NPCs to infect

    [Header("Reset Integration")]
    public NPCResetManager resetManager; // Drag your reset manager here
    public UIResetManager uiResetManager; // Drag your UI reset manager here

    private GameObject[] allNPCs;
    private int currentPopulation;
    private int currentInfectedCount;

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

        // Setup population input field
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

        // Setup infected count input field
        if (infectedCountInputField != null)
        {
            infectedCountInputField.text = defaultInfectedCount.ToString();
            infectedCountInputField.onEndEdit.AddListener(OnInfectedCountChanged);
            infectedCountInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        }

        // Set initial population and infected count
        SetPopulation(defaultPopulation);
        SetInfectedCount(defaultInfectedCount);
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
                UpdatePopulationInputFieldText(clampedPopulation.ToString());
                Debug.Log($"Population clamped from {requestedPopulation} to {clampedPopulation}");
            }

            SetPopulation(clampedPopulation);

            // Reset all NPCs to healthy first
            ResetAllNPCsToHealthy();

            // Set infected count to 1 (or maintain current if valid)
            int newInfectedCount = Mathf.Min(currentInfectedCount, clampedPopulation);
            if (newInfectedCount < 1) newInfectedCount = 1;

            UpdateInfectedInputFieldText(newInfectedCount.ToString());
            SetInfectedCount(newInfectedCount);

            // Update UI displays
            if (uiResetManager != null)
            {
                uiResetManager.ResetStatistics();
            }
        }
        else
        {
            // Invalid input - reset to current population
            Debug.LogWarning($"Invalid input: '{inputValue}'. Resetting to {currentPopulation}");
            UpdatePopulationInputFieldText(currentPopulation.ToString());
            UpdateFeedbackText($"Invalid input! Enter a number between {minPopulation} and {maxPopulation}", Color.red);
        }
    }

    public void OnInfectedCountChanged(string inputValue)
    {
        if (int.TryParse(inputValue, out int requestedInfected))
        {
            // Clamp to valid range (0 to current population)
            int clampedInfected = Mathf.Clamp(requestedInfected, 0, currentPopulation);

            // Update input field if it was clamped
            if (clampedInfected != requestedInfected)
            {
                UpdateInfectedInputFieldText(clampedInfected.ToString());
                Debug.Log($"Infected count clamped from {requestedInfected} to {clampedInfected}");
            }

            SetInfectedCount(clampedInfected);
        }
        else
        {
            // Invalid input - reset to current infected count
            Debug.LogWarning($"Invalid infected count input: '{inputValue}'. Resetting to {currentInfectedCount}");
            UpdateInfectedInputFieldText(currentInfectedCount.ToString());
            UpdateInfectionFeedbackText($"Invalid input! Enter a number between 0 and {currentPopulation}", Color.red);
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

        // Enable/disable NPCs based on population
        for (int i = 0; i < allNPCs.Length; i++)
        {
            if (allNPCs[i] != null)
            {
                bool shouldBeActive = i < actualPopulation;

                if (!shouldBeActive && allNPCs[i].activeInHierarchy)
                {
                    // Properly stop NavMeshAgent before deactivation
                    UnityEngine.AI.NavMeshAgent agent = allNPCs[i].GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null && agent.enabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                    }
                }

                bool wasActive = allNPCs[i].activeInHierarchy;
                allNPCs[i].SetActive(shouldBeActive);

                // Reinitialize newly activated NPCs
                if (shouldBeActive && !wasActive)
                {
                    StartCoroutine(ReinitializeNPC(allNPCs[i]));
                }
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

    private void SetInfectedCount(int targetInfected)
    {
        currentInfectedCount = targetInfected;

        // Get list of active NPCs
        List<GameObject> activeNPCs = new List<GameObject>();
        foreach (GameObject npc in allNPCs)
        {
            if (npc != null && npc.activeInHierarchy)
            {
                activeNPCs.Add(npc);
            }
        }

        // Reset all active NPCs to healthy first
        foreach (GameObject npc in activeNPCs)
        {
            VirusSimulation virusScript = npc.GetComponent<VirusSimulation>();
            if (virusScript != null)
            {
                virusScript.ResetToHealthy();
            }
        }

        // Keep track of which NPCs get infected for reinitialization
        List<GameObject> infectedNPCs = new List<GameObject>();

        // Infect the specified number of NPCs
        if (targetInfected > 0 && activeNPCs.Count > 0)
        {
            if (randomizeInfectedSelection)
            {
                // Randomly select NPCs to infect
                List<GameObject> shuffledNPCs = new List<GameObject>(activeNPCs);
                for (int i = 0; i < shuffledNPCs.Count; i++)
                {
                    GameObject temp = shuffledNPCs[i];
                    int randomIndex = Random.Range(i, shuffledNPCs.Count);
                    shuffledNPCs[i] = shuffledNPCs[randomIndex];
                    shuffledNPCs[randomIndex] = temp;
                }

                // Infect the first N NPCs from shuffled list
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
                // Infect the first N active NPCs
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

        // Reinitialize all NPCs that had their state changed
        // This prevents them from getting stuck after infection state changes
        foreach (GameObject npc in activeNPCs)
        {
            StartCoroutine(ReinitializeNPC(npc));
        }

        // Update infection feedback
        UpdateInfectionFeedbackText($"Infected NPCs: {targetInfected}/{currentPopulation}", Color.green);
        Debug.Log($"Set {targetInfected} NPCs as infected");

        // Update UI displays
        if (uiResetManager != null)
        {
            StartCoroutine(DelayedUIUpdate());
        }
    }

    private IEnumerator DelayedUIUpdate()
    {
        yield return null; // Wait a frame for virus states to update
        if (uiResetManager != null)
        {
            uiResetManager.ResetStatistics();
        }
    }

    private void ResetAllNPCsToHealthy()
    {
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
        // Wait for the GameObject to fully activate
        yield return null;

        // Store the desired Y position (ground level)
        float groundY = -2.7f; // Your ground level

        // Reinitialize NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            // Get current position but force Y to ground level
            Vector3 desiredPos = npc.transform.position;
            desiredPos.y = groundY;

            // Ensure NPC is on NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(desiredPos, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Force the Y position to stay at ground level
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

        // Reinitialize movement scripts
        var moveToObjects = npc.GetComponent<PlayerMoveToObjects>();
        var moveRandomly = npc.GetComponent<PlayerMoveRandomly>();

        if (moveToObjects != null && moveToObjects.enabled)
        {
            moveToObjects.StopAllCoroutines();
            moveToObjects.enabled = false;
            yield return null;
            moveToObjects.enabled = true;
        }

        if (moveRandomly != null && moveRandomly.enabled)
        {
            moveRandomly.StopAllCoroutines();
            moveRandomly.enabled = false;
            yield return null;
            moveRandomly.enabled = true;
        }
    }

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

    // Public methods for other scripts to call
    public void SetPopulationToMin()
    {
        UpdatePopulationInputFieldText(minPopulation.ToString());
        SetPopulation(minPopulation);
        ResetAllNPCsToHealthy();
        SetInfectedCount(1);
    }

    public void SetPopulationToMax()
    {
        int maxPossible = Mathf.Min(maxPopulation, allNPCs.Length);
        UpdatePopulationInputFieldText(maxPossible.ToString());
        SetPopulation(maxPossible);
        ResetAllNPCsToHealthy();
        SetInfectedCount(1);
    }

    public void ResetToDefault()
    {
        UpdatePopulationInputFieldText(defaultPopulation.ToString());
        SetPopulation(defaultPopulation);
        ResetAllNPCsToHealthy();
        SetInfectedCount(defaultInfectedCount);
    }

    // Getter methods
    public int GetCurrentPopulation()
    {
        return currentPopulation;
    }

    public int GetCurrentInfectedCount()
    {
        return currentInfectedCount;
    }

    // Quick infection presets
    public void SetAllHealthy()
    {
        UpdateInfectedInputFieldText("0");
        SetInfectedCount(0);
    }

    public void SetHalfInfected()
    {
        int halfPop = currentPopulation / 2;
        UpdateInfectedInputFieldText(halfPop.ToString());
        SetInfectedCount(halfPop);
    }

    public void SetAllInfected()
    {
        UpdateInfectedInputFieldText(currentPopulation.ToString());
        SetInfectedCount(currentPopulation);
    }

    // Debug methods
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

    [ContextMenu("Fix Stuck NPCs")]
    public void FixStuckNPCs()
    {
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