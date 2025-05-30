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
    private int peakInfections = 0;

    void Start()
    {
        // Get TextMeshPro component if not assigned
        if (peakText == null)
            peakText = GetComponent<TextMeshProUGUI>();

        if (peakText == null)
        {
            Debug.LogError("PeakInfectionTracker: No TextMeshPro component found! Please assign one or attach this script to a TextMeshPro object.");
            enabled = false;
            return;
        }

        // Initial update
        UpdatePeakDisplay();
    }

    void Update()
    {
        // Update at specified intervals
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdatePeakDisplay();
        }
    }

    void UpdatePeakDisplay()
    {
        // Get current infected count
        int currentInfected = VirusSimulation.GetInfectedCount();

        // Update peak if current is higher
        if (currentInfected > peakInfections)
        {
            peakInfections = currentInfected;
        }

        // Update display
        if (peakText != null)
        {
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

    // Public methods for external control
    public int GetPeakInfections()
    {
        return peakInfections;
    }

    public void ResetPeak()
    {
        peakInfections = 0;
        UpdatePeakDisplay();
    }

    public void SetLabel(string newLabel)
    {
        customLabel = newLabel;
        UpdatePeakDisplay();
    }

    public void ForceUpdate()
    {
        UpdatePeakDisplay();
    }
}