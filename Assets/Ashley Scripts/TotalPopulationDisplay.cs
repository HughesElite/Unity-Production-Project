using UnityEngine;
using TMPro;

public class PopulationCounter : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI populationText;

    [Header("Display Options")]
    public bool showLabel = true;
    public string customLabel = "Population";
    public string npcTag = "NPC";

    [Header("Update Settings")]
    public float updateInterval = 0.5f; // How often to count NPCs (seconds)

    private float nextUpdateTime = 0f;
    private int currentPopulation = 0;

    void Start()
    {
        // Get TextMeshPro component if not assigned
        if (populationText == null)
            populationText = GetComponent<TextMeshProUGUI>();

        if (populationText == null)
        {
            Debug.LogError("PopulationCounter: No TextMeshPro component found! Please assign one or attach this script to a TextMeshPro object.");
            enabled = false;
            return;
        }

        // Initial update
        UpdatePopulationCount();
    }

    void Update()
    {
        // Update at specified intervals
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdatePopulationCount();
        }
    }

    void UpdatePopulationCount()
    {
        // Count all active GameObjects with the NPC tag
        GameObject[] npcs = GameObject.FindGameObjectsWithTag(npcTag);

        // Only count active NPCs
        int activeCount = 0;
        foreach (GameObject npc in npcs)
        {
            if (npc.activeInHierarchy)
            {
                activeCount++;
            }
        }

        currentPopulation = activeCount;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (populationText == null) return;

        if (showLabel)
        {
            populationText.text = customLabel + " " + currentPopulation.ToString();
        }
        else
        {
            populationText.text = currentPopulation.ToString();
        }
    }

    // Public methods for external control
    public int GetCurrentPopulation()
    {
        return currentPopulation;
    }

    public void SetLabel(string newLabel)
    {
        customLabel = newLabel;
        UpdateDisplay();
    }

    public void SetNPCTag(string newTag)
    {
        npcTag = newTag;
        UpdatePopulationCount(); // Immediately recount with new tag
    }

    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.1f, interval); // Minimum 0.1 seconds
    }

    public void ForceUpdate()
    {
        UpdatePopulationCount();
    }

    // For debugging
    [ContextMenu("Force Count Update")]
    void ForceCountUpdate()
    {
        ForceUpdate();
        Debug.Log("Current population: " + currentPopulation + " NPCs");
    }
}