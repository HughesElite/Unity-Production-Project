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
    public int currentHour = 1; // 1-24 (1am, 2am... 12pm, 1pm...)
    public int currentDayIndex = 0; // 0 = Monday, 1 = Tuesday, etc.
    public float hourTimer = 0f;

    public GameController gameController;
    public string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

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

    public string GetTimeString()
    {
        // Convert our 1-24 system to 12-hour format
        if (currentHour <= 12)
        {
            if (currentHour == 12)
                return "12pm"; // Noon
            else
                return $"{currentHour}am";
        }
        else
        {
            if (currentHour == 24)
                return "12am"; // Midnight
            else
                return $"{currentHour - 12}pm";
        }
    }

    public void ResetClock()
    {
        currentHour = 1;
        currentDayIndex = 0;
        hourTimer = 0f;
        UpdateDisplay();
    }

    //public void SetupThreeMinuteWeek()
    //{
    //    // 3 minutes = 180 seconds, 1 week = 168 hours
    //    // About 1.07 seconds per hour

    //    hourDuration = 180F / 168f; 
    //}

    public string GetCurrentDayName()
    {
        return dayNames[currentDayIndex];
    }

    public int GetCurrentHour()
    {
        return currentHour;
    }
}