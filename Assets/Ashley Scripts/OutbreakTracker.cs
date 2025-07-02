using UnityEngine;
using TMPro;

public class OutbreakStatusTracker : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI statusText;

    [Header("Update Settings")]
    public float updateInterval = 0.2f;

    [Header("Status Thresholds")]
    [Range(0f, 1f)]
    public float criticalThreshold = 0.4f; // 40% infected = CRITICAL
    [Range(0f, 1f)]
    public float spreadingThreshold = 0.15f; // 15% infected = SPREADING
    [Range(0f, 1f)]
    public float containedThreshold = 0.05f; // 5% infected = CONTAINED

    [Header("Display Options")]
    public bool showLabel = true;
    public string customLabel = "Status";
    public bool useColors = true;

    [Header("Status Colors")]
    public Color safeColor = Color.green;
    public Color containedColor = Color.yellow;
    public Color spreadingColor = new Color(1f, 0.5f, 0f);
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
        if (statusText == null)
            statusText = GetComponent<TextMeshProUGUI>();

        if (statusText == null)
        {
            enabled = false;
            return;
        }

        UpdateStatusDisplay();
    }

    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateStatusDisplay();
        }
    }

    void UpdateStatusDisplay()
    {
        int infected = VirusSimulation.GetInfectedCount();
        int total = VirusSimulation.GetTotalNPCCount();

        if (total == 0)
        {
            DisplayStatus("NO DATA", Color.gray);
            return;
        }

        float infectionRate = (float)infected / total;
        OutbreakLevel level = GetOutbreakLevel(infectionRate);
        string statusName = GetStatusName(level);
        Color statusColor = GetStatusColor(level);

        DisplayStatus(statusName, statusColor);
    }

    OutbreakLevel GetOutbreakLevel(float infectionRate)
    {
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

        // Keep label white, status gets coloured
        string displayText = showLabel ? $"<color=white>{customLabel}:</color> {status}" : status;

        statusText.text = displayText;

        if (useColors)
        {
            statusText.color = color;
        }
    }

    public OutbreakLevel GetCurrentLevel()
    {
        int infected = VirusSimulation.GetInfectedCount();
        int total = VirusSimulation.GetTotalNPCCount();

        if (total == 0) return OutbreakLevel.Safe;

        float infectionRate = (float)infected / total;
        return GetOutbreakLevel(infectionRate);
    }

    public string GetCurrentStatus()
    {
        return GetStatusName(GetCurrentLevel());
    }

    public float GetInfectionRate()
    {
        int infected = VirusSimulation.GetInfectedCount();
        int total = VirusSimulation.GetTotalNPCCount();

        if (total == 0) return 0f;
        return (float)infected / total;
    }

    public void SetLabel(string newLabel)
    {
        customLabel = newLabel;
        UpdateStatusDisplay();
    }

    public void ForceUpdate()
    {
        UpdateStatusDisplay();
    }

    public void SetThresholds(float safe, float contained, float spreading, float critical)
    {
        containedThreshold = safe;
        spreadingThreshold = contained;
        criticalThreshold = spreading;
        UpdateStatusDisplay();
    }
}