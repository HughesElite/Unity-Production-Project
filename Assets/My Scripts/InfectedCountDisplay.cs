using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfectedCountDisplay : MonoBehaviour
{
    public TextMeshProUGUI infectedCountText; // Reference to the TextMeshPro UI element

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