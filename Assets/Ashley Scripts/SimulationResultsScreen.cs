using UnityEngine;
using TMPro;

public class SimulationResultsScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject resultsPanel; // Panel to show/hide results
    public TextMeshProUGUI resultsText; // Single text component for all results

    [Header("Display Options - Check What You Want")]
    [Space(10)]
    public bool showAttackRate = true;
    public bool showPeakInfections = true;
    public bool showPeakTime = true;
    public bool showFastestSpread = true;
    public bool showHighestRValue = true;
    public bool showHighestRTime = true;
    public bool showAvgDuration = true;

    [Header("Tracker References")]
    public PeakInfectionTracker peakTracker;
    public RValueCalculator rValueTracker;
    public InfectionRateTracker infectionRateTracker;
    public OutbreakStatusTracker outbreakTracker;
    public SimulationClock simulationClock;

    [Header("Auto-Show Settings")]
    public bool showAtSpecificTime = true;
    public bool showOnAllInfectionsCleared = false;
    public bool pauseGameWhenShown = true; // NEW: Pause game when results appear

    [Header("Pause Integration")]
    public PauseOnlyButton pauseButton; // Reference to your pause script

    [Header("Clock-Based Trigger")]
    public string endDay = "Sunday";
    [Range(0, 24)]
    public int endHour = 23;
    public bool showEndHourAndLater = true;

    [Header("Testing")]
    public bool enableTestMode = false;
    public float testDelay = 20f;

    [Header("Results Formatting")]
    public bool convertRealTimeToGameTime = true;

    void Start()
    {
        // Auto-find components if not assigned
        if (peakTracker == null)
            peakTracker = FindFirstObjectByType<PeakInfectionTracker>();

        if (rValueTracker == null)
            rValueTracker = FindFirstObjectByType<RValueCalculator>();

        if (infectionRateTracker == null)
            infectionRateTracker = FindFirstObjectByType<InfectionRateTracker>();

        if (outbreakTracker == null)
            outbreakTracker = FindFirstObjectByType<OutbreakStatusTracker>();

        if (simulationClock == null)
            simulationClock = FindFirstObjectByType<SimulationClock>();

        // Auto-find pause button if not assigned
        if (pauseButton == null)
            pauseButton = FindFirstObjectByType<PauseOnlyButton>();

        // Hide results panel on start
        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        // Add test functionality
        if (enableTestMode)
        {
            Invoke(nameof(ShowResultsTest), testDelay);
        }
    }

    void Update()
    {
        // Don't show if already showing
        if (resultsPanel != null && resultsPanel.activeInHierarchy) return;

        // Clock-based trigger
        if (showAtSpecificTime && simulationClock != null)
        {
            string currentDay = simulationClock.GetCurrentDayName();
            int currentHour = simulationClock.GetCurrentHour();

            bool dayMatches = currentDay.Equals(endDay, System.StringComparison.OrdinalIgnoreCase);
            bool timeMatches = showEndHourAndLater ? (currentHour >= endHour) : (currentHour == endHour);

            if (dayMatches && timeMatches)
            {
                Debug.Log($"Simulation results triggered at {currentDay} {currentHour}:00 (Target: {endDay} {endHour}:00)");
                ShowResults();
                return;
            }
        }

        // Infection-based trigger (secondary option)
        if (showOnAllInfectionsCleared)
        {
            int currentInfected = VirusSimulation.GetInfectedCount();
            int totalRecovered = VirusSimulation.GetTotalRecoveredCount();

            if (currentInfected == 0 && totalRecovered > 0)
            {
                Debug.Log("All infections cleared - showing results");
                ShowResults();
                return;
            }
        }
    }

    void ShowResultsTest()
    {
        Debug.Log("Testing results screen after " + testDelay + " seconds");
        ShowResults();
    }

    public void ShowResults()
    {
        if (resultsPanel == null || resultsText == null)
        {
            Debug.LogError("SimulationResultsScreen: Missing UI components!");
            return;
        }

        // Pause the game if enabled
        if (pauseGameWhenShown && pauseButton != null)
        {
            pauseButton.PauseGameIfNotPaused();
            Debug.Log("Game paused when results screen appeared");
        }

        // Build results text based on what's enabled
        string results = BuildCustomResultsText();

        // Display the results
        resultsText.text = results;
        resultsPanel.SetActive(true);

        Debug.Log("Simulation results displayed:\n" + results);
    }

    string BuildCustomResultsText()
    {
        string results = "Simulation Results\n\n";
        // Calculate basic statistics
        int totalPopulation = VirusSimulation.GetTotalNPCCount();
        int currentInfected = VirusSimulation.GetInfectedCount();
        int currentRecovered = VirusSimulation.GetRecoveredCount();

       
        int totalEverInfected = VirusSimulation.GetTotalRecoveredCount();
        float attackRate = totalPopulation > 0 ? ((float)totalEverInfected / totalPopulation) * 100f : 0f;

        // Add enabled statistics
        if (showAttackRate)
            results += $"Attack Rate: {attackRate:F0}% of population infected\n";

        // Peak infection info
        if (showPeakInfections && peakTracker != null)
        {
            int peakCount = peakTracker.GetPeakInfections();
            results += $"Peak Infections: {peakCount}\n";
        }

        if (showPeakTime && peakTracker != null)
        {
            string peakTime = peakTracker.GetPeakTime();
            if (peakTime != "Not reached")
                results += $"Peak Occurred: {peakTime}\n";
            else
                results += "Peak Time: Not reached\n";
        }

        // Spread analysis
        if (showFastestSpread && infectionRateTracker != null)
        {
            string fastestPeriod = infectionRateTracker.GetFastestSpreadPeriod();
            if (!string.IsNullOrEmpty(fastestPeriod))
                results += $"Fastest Spread: {fastestPeriod}\n";
            else
                results += "Fastest Spread: No rapid spread detected\n";
        }

        // R-Value info
        if (showHighestRValue && rValueTracker != null)
        {
            float highestR = rValueTracker.GetHighestRValue();
            results += $"Highest R-Value: {highestR:F1}\n";
        }

        if (showHighestRTime && rValueTracker != null)
        {
            string rTime = rValueTracker.GetHighestRValueTime();
            if (!string.IsNullOrEmpty(rTime))
                results += $"R-Value Peak Time: {rTime}\n";
            else
                results += "R-Value Peak Time: Unknown\n";
        }

        // Duration info
        if (showAvgDuration)
        {
            float avgDuration = VirusSimulation.GetAverageInfectionDuration();
            int totalRecovered = VirusSimulation.GetTotalRecoveredCount();

            if (totalRecovered == 0)
            {
                results += "Average Duration: No data available\n";
            }
            else if (convertRealTimeToGameTime && simulationClock != null)
            {
                float gameDays = avgDuration / (24f * 1.071f);
                results += $"Average Infection Duration: {gameDays:F1} game days\n";
            }
            else
            {
                results += $"Average Infection Duration: {avgDuration:F1} seconds\n";
            }
        }

        return results;
    }

    public void HideResults()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    // Public methods for manual control
    [ContextMenu("Show Results")]
    public void ShowResultsManual()
    {
        ShowResults();
    }

    [ContextMenu("Hide Results")]
    public void HideResultsManual()
    {
        HideResults();
    }
}