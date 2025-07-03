using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class NPCUIResetManager : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcTag = "NPC";

    [Header("Reset Options")]
    public bool resetNPCPositions = true;
    public bool resetWorldSpaceUI = true;
    public bool resetNPCMovement = true;
    public bool resetNPCStates = true;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Store original NPC data when scene starts
    private Dictionary<GameObject, NPCResetData> originalNPCData = new Dictionary<GameObject, NPCResetData>();
    private bool dataInitialized = false;
    private GameController gameController;

    private struct NPCResetData
    {
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public bool wasActive;
        public List<Canvas> worldSpaceCanvases;
        public NavMeshAgent navAgent;
        public PlayerMoveToObjects moveScript;
        public PlayerMoveRandomly randomMoveScript;

        public NPCResetData(GameObject npc)
        {
            originalPosition = npc.transform.position;
            originalRotation = npc.transform.rotation;
            wasActive = npc.activeInHierarchy;

            // Find all world-space canvases attached to this NPC
            worldSpaceCanvases = new List<Canvas>();
            Canvas[] canvases = npc.GetComponentsInChildren<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    worldSpaceCanvases.Add(canvas);
                }
            }

            // Get movement components
            navAgent = npc.GetComponent<NavMeshAgent>();
            moveScript = npc.GetComponent<PlayerMoveToObjects>();
            randomMoveScript = npc.GetComponent<PlayerMoveRandomly>();
        }
    }

    void Start()
    {
        // Get reference to GameController
        gameController = GameController.Instance;

        // Wait a frame to ensure all NPCs are properly initialized
        StartCoroutine(InitializeNPCDataDelayed());
    }

    IEnumerator InitializeNPCDataDelayed()
    {
        yield return null; // Wait one frame
        InitializeNPCData();
    }

    void InitializeNPCData()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag(npcTag);

        foreach (GameObject npc in npcs)
        {
            if (npc != null)
            {
                NPCResetData data = new NPCResetData(npc);
                originalNPCData[npc] = data;
            }
        }

        dataInitialized = true;

        if (showDebugLogs)
        {
            Debug.Log($"NPCUIResetManager: Initialized reset data for {originalNPCData.Count} NPCs");
        }
    }

    [ContextMenu("Reset All NPC UI Elements")]
    public void ResetAllNPCUIElements()
    {
        if (!dataInitialized)
        {
            Debug.LogWarning("NPCUIResetManager: Data not initialized yet. Trying to initialize now...");
            InitializeNPCData();
        }

        int resetCount = 0;

        foreach (var kvp in originalNPCData)
        {
            GameObject npc = kvp.Key;
            NPCResetData data = kvp.Value;

            if (npc == null) continue;

            // Reset NPC position
            if (resetNPCPositions)
            {
                ResetNPCPosition(npc, data);
            }

            // Reset world-space UI
            if (resetWorldSpaceUI)
            {
                ResetWorldSpaceUI(npc, data);
            }

            // Reset movement scripts
            if (resetNPCMovement)
            {
                ResetNPCMovement(npc, data);
            }

            // Reset NPC states (virus simulation)
            if (resetNPCStates)
            {
                ResetNPCVirusState(npc);
            }

            resetCount++;
        }

        if (showDebugLogs)
        {
            Debug.Log($"NPCUIResetManager: Reset {resetCount} NPCs and their UI elements");
        }

        // Pause the game after reset is complete
        if (gameController != null)
        {
            gameController.PauseGame();
            if (showDebugLogs)
            {
                Debug.Log("NPCUIResetManager: Game paused after reset");
            }
        }
        else
        {
            // Fallback pause method
            Time.timeScale = 0f;
            if (showDebugLogs)
            {
                Debug.Log("NPCUIResetManager: Game paused after reset (fallback method)");
            }
        }
    }

    void ResetNPCPosition(GameObject npc, NPCResetData data)
    {
        // Stop NavMesh agent if it exists
        if (data.navAgent != null)
        {
            data.navAgent.isStopped = true;
            data.navAgent.ResetPath();
        }

        // Reset position and rotation
        npc.transform.position = data.originalPosition;
        npc.transform.rotation = data.originalRotation;

        // Restart NavMesh agent
        if (data.navAgent != null)
        {
            // Wait a frame then re-enable agent
            StartCoroutine(ReEnableNavAgent(data.navAgent));
        }

        // Reset active state
        npc.SetActive(data.wasActive);
    }

    IEnumerator ReEnableNavAgent(NavMeshAgent agent)
    {
        yield return null; // Wait one frame for position to settle
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    void ResetWorldSpaceUI(GameObject npc, NPCResetData data)
    {
        // Reset all world-space canvases attached to this NPC
        foreach (Canvas canvas in data.worldSpaceCanvases)
        {
            if (canvas != null)
            {
                // Reset canvas position to follow NPC
                canvas.transform.position = npc.transform.position + Vector3.up * 2f; // Adjust height as needed

                // Reset any UI animations or states
                ResetCanvasUIElements(canvas);
            }
        }
    }

    void ResetCanvasUIElements(Canvas canvas)
    {
        // Reset all UI elements in this canvas to default states
        UnityEngine.UI.Slider[] sliders = canvas.GetComponentsInChildren<UnityEngine.UI.Slider>();
        foreach (var slider in sliders)
        {
            slider.value = slider.maxValue; // Reset health bars to full, etc.
        }

        UnityEngine.UI.Image[] images = canvas.GetComponentsInChildren<UnityEngine.UI.Image>();
        foreach (var image in images)
        {
            image.color = Color.white; // Reset any color changes
        }

        // Hide any temporary UI elements
        GameObject[] uiObjects = new GameObject[canvas.transform.childCount];
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            uiObjects[i] = canvas.transform.GetChild(i).gameObject;
        }

        foreach (GameObject uiObj in uiObjects)
        {
            // Reset visibility based on tag or name
            if (uiObj.name.Contains("Temp") || uiObj.name.Contains("Effect"))
            {
                uiObj.SetActive(false);
            }
        }
    }

    void ResetNPCMovement(GameObject npc, NPCResetData data)
    {
        // Stop NavMesh agent first
        if (data.navAgent != null)
        {
            data.navAgent.isStopped = true;
            data.navAgent.ResetPath();
        }

        // Reset PlayerMoveToObjects script
        if (data.moveScript != null)
        {
            data.moveScript.StopAllCoroutines();
            StartCoroutine(RestartMovementScript(data.moveScript));
        }

        // Reset PlayerMoveRandomly script  
        if (data.randomMoveScript != null)
        {
            data.randomMoveScript.StopAllCoroutines();
            StartCoroutine(RestartRandomMovementScript(data.randomMoveScript));
        }
    }

    // Helper coroutine to properly restart the movement script
    IEnumerator RestartMovementScript(PlayerMoveToObjects moveScript)
    {
        yield return null; // Wait one frame for position reset to complete

        // Manually restart the movement cycle
        moveScript.StartCoroutine("MoveToWaypointsAndBack");
    }

    // Helper coroutine for random movement script
    IEnumerator RestartRandomMovementScript(PlayerMoveRandomly randomScript)
    {
        yield return null; // Wait one frame for position reset to complete

        // Manually restart the movement cycle  
        randomScript.StartCoroutine("MoveToRandomTargetsAndBack");
    }

    void ResetNPCVirusState(GameObject npc)
    {
        // Reset virus simulation state
        VirusSimulation virusComp = npc.GetComponent<VirusSimulation>();
        if (virusComp != null)
        {
            virusComp.ResetToHealthy();
        }
    }

    // Public methods for external scripts to call
    public void ResetSpecificNPC(GameObject npc)
    {
        if (originalNPCData.ContainsKey(npc))
        {
            NPCResetData data = originalNPCData[npc];

            if (resetNPCPositions) ResetNPCPosition(npc, data);
            if (resetWorldSpaceUI) ResetWorldSpaceUI(npc, data);
            if (resetNPCMovement) ResetNPCMovement(npc, data);
            if (resetNPCStates) ResetNPCVirusState(npc);

            if (showDebugLogs)
            {
                Debug.Log($"Reset specific NPC: {npc.name}");
            }
        }
    }

    public void RefreshNPCData()
    {
        originalNPCData.Clear();
        InitializeNPCData();
    }

    // Reset only positions (for quick repositioning)
    public void ResetPositionsOnly()
    {
        foreach (var kvp in originalNPCData)
        {
            GameObject npc = kvp.Key;
            NPCResetData data = kvp.Value;

            if (npc != null)
            {
                ResetNPCPosition(npc, data);
            }
        }
    }

    // Reset only world-space UI (for UI-only resets)
    public void ResetWorldSpaceUIOnly()
    {
        foreach (var kvp in originalNPCData)
        {
            GameObject npc = kvp.Key;
            NPCResetData data = kvp.Value;

            if (npc != null)
            {
                ResetWorldSpaceUI(npc, data);
            }
        }
    }

    // Get statistics
    public int GetTrackedNPCCount()
    {
        return originalNPCData.Count;
    }

    public void SetResetOptions(bool positions, bool worldUI, bool movement, bool states)
    {
        resetNPCPositions = positions;
        resetWorldSpaceUI = worldUI;
        resetNPCMovement = movement;
        resetNPCStates = states;
    }

    // Debug methods
    [ContextMenu("Debug: Show NPC Data")]
    void DebugShowNPCData()
    {
        foreach (var kvp in originalNPCData)
        {
            GameObject npc = kvp.Key;
            NPCResetData data = kvp.Value;

            Debug.Log($"NPC: {npc.name} | Original Pos: {data.originalPosition} | Current Pos: {npc.transform.position} | World UI Count: {data.worldSpaceCanvases.Count}");
        }
    }

    [ContextMenu("Debug: Reset Positions Only")]
    void DebugResetPositions()
    {
        ResetPositionsOnly();
    }
}