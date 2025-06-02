using UnityEngine;
using TMPro;

public class OutbreakStatusTracker : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI statusText;

    [Header("Update Settings")]
    public float updateInterval = 0.2f; // How often to update the display (seconds)

    [Header("Status Thresholds")]
    [Range(0f, 1f)]
    public float criticalThreshold = 0.4f; // 40% infected = CRITICAL
    [Range(0f, 1f)]
    public float spreadingThreshold = 0.15f; // 15% infected = SPREADING
    [Range(0f, 1f)]
    public float containedThreshold = 0.05f; // 5% infected = CONTAINED
    // Below 5% = SAFE

    [Header("Display Options")]
    public bool showLabel = true;
    public string customLabel = "Status";
    public bool useColors = true;

    [Header("Status Colors")]
    public Color safeColor = Color.green;
    public Color containedColor = Color.yellow;
    public Color spreadingColor = new Color(1f, 0.5f, 0f); // Orange: #FF8000
    public Color criticalColor = Color.red;

    private float nextUpdateTime;

    public enum OutbreakLevel
    {
        Safe,
        Contained,
        Spreading,
        Critical
    }

    void Start()
    {
        // Auto-detect TextMeshPro component if not manually assigned
        if (statusText == null)
            statusText = GetComponent<TextMeshProUGUI>();

        // Ensure we have a valid text component before proceeding
        if (statusText == null)
        {
            Debug.LogError("OutbreakStatusTracker: No TextMeshPro component found! Please assign one or attach this script to a TextMeshPro object.");
            enabled = false;
            return;
        }

        // Display initial status immediately on startup
        UpdateStatusDisplay();
    }

    void Update()
    {
        // Throttle updates to improve performance (no need to update every frame)
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateStatusDisplay();
        }
    }

    void UpdateStatusDisplay()
    {
        // Get current infection data from the virus simulation
        int infected = VirusSimulation.GetInfectedCount();
        int total = VirusSimulation.GetTotalNPCCount();

        // Handle edge case where no NPCs exist in simulation
        if (total == 0)
        {
            DisplayStatus("NO DATA", Color.gray);
            return;
        }

        // Calculate what percentage of population is currently infected
        float infectionRate = (float)infected / total;

        // Classify outbreak severity based on infection percentage
        OutbreakLevel level = GetOutbreakLevel(infectionRate);

        // Get display text and color for this severity level
        string statusName = GetStatusName(level);
        Color statusColor = GetStatusColor(level);

        // Update the UI display with new status
        DisplayStatus(statusName, statusColor);
    }

    OutbreakLevel GetOutbreakLevel(float infectionRate)
    {
        // Determine outbreak severity using threshold-based classification
        if (infectionRate >= criticalThreshold)
            return OutbreakLevel.Critical;
        else if (infectionRate >= spreadingThreshold)
            return OutbreakLevel.Spreading;
        else if (infectionRate >= containedThreshold)
            return OutbreakLevel.Contained;
        else
            return OutbreakLevel.Safe;
    }

    string GetStatusName(OutbreakLevel level)
    {
        // Convert outbreak level enum to user-friendly display text
        switch (level)
        {
            case OutbreakLevel.Critical: return "Critical";
            case OutbreakLevel.Spreading: return "Spreading";
            case OutbreakLevel.Contained: return "Contained";
            case OutbreakLevel.Safe: return "Safe";
            default: return "UNKNOWN";
        }
    }

    Color GetStatusColor(OutbreakLevel level)
    {
        // Assign color coding for visual severity indication
        switch (level)
        {
            case OutbreakLevel.Critical: return criticalColor;
            case OutbreakLevel.Spreading: return spreadingColor;
            case OutbreakLevel.Contained: return containedColor;
            case OutbreakLevel.Safe: return safeColor;
            default: return Color.white;
        }
    }

    void DisplayStatus(string status, Color color)
    {
        if (statusText == null) return;

        // Format text with optional label (label stays white, status gets colored)
        string displayText = showLabel ? $"<color=white>{customLabel}:</color> {status}" : status;

        // Apply formatted text to UI component
        statusText.text = displayText;

        // Apply color coding if enabled
        if (useColors)
        {
            statusText.color = color;
        }
    }

    // External access methods for other scripts to query outbreak status
    public OutbreakLevel GetCurrentLevel()
    {
        // Calculate current outbreak level without updating display
        int infected = VirusSimulation.GetInfectedCount();
        int total = VirusSimulation.GetTotalNPCCount();

        if (total == 0) return OutbreakLevel.Safe;

        float infectionRate = (float)infected / total;
        return GetOutbreakLevel(infectionRate);
    }

    public string GetCurrentStatus()
    {
        // Get human-readable status string for current conditions
        return GetStatusName(GetCurrentLevel());
    }

    public float GetInfectionRate()
    {
        // Get current infection rate as percentage (0.0 to 1.0)
        int infected = VirusSimulation.GetInfectedCount();
        int total = VirusSimulation.GetTotalNPCCount();

        if (total == 0) return 0f;
        return (float)infected / total;
    }

    public void SetLabel(string newLabel)
    {
        // Change the display label and refresh the UI
        customLabel = newLabel;
        UpdateStatusDisplay();
    }

    public void ForceUpdate()
    {
        // Manually trigger status update outside normal interval
        UpdateStatusDisplay();
    }

    // Runtime configuration method for dynamic threshold adjustment
    public void SetThresholds(float safe, float contained, float spreading, float critical)
    {
        // Update severity thresholds and refresh display immediately
        containedThreshold = safe;
        spreadingThreshold = contained;
        criticalThreshold = spreading;
        UpdateStatusDisplay();
    }
}