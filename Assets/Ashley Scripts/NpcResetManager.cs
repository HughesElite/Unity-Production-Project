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

    // Store initial positions and rotations for restoration
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> originalRotations = new Dictionary<GameObject, Quaternion>();

    void Start()
    {
        // Capture the starting positions of all NPCs for later restoration
        StoreOriginalPositions();

        // Set up reset button functionality (auto-detect if not manually assigned)
        if (resetButton == null)
        {
            resetButton = GetComponent<Button>();
        }

        // Connect button click to reset functionality
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
        // Record the initial placement of all NPCs in the scene
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
    /// Complete reset - positions and virus states back to initial conditions
    /// </summary>
    public void ResetAllNPCs()
    {
        ResetNPCPositions();
        ResetVirusStates();

        if (debugMode)
            Debug.Log("All NPCs have been reset!");
    }

    /// <summary>
    /// Move all NPCs back to their starting positions (or spawn points)
    /// </summary>
    public void ResetNPCPositions()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");

        if (useOriginalPositions)
        {
            // Restore NPCs to their exact starting locations
            foreach (GameObject npc in npcs)
            {
                if (originalPositions.ContainsKey(npc))
                {
                    npc.transform.position = originalPositions[npc];
                    npc.transform.rotation = originalRotations[npc];
                }
                else
                {
                    // Handle NPCs spawned after initialization
                    originalPositions[npc] = npc.transform.position;
                    originalRotations[npc] = npc.transform.rotation;
                }
            }
        }
        else
        {
            // Distribute NPCs among predefined spawn points
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                if (randomizeSpawnPoints)
                {
                    // Random assignment for varied simulation starts
                    for (int i = 0; i < npcs.Length; i++)
                    {
                        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                        npcs[i].transform.position = spawnPoint.position;
                        npcs[i].transform.rotation = spawnPoint.rotation;
                    }
                }
                else
                {
                    // Even distribution across all spawn points
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
    /// Reset infection states while preserving patient zero configuration
    /// </summary>
    public void ResetVirusStates()
    {
        // Clear all current infections and immunity
        VirusSimulation.ResetAllNPCs();

        // Restore initial infected NPCs (patient zeros) as configured
        RestorePatientZeros();

        if (debugMode)
            Debug.Log("All virus states reset to healthy, patient zeros restored");
    }

    /// <summary>
    /// Re-infect NPCs that are configured as initial carriers (patient zeros)
    /// </summary>
    private void RestorePatientZeros()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        int patientZeroCount = 0;

        // Find and re-infect NPCs marked as starting infected
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
    /// Testing utility - infect a random NPC for quick simulation starts
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
    /// Update stored positions if NPCs have been manually repositioned in editor
    /// </summary>
    [ContextMenu("Update Original Positions")]
    public void UpdateOriginalPositions()
    {
        StoreOriginalPositions();
        if (debugMode)
            Debug.Log("Original positions updated");
    }

    /// <summary>
    /// Debug output of current simulation state
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
    /// Count NPCs configured as initial infection sources
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
    /// Configure an NPC as initial infection source for simulation starts
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
        // Clean up event listeners to prevent memory leaks
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(ResetAllNPCs);
        }
    }

    // Utility methods for external scripts to query NPC state
    public int GetNPCCount()
    {
        return GameObject.FindGameObjectsWithTag("NPC").Length;
    }

    public bool HasNPCs()
    {
        return GetNPCCount() > 0;
    }
}