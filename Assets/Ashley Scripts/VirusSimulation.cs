using UnityEngine;
using System.Collections.Generic;

public class VirusSimulation : MonoBehaviour
{
    [Header("Infection Settings")]
    public float infectionRadius = 2f;
    public float checkInterval = 0.1f; // Check every 0.1 seconds instead of every frame
    public bool startInfected = false; // Check this for patient zero

    [Header("Infection Duration")]
    [Range(1f, 200f)]
    public float infectionDuration = 5f; // How long NPC stays infected
    public bool immuneAfterRecovery = true; // Can't be reinfected after recovery

    [Header("Recovery Duration")]
    [Range(1f, 200f)]
    public float recoveredDuration = 10f; // How long NPC stays in recovered state before returning to healthy
    public bool enableRecoveredDuration = true; // If true, NPCs return to healthy color after recovered duration

    [Header("Colour Settings")]
    public Color healthyColor = Color.white;
    public Color infectedColor = Color.red;
    public Color recoveredColor = Color.green;
    public bool resetToOriginalColor = false; // If true, use original color instead of healthyColor

    // Static tracking for all NPCs
    private static List<VirusSimulation> allNPCs = new List<VirusSimulation>();

    // NEW: Static tracking for infection durations (for results screen)
    private static List<float> allInfectionDurations = new List<float>();

    // Instance variables
    private Renderer npcRenderer;
    private Color originalColor;
    private NPCState currentState = NPCState.Healthy;
    private float infectionStartTime;
    private float recoveryStartTime; // Track when recovery started
    private float nextCheckTime = 0f;

    public enum NPCState
    {
        Healthy,
        Infected,
        Recovered
    }

    void Start()
    {
        npcRenderer = GetComponent<Renderer>();

        // Store original color
        originalColor = npcRenderer.material.color;

        // Set initial color
        if (!resetToOriginalColor)
        {
            npcRenderer.material.color = healthyColor;
        }

        // Check if this NPC should start infected (only if active)
        if (startInfected && gameObject.activeInHierarchy)
        {
            InfectNPC();
        }
    }

    void OnEnable()
    {
        // Register this NPC when activated
        if (!allNPCs.Contains(this))
        {
            allNPCs.Add(this);
            Debug.Log($"{gameObject.name} added to simulation (Total active: {allNPCs.Count})");
        }
    }

    void OnDisable()
    {
        // Unregister when deactivated to prevent ghost infections
        allNPCs.Remove(this);
        Debug.Log($"{gameObject.name} removed from simulation (Total active: {allNPCs.Count})");
    }

    void OnDestroy()
    {
        // Unregister when destroyed to prevent memory leaks
        allNPCs.Remove(this);
    }

    void Update()
    {
        // Only process if this GameObject is active
        if (!gameObject.activeInHierarchy) return;

        // Check for infection spreading (only if this NPC is infected)
        if (Time.time >= nextCheckTime && currentState == NPCState.Infected)
        {
            nextCheckTime = Time.time + checkInterval;
            CheckForInfectionSpread();
        }

        // Check for recovery (only if infected)
        if (currentState == NPCState.Infected)
        {
            CheckForRecovery();
        }

        // Check for return to healthy (only if recovered and feature enabled)
        if (currentState == NPCState.Recovered && enableRecoveredDuration)
        {
            CheckForReturnToHealthy();
        }
    }

    void CheckForInfectionSpread()
    {
        // If this NPC is infected, check if it can infect nearby NPCs
        foreach (VirusSimulation otherNPC in allNPCs)
        {
            // Skip self, inactive NPCs, and NPCs that can't be infected
            if (otherNPC == this || !otherNPC.gameObject.activeInHierarchy || !CanBeInfected(otherNPC))
                continue;

            float distance = Vector3.Distance(transform.position, otherNPC.transform.position);
            if (distance < infectionRadius)
            {
                otherNPC.InfectNPC();
            }
        }
    }

    void CheckForRecovery()
    {
        // Check if enough time has passed for recovery
        if (Time.time >= infectionStartTime + infectionDuration)
        {
            RecoverNPC();
        }
    }

    void CheckForReturnToHealthy()
    {
        // Check if enough time has passed to return to healthy state
        if (Time.time >= recoveryStartTime + recoveredDuration)
        {
            ReturnToHealthy();
        }
    }

    bool CanBeInfected(VirusSimulation npc)
    {
        // Can't infect inactive NPCs
        if (!npc.gameObject.activeInHierarchy) return false;

        // Can only infect healthy NPCs, or recovered NPCs if immunity is disabled
        if (npc.currentState == NPCState.Healthy)
            return true;

        if (npc.currentState == NPCState.Recovered && !immuneAfterRecovery)
            return true;

        return false;
    }

    void InfectNPC()
    {
        if (currentState != NPCState.Infected)
        {
            currentState = NPCState.Infected;
            infectionStartTime = Time.time;

            // Add null check to prevent crash during scene reload
            if (npcRenderer != null)
            {
                npcRenderer.material.color = infectedColor;
            }

            Debug.Log($"{gameObject.name} has been infected!");
        }
    }

    void RecoverNPC()
    {
        if (currentState == NPCState.Infected)
        {
            currentState = NPCState.Recovered;
            recoveryStartTime = Time.time; // Track when recovery started

            // NEW: Track infection duration for results screen
            float infectionDurationActual = Time.time - infectionStartTime;
            allInfectionDurations.Add(infectionDurationActual);

            // Add null check to prevent crash during scene reload
            if (npcRenderer != null)
            {
                // Set recovered color
                if (resetToOriginalColor)
                {
                    npcRenderer.material.color = originalColor;
                }
                else
                {
                    npcRenderer.material.color = recoveredColor;
                }
            }

            Debug.Log($"{gameObject.name} has recovered!");
        }
    }

    void ReturnToHealthy()
    {
        if (currentState == NPCState.Recovered)
        {
            currentState = NPCState.Healthy;

            // Add null check to prevent crash during scene reload
            if (npcRenderer != null)
            {
                // Return to healthy color
                if (resetToOriginalColor)
                {
                    npcRenderer.material.color = originalColor;
                }
                else
                {
                    npcRenderer.material.color = healthyColor;
                }
            }

            Debug.Log($"{gameObject.name} has returned to healthy state!");
        }
    }

    // Public method to infect this NPC from external sources
    public void ForceInfection()
    {
        if (CanBeInfected(this))
        {
            InfectNPC();
        }
    }

    // Public method to force recovery (for testing)
    public void ForceRecovery()
    {
        if (currentState == NPCState.Infected)
        {
            RecoverNPC();
        }
    }

    // Public method to force return to healthy (for testing)
    public void ForceReturnToHealthy()
    {
        if (currentState == NPCState.Recovered)
        {
            ReturnToHealthy();
        }
    }

    // Public method to reset NPC to healthy state
    public void ResetToHealthy()
    {
        currentState = NPCState.Healthy;

        // Add null check to prevent crash during scene reload
        if (npcRenderer != null)
        {
            if (resetToOriginalColor)
            {
                npcRenderer.material.color = originalColor;
            }
            else
            {
                npcRenderer.material.color = healthyColor;
            }
        }
    }

    // Getters for external scripts (like stats display)
    public NPCState GetCurrentState() => currentState;
    public bool IsInfected() => currentState == NPCState.Infected;
    public bool IsRecovered() => currentState == NPCState.Recovered;
    public bool IsHealthy() => currentState == NPCState.Healthy;

    public float GetInfectionProgress()
    {
        if (currentState != NPCState.Infected) return 0f;

        float timeInfected = Time.time - infectionStartTime;
        return Mathf.Clamp01(timeInfected / infectionDuration);
    }

    public float GetTimeUntilRecovery()
    {
        if (currentState != NPCState.Infected) return 0f;

        float timeRemaining = (infectionStartTime + infectionDuration) - Time.time;
        return Mathf.Max(0f, timeRemaining);
    }

    // Methods for recovery duration tracking
    public float GetRecoveryProgress()
    {
        if (currentState != NPCState.Recovered) return 0f;

        float timeRecovered = Time.time - recoveryStartTime;
        return Mathf.Clamp01(timeRecovered / recoveredDuration);
    }

    public float GetTimeUntilReturnToHealthy()
    {
        if (currentState != NPCState.Recovered) return 0f;

        float timeRemaining = (recoveryStartTime + recoveredDuration) - Time.time;
        return Mathf.Max(0f, timeRemaining);
    }

    // Static methods for getting overall statistics (only count active NPCs)
    public static int GetHealthyCount()
    {
        int count = 0;
        foreach (var npc in allNPCs)
        {
            if (npc.gameObject.activeInHierarchy && npc.IsHealthy()) count++;
        }
        return count;
    }

    public static int GetInfectedCount()
    {
        int count = 0;
        foreach (var npc in allNPCs)
        {
            if (npc.gameObject.activeInHierarchy && npc.IsInfected()) count++;
        }
        return count;
    }

    public static int GetRecoveredCount()
    {
        int count = 0;
        foreach (var npc in allNPCs)
        {
            if (npc.gameObject.activeInHierarchy && npc.IsRecovered()) count++;
        }
        return count;
    }

    public static int GetTotalNPCCount()
    {
        // Only count active NPCs
        int count = 0;
        foreach (var npc in allNPCs)
        {
            if (npc.gameObject.activeInHierarchy) count++;
        }
        return count;
    }

    public static List<VirusSimulation> GetAllNPCs()
    {
        return new List<VirusSimulation>(allNPCs); // Return copy to prevent external modification
    }

    // Get only active NPCs
    public static List<VirusSimulation> GetActiveNPCs()
    {
        List<VirusSimulation> activeNPCs = new List<VirusSimulation>();
        foreach (var npc in allNPCs)
        {
            if (npc.gameObject.activeInHierarchy)
            {
                activeNPCs.Add(npc);
            }
        }
        return activeNPCs;
    }

    // Reset all NPCs to healthy (useful for simulation reset)
    public static void ResetAllNPCs()
    {
        foreach (var npc in allNPCs)
        {
            npc.ResetToHealthy();
        }
        Debug.Log("All NPCs reset to healthy state");
    }

    // Reset only active NPCs to healthy
    public static void ResetActiveNPCs()
    {
        int resetCount = 0;
        foreach (var npc in allNPCs)
        {
            if (npc.gameObject.activeInHierarchy)
            {
                npc.ResetToHealthy();
                resetCount++;
            }
        }
        Debug.Log($"{resetCount} active NPCs reset to healthy state");
    }

    // NEW: Static methods for infection duration tracking (for results screen)
    public static float GetAverageInfectionDuration()
    {
        if (allInfectionDurations.Count == 0) return 0f;

        float sum = 0f;
        foreach (float duration in allInfectionDurations)
        {
            sum += duration;
        }
        return sum / allInfectionDurations.Count;
    }

    public static void ResetInfectionDurations()
    {
        allInfectionDurations.Clear();
    }

    public static int GetTotalRecoveredCount()
    {
        return allInfectionDurations.Count;
    }

    // Manual infection controls (for testing/debugging)
    [ContextMenu("Infect This NPC")]
    void InfectThisNPC()
    {
        ForceInfection();
    }

    [ContextMenu("Recover This NPC")]
    void RecoverThisNPC()
    {
        ForceRecovery();
    }

    [ContextMenu("Force Return to Healthy")]
    void ForceReturnToHealthyState()
    {
        ForceReturnToHealthy();
    }

    [ContextMenu("Reset This NPC")]
    void ResetThisNPC()
    {
        ResetToHealthy();
    }
}