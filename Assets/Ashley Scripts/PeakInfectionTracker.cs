using UnityEngine;
using TMPro;

public class PeakInfectionTracker : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI peakText;

    [Header("Update Settings")]
    public float updateInterval = 0.1f; // How often to update the display (seconds)

    [Header("Display Options")]
    public bool showLabel = true;
    public string customLabel = "Peak";

    private float nextUpdateTime;
    private int peakInfections = 0; // Highest simultaneous infection count recorded

    void Start()
    {
        // Auto-detect TextMeshPro component if not manually assigned
        if (peakText == null)
            peakText = GetComponent<TextMeshProUGUI>();

        // Ensure we have a valid text component before starting tracking
        if (peakText == null)
        {
            Debug.LogError("PeakInfectionTracker: No TextMeshPro component found! Please assign one or attach this script to a TextMeshPro object.");
            enabled = false;
            return;
        }

        // Display initial peak value (starts at 0)
        UpdatePeakDisplay();
    }

    void Update()
    {
        // Throttle updates for performance (peak tracking doesn't need every frame)
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdatePeakDisplay();
        }
    }

    void UpdatePeakDisplay()
    {
        // Get current number of simultaneously infected NPCs
        int currentInfected = VirusSimulation.GetInfectedCount();

        // Track the highest infection count reached during this simulation
        // Peak only increases, never decreases (historical maximum)
        if (currentInfected > peakInfections)
        {
            peakInfections = currentInfected;
        }

        // Update UI display with current peak value
        if (peakText != null)
        {
            // Format display with optional label
            if (showLabel)
            {
                peakText.text = $"{customLabel}: {peakInfections}";
            }
            else
            {
                peakText.text = peakInfections.ToString();
            }
        }
    }

    // External access methods for other scripts to query peak data
    public int GetPeakInfections()
    {
        // Return the highest infection count recorded so far
        return peakInfections;
    }

    public void ResetPeak()
    {
        // Clear peak history and start tracking fresh (useful for simulation resets)
        peakInfections = 0;
        UpdatePeakDisplay();
    }

    public void SetLabel(string newLabel)
    {
        // Change display label and refresh UI immediately
        customLabel = newLabel;
        UpdatePeakDisplay();
    }

    public void ForceUpdate()
    {
        // Manually trigger peak check and display update outside normal interval
        UpdatePeakDisplay();
    }
}