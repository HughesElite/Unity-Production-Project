using UnityEngine;
using UnityEngine.UI;

public class InfectedCountDisplay : MonoBehaviour
{
    public Text infectedCountText; // Reference to the Text UI element

    private static int infectedCount = 0;

    void Start()
    {
        // Initialize the Text component with the initial infected count
        UpdateInfectedCountText();
    }

    void UpdateInfectedCountText()
    {
        infectedCountText.text = "Infected: " + infectedCount.ToString();
    }

    // Call this function when an NPC gets infected
    public void IncreaseInfectedCount()
    {
        infectedCount++;
        UpdateInfectedCountText(); // Update the UI
    }
}
