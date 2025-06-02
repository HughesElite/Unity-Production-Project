using UnityEngine;
using TMPro;

public class SimulationClock : MonoBehaviour
{
    [Header("Clock Display")]
    public TextMeshProUGUI clockText;

    [Header("Time Settings")]
    [Range(0.1f, 60f)]
    public float hourDuration = 5f; // How many real seconds = 1 game hour

    [Header("Clock Control")]
    public bool isRunning = true;
    public bool pauseWithGameController = true;

    // Time tracking
    private int currentHour = 1; // 1-24 (1am, 2am, ..., 12pm, 1pm, ..., 12am)
    private int currentDayIndex = 0; // 0 = Monday, 1 = Tuesday, etc.
    private float hourTimer = 0f;
    private GameController gameController;

    private string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    void Start()
    {
        if (pauseWithGameController)
        {
            gameController = GameController.Instance;
        }

        if (clockText == null)
            clockText = GetComponent<TextMeshProUGUI>();

        if (clockText == null)
        {
            Debug.LogError("SimulationClock: No TextMeshPro component found!");
            enabled = false;
            return;
        }

        UpdateDisplay();
    }

    void Update()
    {
        if (!isRunning) return;
        if (pauseWithGameController && gameController != null && gameController.IsGamePaused()) return;

        hourTimer += Time.deltaTime;

        if (hourTimer >= hourDuration)
        {
            hourTimer = 0f;
            AdvanceHour();
        }
    }

    void AdvanceHour()
    {
        currentHour++;

        // After 12am (24), goes to 1am and advances day
        if (currentHour > 24)
        {
            currentHour = 1;
            currentDayIndex++;

            // Cycles back to Monday after Sunday
            if (currentDayIndex >= dayNames.Length)
            {
                currentDayIndex = 0;
            }
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (clockText == null) return;

        string timeString = GetTimeString();
        clockText.text = $"{GetCurrentDayName()}\n{timeString}";
    }

    string GetTimeString()
    {
        // Convert our 1-24 system to 12-hour format
        if (currentHour <= 12)
        {
            // 1am - 12pm
            if (currentHour == 12)
                return "12pm"; // Noon
            else
                return $"{currentHour}am";
        }
        else
        {
            // 1pm - 12am
            if (currentHour == 24)
                return "12am"; // Midnight
            else
                return $"{currentHour - 12}pm";
        }
    }

    // Public methods
    public void ResetClock()
    {
        currentHour = 1;
        currentDayIndex = 0;
        hourTimer = 0f;
        UpdateDisplay();
    }

    public void SetupTwoMinuteWeek()
    {
        // 2 minutes = 120 seconds, 7 days × 24 hours = 168 hours
        hourDuration = 120f / 168f; // About 0.71 seconds per hour
        Debug.Log($"Set up 2-minute week: {hourDuration:F2} seconds per hour");
    }

    public string GetCurrentDayName()
    {
        return dayNames[currentDayIndex];
    }

    public int GetCurrentHour()
    {
        return currentHour;
    }

    [ContextMenu("Setup 2-Minute Week")]
    void Setup2MinuteWeek()
    {
        SetupTwoMinuteWeek();
    }
}