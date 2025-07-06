using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RValueCalculator : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI rValueText;

    [Header("Calculation Settings")]
    public float sampleInterval = 1f; // How often to sample data (seconds)
    public int maxSamples = 15; // How many samples to keep for calculation
    public bool pauseWithGameController = true;

    [Header("Display Options")]
    public bool showLabel = true;
    public string customLabel = "R-Value";
    public bool showInterpretation = true; // Show "Growing/Stable/Declining"

    [Header("R-Value Thresholds")]
    public float stableThreshold = 0.1f; // Within +/- 0.1 of 1.0 is considered "stable"

    [Header("Simulation Parameters")]
    public float estimatedInfectionDuration = 5f; // Should match your VirusSimulation infectionDuration

    public enum OutbreakStatus
    {
        Growing,   // R > 1
        Stable,    // R approximately 1
        Declining  // R < 1
    }

    private GameController gameController;
    private Queue<InfectionSample> samples = new Queue<InfectionSample>();
    private float nextSampleTime = 0f;
    private float currentRValue = 1f;

    // NEW: Additional tracking for results screen
    private float highestRValue = 1f;
    private string highestRValueTime = "";

    private struct InfectionSample
    {
        public float time;
        public int infected;
        public int recovered;
        public int susceptible;
        public int total;

        public InfectionSample(float time, int infected, int recovered, int susceptible, int total)
        {
            this.time = time;
            this.infected = infected;
            this.recovered = recovered;
            this.susceptible = susceptible;
            this.total = total;
        }
    }

    void Start()
    {
        // Get TextMeshPro component if not assigned
        if (rValueText == null)
            rValueText = GetComponent<TextMeshProUGUI>();

        if (rValueText == null)
        {
            Debug.LogError("RValueCalculator: No TextMeshPro component found!");
            enabled = false;
            return;
        }

        // Get references
        if (pauseWithGameController)
        {
            gameController = GameController.Instance;
        }

        // Try to get infection duration from a VirusSimulation in the scene
        VirusSimulation virusSim = FindFirstObjectByType<VirusSimulation>();
        if (virusSim != null)
        {
            estimatedInfectionDuration = virusSim.infectionDuration;
            Debug.Log("RValueCalculator: Using infection duration from VirusSimulation: " + estimatedInfectionDuration);
        }

        // Initial sample
        TakeSample();
        UpdateDisplay();
    }

    void Update()
    {
        // Skip if game is paused
        if (pauseWithGameController && gameController != null && gameController.IsGamePaused())
            return;

        // Take samples at specified intervals
        if (Time.time >= nextSampleTime)
        {
            nextSampleTime = Time.time + sampleInterval;
            TakeSample();
            CalculateRValue();
            UpdateDisplay();
        }
    }

    void TakeSample()
    {
        int infected = VirusSimulation.GetInfectedCount();
        int recovered = VirusSimulation.GetRecoveredCount();
        int healthy = VirusSimulation.GetHealthyCount();
        int total = VirusSimulation.GetTotalNPCCount();

        InfectionSample sample = new InfectionSample(Time.time, infected, recovered, healthy, total);
        samples.Enqueue(sample);

        // Remove old samples to maintain max count
        while (samples.Count > maxSamples)
        {
            samples.Dequeue();
        }
    }

    void CalculateRValue()
    {
        if (samples.Count < 3)
        {
            currentRValue = 1f; // Default assumption
            return;
        }

        // Convert to array for easier access
        InfectionSample[] sampleArray = samples.ToArray();

        // Use SIR model approach to estimate R-value
        currentRValue = CalculateUsingGrowthRate(sampleArray);

        // Clamp to reasonable bounds
        currentRValue = Mathf.Clamp(currentRValue, 0f, 10f);

        // NEW: Track highest R-value
        if (currentRValue > highestRValue)
        {
            highestRValue = currentRValue;
            SimulationClock clock = FindFirstObjectByType<SimulationClock>();
            if (clock != null)
            {
                highestRValueTime = $"{clock.GetCurrentDayName()} {clock.GetTimeString()}";
            }
        }
    }

    float CalculateUsingGrowthRate(InfectionSample[] samples)
    {
        if (samples.Length < 3) return 1f;

        // Get recent samples for calculation
        InfectionSample oldest = samples[0];
        InfectionSample newest = samples[samples.Length - 1];

        float timeDiff = newest.time - oldest.time;
        if (timeDiff <= 0) return 1f;

        // Calculate infection growth rate
        int infectionChange = newest.infected - oldest.infected;

        // If no infected people, look at recovered growth (past infections)
        if (newest.infected == 0 && oldest.infected == 0)
        {
            int recoveredChange = newest.recovered - oldest.recovered;
            if (recoveredChange <= 0) return 0f; // No infections happening

            // Estimate R from recovery data (less accurate but better than nothing)
            float recoveryRate = recoveredChange / timeDiff;
            return Mathf.Max(0.1f, recoveryRate * estimatedInfectionDuration / Mathf.Max(1, newest.total));
        }

        // If infections are declining (more recovering than new infections)
        if (infectionChange < 0)
        {
            // R < 1, calculate based on decline rate
            float declineRate = Mathf.Abs(infectionChange) / timeDiff;
            float avgInfected = (newest.infected + oldest.infected) / 2f;

            if (avgInfected > 0)
            {
                float recoveryRate = declineRate / avgInfected;
                return Mathf.Max(0.1f, 1f - (recoveryRate * estimatedInfectionDuration));
            }
            return 0.5f; // Declining
        }

        // If infections are growing
        if (infectionChange > 0)
        {
            float growthRate = infectionChange / timeDiff;
            float avgInfected = (newest.infected + oldest.infected) / 2f;

            if (avgInfected > 0)
            {
                // R = growth rate * infection duration / current infected proportion
                float susceptibleRatio = (float)newest.susceptible / newest.total;
                susceptibleRatio = Mathf.Max(0.01f, susceptibleRatio); // Avoid division by zero

                float baseR = (growthRate / avgInfected) * estimatedInfectionDuration;
                return 1f + (baseR / susceptibleRatio);
            }
            return 1.5f; // Growing
        }

        // Stable case
        return 1f;
    }

    void UpdateDisplay()
    {
        if (rValueText == null) return;

        string displayText = BuildDisplayText();
        rValueText.text = displayText;
    }

    string BuildDisplayText()
    {
        string rValueString = currentRValue.ToString("F1");

        if (!showLabel && !showInterpretation)
        {
            return rValueString;
        }

        string result = "";

        if (showLabel)
        {
            result += customLabel + ": " + rValueString;
        }
        else
        {
            result += rValueString;
        }

        if (showInterpretation)
        {
            string interpretation = GetInterpretation();
            result += " (" + interpretation + ")";
        }

        return result;
    }

    string GetInterpretation()
    {
        OutbreakStatus status = GetOutbreakStatus();

        switch (status)
        {
            case OutbreakStatus.Growing: return "Growing";
            case OutbreakStatus.Stable: return "Stable";
            case OutbreakStatus.Declining: return "Declining";
            default: return "Unknown";
        }
    }

    OutbreakStatus GetOutbreakStatus()
    {
        if (currentRValue > 1f + stableThreshold)
            return OutbreakStatus.Growing;
        else if (currentRValue < 1f - stableThreshold)
            return OutbreakStatus.Declining;
        else
            return OutbreakStatus.Stable;
    }

    // Public methods for external control
    public float GetCurrentRValue()
    {
        return currentRValue;
    }

    public OutbreakStatus GetCurrentStatus()
    {
        return GetOutbreakStatus();
    }

    public void SetInfectionDuration(float duration)
    {
        estimatedInfectionDuration = Mathf.Max(0.1f, duration);
    }

    public void SetSampleInterval(float interval)
    {
        sampleInterval = Mathf.Max(0.1f, interval);
    }

    public void SetMaxSamples(int maxSamples)
    {
        this.maxSamples = Mathf.Max(3, maxSamples);

        // Remove excess samples if new max is smaller
        while (samples.Count > this.maxSamples)
        {
            samples.Dequeue();
        }
    }

    public void ResetSamples()
    {
        samples.Clear();
        currentRValue = 1f;
        // NEW: Reset highest R-value tracking
        highestRValue = 1f;
        highestRValueTime = "";
        UpdateDisplay();
    }

    public void ForceUpdate()
    {
        TakeSample();
        CalculateRValue();
        UpdateDisplay();
    }

    // NEW: Getter methods for results screen
    public float GetHighestRValue()
    {
        return highestRValue;
    }

    public string GetHighestRValueTime()
    {
        return highestRValueTime;
    }

    // For debugging
    [ContextMenu("Reset R-Value Tracker")]
    void ResetTracker()
    {
        ResetSamples();
    }

    [ContextMenu("Force R-Value Update")]
    void ForceRValueUpdate()
    {
        ForceUpdate();
        Debug.Log("Current R-Value: " + currentRValue.ToString("F2"));
    }
}