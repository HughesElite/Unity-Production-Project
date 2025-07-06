using UnityEngine;
using TMPro;

public class PeakInfectionTracker : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI peakText;

    [Header("Update Settings")]
    public float updateInterval = 0.1f;

    [Header("Display Options")]
    public bool showLabel = true;
    public string customLabel = "Peak";

    private float nextUpdateTime;
    private int peakInfections = 0; // Highest simultaneous infection count recorded

    // NEW: Additional tracking for results screen
    private string peakTime = "Not reached";
    private float outbreakStartTime = -1f;
    private bool outbreakStarted = false;

    void Start()
    {
        if (peakText == null)
            peakText = GetComponent<TextMeshProUGUI>();

        if (peakText == null)
        {
            enabled = false;
            return;
        }

        UpdatePeakDisplay();
    }

    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdatePeakDisplay();
        }
    }

    void UpdatePeakDisplay()
    {
        int currentInfected = VirusSimulation.GetInfectedCount();
        Debug.Log($"Peak tracker sees {currentInfected} infected NPCs");

        // Peak only increases, never decreases (historical maximum)
        if (currentInfected > peakInfections)
        {
            peakInfections = currentInfected;

            // NEW: Record when peak occurred
            SimulationClock clock = FindFirstObjectByType<SimulationClock>();
            if (clock != null)
            {
                peakTime = $"{clock.GetCurrentDayName()} {clock.GetTimeString()}";
            }
        }

        // NEW: Track outbreak start
        if (!outbreakStarted && currentInfected > 0)
        {
            outbreakStarted = true;
            outbreakStartTime = Time.time;
        }

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

    public int GetPeakInfections()
    {
        return peakInfections;
    }

    public void ResetPeak()
    {
        peakInfections = 0;
        // NEW: Reset additional tracking
        peakTime = "Not reached";
        outbreakStartTime = -1f;
        outbreakStarted = false;
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

    // NEW: Getter methods for results screen
    public string GetPeakTime()
    {
        return peakTime;
    }

    public float GetOutbreakStartTime()
    {
        return outbreakStartTime;
    }
}