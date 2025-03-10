using UnityEngine;

public class VirusSimulation : MonoBehaviour
{
    public float infectionRadius = 2f;  // Distance at which NPCs become "infected"
    public Color infectedColor = Color.black;  // Color when infected
    private Renderer npcRenderer;

    void Start()
    {
        // Get the Renderer component to change color
        npcRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // Check the distance to other NPCs in the scene
        NPCInfectedCheck();
    }

    void NPCInfectedCheck()
    {
        // Find all NPC objects in the scene
        GameObject[] allNPCs = GameObject.FindGameObjectsWithTag("NPC");

        foreach (GameObject npc in allNPCs)
        {
            // Skip checking the NPC itself
            if (npc == gameObject)
                continue;

            // Get the distance between this NPC and the other NPC
            float distance = Vector3.Distance(transform.position, npc.transform.position);

            // If the distance is smaller than the infection radius, turn both NPCs black
            if (distance < infectionRadius)
            {
                // Change color of both NPCs to simulate infection
                npc.GetComponent<Renderer>().material.color = infectedColor;
                npcRenderer.material.color = infectedColor;
            }
        }
    }
}
