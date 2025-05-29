using UnityEngine;
using System.Collections.Generic;

public class VirusSimulation : MonoBehaviour
{
    [Header("Infection Settings")]
    public float infectionRadius = 2f;
    public float checkInterval = 0.1f; // Check every 0.1 seconds instead of every frame
    public bool startInfected = false; // Check this for patient zero

    [Header("Recovery Settings")]
    [Range(1f, 200f)]
    public float infectionDuration = 5f; // How long NPC stays infected
    public bool immuneAfterRecovery = true; // Can't be reinfected after recovery

    [Header("Visual Settings")]
    public Color healthyColor = Color.white;
    public Color infectedColor = Color.red;
    public Color recoveredColor = Color.green;
    public bool resetToOriginalColor = false; // If true, use original color instead of healthyColor

    // Static tracking for all NPCs
    private static List<VirusSimulation> allNPCs = new List<VirusSimulation>();

    // Instance variables
    private Renderer npcRenderer;
    private Color originalColor;
    private NPCState currentState = NPCState.Healthy;
    private float infectionStartTime;
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

        // Register this NPC
        allNPCs.Add(this);

        // Set initial color
        if (!resetToOriginalColor)
        {
            npcRenderer.material.color = healthyColor;
        }

        // Check if this NPC should start infected
        if (startInfected)
        {
            InfectNPC();
        }
    }

    void OnDestroy()
    {
        // Unregister when destroyed to prevent memory leaks
        allNPCs.Remove(this);
    }

    void Update()
    {
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
    }

    void CheckForInfectionSpread()
    {
        // If this NPC is infected, check if it can infect nearby NPCs
        foreach (VirusSimulation otherNPC in allNPCs)
        {
            if (otherNPC == this || !CanBeInfected(otherNPC))
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

    bool CanBeInfected(VirusSimulation npc)
    {
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
            npcRenderer.material.color = infectedColor;

            Debug.Log($"{gameObject.name} has been infected!");
        }
    }

    void RecoverNPC()
    {
        if (currentState == NPCState.Infected)
        {
            currentState = NPCState.Recovered;

            // Set recovered color
            if (resetToOriginalColor)
            {
                npcRenderer.material.color = originalColor;
            }
            else
            {
                npcRenderer.material.color = recoveredColor;
            }

            Debug.Log($"{gameObject.name} has recovered!");
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

    // Public method to reset NPC to healthy state
    public void ResetToHealthy()
    {
        currentState = NPCState.Healthy;

        if (resetToOriginalColor)
        {
            npcRenderer.material.color = originalColor;
        }
        else
        {
            npcRenderer.material.color = healthyColor;
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

    // Static methods for getting overall statistics
    public static int GetHealthyCount()
    {
        int count = 0;
        foreach (var npc in allNPCs)
        {
            if (npc.IsHealthy()) count++;
        }
        return count;
    }

    public static int GetInfectedCount()
    {
        int count = 0;
        foreach (var npc in allNPCs)
        {
            if (npc.IsInfected()) count++;
        }
        return count;
    }

    public static int GetRecoveredCount()
    {
        int count = 0;
        foreach (var npc in allNPCs)
        {
            if (npc.IsRecovered()) count++;
        }
        return count;
    }

    public static int GetTotalNPCCount()
    {
        return allNPCs.Count;
    }

    public static List<VirusSimulation> GetAllNPCs()
    {
        return new List<VirusSimulation>(allNPCs); // Return copy to prevent external modification
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

    [ContextMenu("Reset This NPC")]
    void ResetThisNPC()
    {
        ResetToHealthy();
    }
}