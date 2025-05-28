using UnityEngine;
using System.Collections.Generic;

public class VirusSimulation : MonoBehaviour
{
    public float infectionRadius = 2f;
    public Color infectedColor = Color.black;
    public float checkInterval = 0.1f; // Check every 0.1 seconds instead of every frame
    public bool startInfected = false; // Check this for patient zero

    private static List<VirusSimulation> allNPCs = new List<VirusSimulation>();
    private Renderer npcRenderer;
    private bool isInfected = false;
    private float nextCheckTime = 0f;

    void Start()
    {
        npcRenderer = GetComponent<Renderer>();

        // Register this NPC
        allNPCs.Add(this);

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
        // Only check at intervals, not every frame, and only if this NPC is infected
        if (Time.time >= nextCheckTime && isInfected)
        {
            nextCheckTime = Time.time + checkInterval;
            NPCInfectedCheck();
        }
    }

    void NPCInfectedCheck()
    {
        // If this NPC is infected, check if it can infect nearby NPCs
        foreach (VirusSimulation otherNPC in allNPCs)
        {
            if (otherNPC == this || otherNPC.isInfected == true)
                continue; // Skip self and already infected NPCs

            float distance = Vector3.Distance(transform.position, otherNPC.transform.position);
            if (distance < infectionRadius)
            {
                otherNPC.InfectNPC(); // Infect the OTHER NPC
            }
        }
    }

    void InfectNPC()
    {
        if (!isInfected)
        {
            isInfected = true;
            npcRenderer.material.color = infectedColor;
        }
    }

    // Public method to infect this NPC from external sources (like player contact)
    public void ForceInfection()
    {
        InfectNPC();
    }
}