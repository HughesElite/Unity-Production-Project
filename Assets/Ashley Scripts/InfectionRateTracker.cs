using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InfectionRateTracker : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI rateText;

    [Header("Rate Calculation Settings")]
    public float sampleInterval = 1f; // How often to sample infection count (seconds)
    public int maxSamples = 10; // How many samples to keep for rate calculation
    public bool pauseWithGameController = true;

    [Header("Display Options")]
    public bool showLabel = true;
    public string customLabel = "Infection Rate";
    public RateDisplayFormat displayFormat = RateDisplayFormat.PerMinute;
    public bool showTrend = true; // Show ^ v - arrows
    public bool useColors = false;

    [Header("Display Colors")]
    public Color increasingColor = Color.red;
    public Color decreasingColor = Color.green;
    public Color stableColor = Color.yellow;
    public Color noDataColor = Color.gray;

    public enum RateDisplayFormat
    {
        PerSecond,   // "+2.3/sec"
        PerMinute,   // "+5/min"
        PerHour      // "+12/hr" (using game time from SimulationClock)
    }

    public enum TrendDirection
    {
        Increasing,
        Decreasing,
        Stable
    }

    private GameController gameController;
    private SimulationClock simulationClock;
    private Queue<InfectionSample> samples = new Queue<InfectionSample>();
    private float nextSampleTime = 0f;
    private float currentRate = 0f;
    private TrendDirection currentTrend = TrendDirection.Stable;

    private struct InfectionSample
    {
        public float realTime;
        public float gameTime; // For game-hour calculations
        public int infectedCount;

        public InfectionSample(float realTime, float gameTime, int infectedCount)
        {
            this.realTime = realTime;
            this.gameTime = gameTime;
            this.infectedCount = infectedCount;
        }
    }

    void Start()
    {
        // Get components
        if (rateText == null)
            rateText = GetComponent<TextMeshProUGUI>();

        if (rateText == null)
        {
            Debug.LogError("InfectionRateTracker: No TextMeshPro component found!");
            enabled = false;
            return;
        }

        // Get references
        if (pauseWithGameController)
        {
            gameController = GameController.Instance;
        }

        simulationClock = FindFirstObjectByType<SimulationClock>();

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
            CalculateRate();
            UpdateDisplay();
        }
    }

    void TakeSample()
    {
        int currentInfected = VirusSimulation.GetInfectedCount();
        float gameTime = simulationClock != null ? simulationClock.GetCurrentHour() : 0f;

        InfectionSample sample = new InfectionSample(Time.time, gameTime, currentInfected);
        samples.Enqueue(sample);

        // Remove old samples to maintain max count
        while (samples.Count > maxSamples)
        {
            samples.Dequeue();
        }
    }

    void CalculateRate()
    {
        if (samples.Count < 2)
        {
            currentRate = 0f;
            currentTrend = TrendDirection.Stable;
            return;
        }

        // Convert to array for easier access
        InfectionSample[] sampleArray = samples.ToArray();
        InfectionSample oldestSample = sampleArray[0];
        InfectionSample newestSample = sampleArray[sampleArray.Length - 1];

        // Calculate time difference based on display format
        float timeDifference = GetTimeDifference(oldestSample, newestSample);

        if (timeDifference <= 0f)
        {
            currentRate = 0f;
            currentTrend = TrendDirection.Stable;
            return;
        }

        // Calculate infection difference
        int infectionDifference = newestSample.infectedCount - oldestSample.infectedCount;

        // Calculate rate
        currentRate = infectionDifference / timeDifference;

        // Determine trend (compare with previous rate if available)
        if (samples.Count >= 3)
        {
            // Compare recent rate vs older rate for trend
            InfectionSample midSample = sampleArray[sampleArray.Length / 2];
            float recentTimeDiff = GetTimeDifference(midSample, newestSample);
            float olderTimeDiff = GetTimeDifference(oldestSample, midSample);

            if (recentTimeDiff > 0 && olderTimeDiff > 0)
            {
                float recentRate = (newestSample.infectedCount - midSample.infectedCount) / recentTimeDiff;
                float olderRate = (midSample.infectedCount - oldestSample.infectedCount) / olderTimeDiff;

                float rateDifference = recentRate - olderRate;

                if (rateDifference > 0.1f) // Threshold for "significant" change
                    currentTrend = TrendDirection.Increasing;
                else if (rateDifference < -0.1f)
                    currentTrend = TrendDirection.Decreasing;
                else
                    currentTrend = TrendDirection.Stable;
            }
        }
    }

    float GetTimeDifference(InfectionSample older, InfectionSample newer)
    {
        switch (displayFormat)
        {
            case RateDisplayFormat.PerSecond:
                return newer.realTime - older.realTime;

            case RateDisplayFormat.PerMinute:
                return (newer.realTime - older.realTime) / 60f;

            case RateDisplayFormat.PerHour:
                // Use game time if available, otherwise fall back to real time
                if (simulationClock != null)
                {
                    float gameTimeDiff = newer.gameTime - older.gameTime;
                    // Handle day wrapping (if hour goes from 24 to 1)
                    if (gameTimeDiff < 0) gameTimeDiff += 24;
                    return gameTimeDiff;
                }
                else
                {
                    return (newer.realTime - older.realTime) / 3600f; // Convert to hours
                }

            default:
                return newer.realTime - older.realTime;
        }
    }

    void UpdateDisplay()
    {
        if (rateText == null) return;

        string displayText = BuildDisplayText();
        Color displayColor = GetDisplayColor();

        rateText.text = displayText;

        if (useColors)
        {
            rateText.color = displayColor;
        }
    }

    string BuildDisplayText()
    {
        if (samples.Count < 2)
        {
            string noDataText = showLabel ? customLabel + ": No Data" : "No Data";
            return noDataText;
        }

        // Format rate based on display format
        string rateString = FormatRate(currentRate);

        // Add trend arrow if enabled
        string trendArrow = "";
        if (showTrend)
        {
            switch (currentTrend)
            {
                case TrendDirection.Increasing: trendArrow = " ^"; break;
                case TrendDirection.Decreasing: trendArrow = " v"; break;
                case TrendDirection.Stable: trendArrow = " -"; break;
            }
        }

        // Build final text
        if (showLabel)
        {
            return customLabel + ": " + rateString + trendArrow;
        }
        else
        {
            return rateString + trendArrow;
        }
    }

    string FormatRate(float rate)
    {
        string sign = rate > 0 ? "+" : "";

        switch (displayFormat)
        {
            case RateDisplayFormat.PerSecond:
                return sign + rate.ToString("F1") + "/sec";

            case RateDisplayFormat.PerMinute:
                return sign + rate.ToString("F0") + "/min";

            case RateDisplayFormat.PerHour:
                return sign + rate.ToString("F0") + "/hr";

            default:
                return sign + rate.ToString("F1");
        }
    }

    Color GetDisplayColor()
    {
        if (samples.Count < 2) return noDataColor;

        switch (currentTrend)
        {
            case TrendDirection.Increasing: return increasingColor;
            case TrendDirection.Decreasing: return decreasingColor;
            case TrendDirection.Stable: return stableColor;
            default: return noDataColor;
        }
    }

    // Public methods for external control
    public float GetCurrentRate()
    {
        return currentRate;
    }

    public TrendDirection GetCurrentTrend()
    {
        return currentTrend;
    }

    public void SetDisplayFormat(RateDisplayFormat format)
    {
        displayFormat = format;
        CalculateRate(); // Recalculate with new format
        UpdateDisplay();
    }

    public void SetSampleInterval(float interval)
    {
        sampleInterval = Mathf.Max(0.1f, interval);
    }

    public void SetMaxSamples(int maxSamples)
    {
        this.maxSamples = Mathf.Max(2, maxSamples);

        // Remove excess samples if new max is smaller
        while (samples.Count > this.maxSamples)
        {
            samples.Dequeue();
        }
    }

    public void ResetSamples()
    {
        samples.Clear();
        currentRate = 0f;
        currentTrend = TrendDirection.Stable;
        UpdateDisplay();
    }

    public void ForceUpdate()
    {
        TakeSample();
        CalculateRate();
        UpdateDisplay();
    }

    // For debugging
    [ContextMenu("Reset Rate Tracker")]
    void ResetTracker()
    {
        ResetSamples();
    }

    [ContextMenu("Force Sample")]
    void ForceSample()
    {
        ForceUpdate();
    }
}