using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfectedCountDisplay : MonoBehaviour
{
    public TextMeshProUGUI infectedCountText; // Reference to the TextMeshPro UI element
    private static int infectedCount = 0;

    void Start()
    {
        // Check if game is being reset (from GameControl)
        if (GameControl.IsResetting)
        {
            // Reset the counter to zero
            infectedCount = 0;

            // Tell GameControl we've handled the reset
            GameControl.FinishReset();
        }

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

    // Optional: Add a public method to manually reset the count if needed
    public void ResetCount()
    {
        infectedCount = 0;
        UpdateInfectedCountText();
    }
}