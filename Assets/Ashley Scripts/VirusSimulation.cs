using UnityEngine;

public class VirusSimulation : MonoBehaviour
{
    public float infectionRadius = 2f;  // Distance at which NPCs become "infected"
    public Color infectedColor = Color.black;  // Color when infected
    private InfectedCountDisplay infectedCountDisplay;

    void Start()
    {
        // Get reference to the InfectedCountDisplay to update the infected count UI
        infectedCountDisplay = FindFirstObjectByType<InfectedCountDisplay>();
    }

    void Update()
    {
        // Check the distance to other NPCs in the scene
        NPCInfectedCheck();
    }

    void NPCInfectedCheck()
    {
        // Find all NPC objects in the scene with the "NPC" tag
        GameObject[] allNPCs = GameObject.FindGameObjectsWithTag("NPC");

        foreach (GameObject npc in allNPCs)
        {
            // Skip checking the NPC itself
            if (npc == gameObject)
                continue;

            // Check if the NPC has already been infected by checking its color
            Renderer npcRenderer = npc.GetComponent<Renderer>();
            if (npcRenderer.material.color == infectedColor)
                continue;  // Skip if NPC is already infected

            // Get the distance between this NPC and the other NPC
            float distance = Vector3.Distance(transform.position, npc.transform.position);

            // If the distance is smaller than the infection radius, infect the NPC
            if (distance < infectionRadius)
            {
                // Change color to infected color
                npcRenderer.material.color = infectedColor;

                // Update the infected count UI
                infectedCountDisplay.IncreaseInfectedCount();
            }
        }
    }
}
