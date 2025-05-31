using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NPCResetManager : MonoBehaviour
{
    [Header("Reset Settings")]
    [Tooltip("If true, NPCs will return to their original spawn positions")]
    public bool useOriginalPositions = true;

    [Tooltip("If useOriginalPositions is false, NPCs will be placed at these spawn points")]
    public Transform[] spawnPoints;

    [Tooltip("If using spawn points, should NPCs be randomly distributed?")]
    public bool randomizeSpawnPoints = true;

    [Header("UI References")]
    [Tooltip("Optional: Button to trigger reset (will auto-assign if not set)")]
    public Button resetButton;

    [Header("Debug")]
    public bool debugMode = false;

    // Dictionary to store original positions of NPCs
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> originalRotations = new Dictionary<GameObject, Quaternion>();

    void Start()
    {
        // Store original positions of all NPCs
        StoreOriginalPositions();

        // Auto-assign button if not set and this script is on a button
        if (resetButton == null)
        {
            resetButton = GetComponent<Button>();
        }

        // Subscribe to button click event
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetAllNPCs);
            if (debugMode)
                Debug.Log("Reset button assigned successfully");
        }
        else if (debugMode)
        {
            Debug.LogWarning("No reset button assigned. You can still call ResetAllNPCs() manually.");
        }
    }

    void StoreOriginalPositions()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");

        foreach (GameObject npc in npcs)
        {
            originalPositions[npc] = npc.transform.position;
            originalRotations[npc] = npc.transform.rotation;
        }

        if (debugMode)
            Debug.Log($"Stored original positions for {npcs.Length} NPCs");
    }

    /// <summary>
    /// Resets all NPCs to their original positions and healthy virus state
    /// </summary>
    public void ResetAllNPCs()
    {
        ResetNPCPositions();
        ResetVirusStates();

        if (debugMode)
            Debug.Log("All NPCs have been reset!");
    }

    /// <summary>
    /// Resets only the positions of all NPCs
    /// </summary>
    public void ResetNPCPositions()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");

        if (useOriginalPositions)
        {
            // Reset to original positions
            foreach (GameObject npc in npcs)
            {
                if (originalPositions.ContainsKey(npc))
                {
                    npc.transform.position = originalPositions[npc];
                    npc.transform.rotation = originalRotations[npc];
                }
                else
                {
                    // If original position not stored, store current as original
                    originalPositions[npc] = npc.transform.position;
                    originalRotations[npc] = npc.transform.rotation;
                }
            }
        }
        else
        {
            // Use spawn points
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                if (randomizeSpawnPoints)
                {
                    // Randomly distribute NPCs among spawn points
                    for (int i = 0; i < npcs.Length; i++)
                    {
                        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                        npcs[i].transform.position = spawnPoint.position;
                        npcs[i].transform.rotation = spawnPoint.rotation;
                    }
                }
                else
                {
                    // Distribute NPCs evenly among spawn points
                    for (int i = 0; i < npcs.Length; i++)
                    {
                        Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
                        npcs[i].transform.position = spawnPoint.position;
                        npcs[i].transform.rotation = spawnPoint.rotation;
                    }
                }
            }
            else
            {
                Debug.LogWarning("No spawn points assigned! NPCs will not be repositioned.");
            }
        }

        if (debugMode)
            Debug.Log($"Reset positions for {npcs.Length} NPCs");
    }

    /// <summary>
    /// Resets only the virus states of all NPCs, preserving patient zero settings
    /// </summary>
    public void ResetVirusStates()
    {
        // First, reset all NPCs to healthy
        VirusSimulation.ResetAllNPCs();

        // Then re-infect NPCs that are marked as patient zero
        RestorePatientZeros();

        if (debugMode)
            Debug.Log("All virus states reset to healthy, patient zeros restored");
    }

    /// <summary>
    /// Re-infects NPCs that have startInfected = true (patient zeros)
    /// </summary>
    private void RestorePatientZeros()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        int patientZeroCount = 0;

        foreach (GameObject npc in npcs)
        {
            VirusSimulation virusScript = npc.GetComponent<VirusSimulation>();
            if (virusScript != null && virusScript.startInfected)
            {
                virusScript.ForceInfection();
                patientZeroCount++;

                if (debugMode)
                    Debug.Log($"Restored patient zero: {npc.name}");
            }
        }

        if (debugMode && patientZeroCount > 0)
            Debug.Log($"Restored {patientZeroCount} patient zero(s)");
    }

    /// <summary>
    /// Manually infects a random NPC (useful for testing)
    /// </summary>
    [ContextMenu("Infect Random NPC")]
    public void InfectRandomNPC()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");

        if (npcs.Length > 0)
        {
            GameObject randomNPC = npcs[Random.Range(0, npcs.Length)];
            VirusSimulation virusScript = randomNPC.GetComponent<VirusSimulation>();

            if (virusScript != null)
            {
                virusScript.ForceInfection();
                if (debugMode)
                    Debug.Log($"Infected {randomNPC.name}");
            }
        }
    }

    /// <summary>
    /// Update original positions if NPCs have been moved manually
    /// </summary>
    [ContextMenu("Update Original Positions")]
    public void UpdateOriginalPositions()
    {
        StoreOriginalPositions();
        if (debugMode)
            Debug.Log("Original positions updated");
    }

    /// <summary>
    /// Get statistics about current NPC states
    /// </summary>
    public void LogNPCStatistics()
    {
        if (debugMode)
        {
            int patientZeroCount = GetPatientZeroCount();
            Debug.Log($"NPC Statistics:");
            Debug.Log($"- Total NPCs: {VirusSimulation.GetTotalNPCCount()}");
            Debug.Log($"- Healthy: {VirusSimulation.GetHealthyCount()}");
            Debug.Log($"- Infected: {VirusSimulation.GetInfectedCount()}");
            Debug.Log($"- Recovered: {VirusSimulation.GetRecoveredCount()}");
            Debug.Log($"- Patient Zeros: {patientZeroCount}");
        }
    }

    /// <summary>
    /// Get count of NPCs marked as patient zero
    /// </summary>
    public int GetPatientZeroCount()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        int count = 0;

        foreach (GameObject npc in npcs)
        {
            VirusSimulation virusScript = npc.GetComponent<VirusSimulation>();
            if (virusScript != null && virusScript.startInfected)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Manually set/unset an NPC as patient zero
    /// </summary>
    public void SetPatientZero(GameObject npc, bool isPatientZero)
    {
        VirusSimulation virusScript = npc.GetComponent<VirusSimulation>();
        if (virusScript != null)
        {
            virusScript.startInfected = isPatientZero;

            if (debugMode)
                Debug.Log($"{npc.name} patient zero status set to: {isPatientZero}");
        }
    }

    void OnDestroy()
    {
        // Clean up button listener
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(ResetAllNPCs);
        }
    }

    // Public methods that can be called from other scripts or UI events
    public int GetNPCCount()
    {
        return GameObject.FindGameObjectsWithTag("NPC").Length;
    }

    public bool HasNPCs()
    {
        return GetNPCCount() > 0;
    }
}