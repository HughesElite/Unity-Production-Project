using UnityEngine;
using TMPro;

public class VirusStatsDisplay : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI statsText;

    [Header("Display Options")]
    public bool showHealthy = true;
    public bool showInfected = true;
    public bool showRecovered = true;
    public bool showTotal = true;
    public bool showPercentages = true;

    [Header("Update Settings")]
    public float updateInterval = 0.1f; // How often to update the display (seconds)

    [Header("Display Format")]
    public DisplayStyle displayStyle = DisplayStyle.Detailed;
    public string customFormat = "Infected: {infected}/{total}";

    public enum DisplayStyle
    {
        Simple,      // "Infected: 12"
        Detailed,    // "Healthy: 38\nInfected: 12\nRecovered: 5"
        Percentages, // "Healthy: 38 (76%)\nInfected: 12 (24%)"
        Compact,     // "H:38\nI:12\nR:5"
        Custom       // Uses customFormat string
    }

    private float nextUpdateTime;

    void Start()
    {
        // Get TextMeshPro component if not assigned
        if (statsText == null)
            statsText = GetComponent<TextMeshProUGUI>();

        if (statsText == null)
        {
            Debug.LogError("VirusStatsDisplay: No TextMeshPro component found! Please assign one or attach this script to a TextMeshPro object.");
            enabled = false;
            return;
        }

        // Initial update
        UpdateStatsDisplay();
    }

    void Update()
    {
        // Update at specified intervals
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateStatsDisplay();
        }
    }

    void UpdateStatsDisplay()
    {
        // Get current statistics from VirusSimulation
        int healthy = VirusSimulation.GetHealthyCount();
        int infected = VirusSimulation.GetInfectedCount();
        int recovered = VirusSimulation.GetRecoveredCount();
        int total = VirusSimulation.GetTotalNPCCount();

        // Generate display text based on selected style
        string displayText = GetFormattedText(healthy, infected, recovered, total);

        // Update the text component
        if (statsText != null)
        {
            statsText.text = displayText;
        }
    }

    string GetFormattedText(int healthy, int infected, int recovered, int total)
    {
        switch (displayStyle)
        {
            case DisplayStyle.Simple:
                return $"Infected: {infected}";

            case DisplayStyle.Detailed:
                return BuildDetailedText(healthy, infected, recovered, total);

            case DisplayStyle.Percentages:
                return BuildPercentageText(healthy, infected, recovered, total);

            case DisplayStyle.Compact:
                return BuildCompactText(healthy, infected, recovered, total);

            case DisplayStyle.Custom:
                return BuildCustomText(healthy, infected, recovered, total);

            default:
                return BuildDetailedText(healthy, infected, recovered, total);
        }
    }

    string BuildDetailedText(int healthy, int infected, int recovered, int total)
    {
        var parts = new System.Collections.Generic.List<string>();

        if (showHealthy) parts.Add($"Healthy: {healthy}");
        if (showRecovered) parts.Add($"Recovered: {recovered}");
        if (showInfected) parts.Add($"Infected: {infected}");

        return string.Join("\n", parts);
    }

    string BuildPercentageText(int healthy, int infected, int recovered, int total)
    {
        if (total == 0) return "No NPCs found";

        var parts = new System.Collections.Generic.List<string>();

        if (showHealthy && showPercentages)
            parts.Add($"Healthy: {healthy} ({(healthy * 100f / total):F0}%)");
        else if (showHealthy)
            parts.Add($"Healthy: {healthy}");

        if (showInfected && showPercentages)
            parts.Add($"Infected: {infected} ({(infected * 100f / total):F0}%)");
        else if (showInfected)
            parts.Add($"Infected: {infected}");

        if (showRecovered && showPercentages)
            parts.Add($"Recovered: {recovered} ({(recovered * 100f / total):F0}%)");
        else if (showRecovered)
            parts.Add($"Recovered: {recovered}");

        if (showTotal) parts.Add($"Total: {total}");

        return string.Join("\n", parts);
    }

    string BuildCompactText(int healthy, int infected, int recovered, int total)
    {
        var parts = new System.Collections.Generic.List<string>();

        if (showHealthy) parts.Add($"H:{healthy}");
        if (showInfected) parts.Add($"I:{infected}");
        if (showRecovered) parts.Add($"R:{recovered}");
       

        return string.Join("\n", parts);
    }

    string BuildCustomText(int healthy, int infected, int recovered, int total)
    {
        // Replace placeholders in custom format string
        return customFormat
            .Replace("{healthy}", healthy.ToString())
            .Replace("{infected}", infected.ToString())
            .Replace("{recovered}", recovered.ToString())
            .Replace("{infected_percent}", total > 0 ? (infected * 100f / total).ToString("F0") : "0")
            .Replace("{healthy_percent}", total > 0 ? (healthy * 100f / total).ToString("F0") : "0")
            .Replace("{recovered_percent}", total > 0 ? (recovered * 100f / total).ToString("F0") : "0");
    }

    // Public methods for external control
    public void SetDisplayStyle(DisplayStyle style)
    {
        displayStyle = style;
        UpdateStatsDisplay();
    }

    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.05f, interval); // Minimum 0.05 seconds
    }

    public void ForceUpdate()
    {
        UpdateStatsDisplay();
    }

    // Method to manually set custom format at runtime
    public void SetCustomFormat(string format)
    {
        customFormat = format;
        if (displayStyle == DisplayStyle.Custom)
        {
            UpdateStatsDisplay();
        }
    }
}