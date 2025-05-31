using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIResetManager : MonoBehaviour
{
    [Header("UI Components to Reset")]
    public SimulationClock simulationClock;
    public WeatherButtonGroup weatherButtonGroup;
    public WeatherStatusDisplay weatherStatusDisplay;
    public RValueCalculator rValueCalculator;
    public PeakInfectionTracker peakInfectionTracker;
    public InfectionRateTracker infectionRateTracker;
    public PopulationCounter populationCounter;
    public VirusStatsDisplay virusStatsDisplay;
    public OutbreakStatusTracker outbreakStatusTracker;
    public NPCPopulationController populationController;

    [Header("Reset Settings")]
    public bool resetToDefaultPopulation = true;
    public int defaultPopulation = 10;
    public bool resetWeatherToMild = true;

    [Header("UI References")]
    [Tooltip("Multiple buttons that can trigger UI reset")]
    public Button[] resetUIButtons;
    [Tooltip("Legacy single button support - will be added to array if set")]
    public Button legacyResetButton;

    [Header("Input Field References")]
    [Tooltip("TMP InputFields that trigger UI reset when used")]
    public TMP_InputField[] resetInputFields;
    [Tooltip("When should InputFields trigger reset?")]
    public InputFieldTrigger inputFieldTrigger = InputFieldTrigger.OnEndEdit;

    public enum InputFieldTrigger
    {
        OnEndEdit,      // When user presses Enter or clicks away
        OnValueChanged  // Every time the value changes (while typing)
    }

    [Header("Debug")]
    public bool debugMode = false;

    void Start()
    {
        // Auto-find components if not assigned
        AutoFindComponents();

        // Setup reset buttons
        SetupResetButtons();

        // Setup reset input fields
        SetupResetInputFields();
    }

    void AutoFindComponents()
    {
        // Auto-find components if not manually assigned
        if (simulationClock == null)
            simulationClock = FindFirstObjectByType<SimulationClock>();

        if (weatherButtonGroup == null)
            weatherButtonGroup = FindFirstObjectByType<WeatherButtonGroup>();

        if (weatherStatusDisplay == null)
            weatherStatusDisplay = FindFirstObjectByType<WeatherStatusDisplay>();

        if (rValueCalculator == null)
            rValueCalculator = FindFirstObjectByType<RValueCalculator>();

        if (peakInfectionTracker == null)
            peakInfectionTracker = FindFirstObjectByType<PeakInfectionTracker>();

        if (infectionRateTracker == null)
            infectionRateTracker = FindFirstObjectByType<InfectionRateTracker>();

        if (populationCounter == null)
            populationCounter = FindFirstObjectByType<PopulationCounter>();

        if (virusStatsDisplay == null)
            virusStatsDisplay = FindFirstObjectByType<VirusStatsDisplay>();

        if (outbreakStatusTracker == null)
            outbreakStatusTracker = FindFirstObjectByType<OutbreakStatusTracker>();

        if (populationController == null)
            populationController = FindFirstObjectByType<NPCPopulationController>();

        if (debugMode)
        {
            int foundComponents = 0;
            if (simulationClock != null) foundComponents++;
            if (weatherButtonGroup != null) foundComponents++;
            if (weatherStatusDisplay != null) foundComponents++;
            if (rValueCalculator != null) foundComponents++;
            if (peakInfectionTracker != null) foundComponents++;
            if (infectionRateTracker != null) foundComponents++;
            if (populationCounter != null) foundComponents++;
            if (virusStatsDisplay != null) foundComponents++;
            if (outbreakStatusTracker != null) foundComponents++;
            if (populationController != null) foundComponents++;

            Debug.Log($"Auto-found {foundComponents} UI components");
        }
    }

    void SetupResetButtons()
    {
        // Create list to collect all buttons
        System.Collections.Generic.List<Button> allButtons = new System.Collections.Generic.List<Button>();

        // Add buttons from array
        if (resetUIButtons != null)
        {
            foreach (Button btn in resetUIButtons)
            {
                if (btn != null && !allButtons.Contains(btn))
                    allButtons.Add(btn);
            }
        }

        // Add legacy button if set
        if (legacyResetButton != null && !allButtons.Contains(legacyResetButton))
        {
            allButtons.Add(legacyResetButton);
        }

        // Auto-assign button if no buttons set and this script is on a button
        if (allButtons.Count == 0)
        {
            Button selfButton = GetComponent<Button>();
            if (selfButton != null)
            {
                allButtons.Add(selfButton);
            }
        }

        // Subscribe to all button click events
        foreach (Button btn in allButtons)
        {
            if (btn != null)
            {
                btn.onClick.AddListener(ResetAllUI);
                if (debugMode)
                    Debug.Log($"UI Reset button assigned: {btn.name}");
            }
        }

        // Update the array to include all found buttons
        resetUIButtons = allButtons.ToArray();

        if (debugMode)
        {
            if (allButtons.Count > 0)
                Debug.Log($"Total UI Reset buttons assigned: {allButtons.Count}");
            else
                Debug.LogWarning("No UI reset buttons assigned. You can still call ResetAllUI() manually.");
        }
    }

    void SetupResetInputFields()
    {
        if (resetInputFields == null) return;

        int validInputFields = 0;

        foreach (TMP_InputField inputField in resetInputFields)
        {
            if (inputField != null)
            {
                // Remove any existing listeners to prevent duplicates
                inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
                inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);

                // Add appropriate listener based on trigger setting
                switch (inputFieldTrigger)
                {
                    case InputFieldTrigger.OnEndEdit:
                        inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
                        break;

                    case InputFieldTrigger.OnValueChanged:
                        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
                        break;
                }

                validInputFields++;

                if (debugMode)
                    Debug.Log($"UI Reset input field assigned: {inputField.name} (Trigger: {inputFieldTrigger})");
            }
        }

        if (debugMode)
        {
            if (validInputFields > 0)
                Debug.Log($"Total UI Reset input fields assigned: {validInputFields}");
            else
                Debug.Log("No UI reset input fields assigned.");
        }
    }

    void OnInputFieldEndEdit(string value)
    {
        ResetAllUI();
        if (debugMode)
            Debug.Log($"UI Reset triggered by input field (onEndEdit): '{value}'");
    }

    void OnInputFieldValueChanged(string value)
    {
        ResetAllUI();
        if (debugMode)
            Debug.Log($"UI Reset triggered by input field (onValueChanged): '{value}'");
    }

    /// <summary>
    /// Resets all UI elements to their initial states
    /// </summary>
    public void ResetAllUI()
    {
        ResetClock();
        ResetWeather();
        ResetStatistics();
        ResetPopulation();

        if (debugMode)
            Debug.Log("All UI elements have been reset!");
    }

    /// <summary>
    /// Reset simulation clock to starting time
    /// </summary>
    public void ResetClock()
    {
        if (simulationClock != null)
        {
            simulationClock.ResetClock();
            if (debugMode)
                Debug.Log("Clock reset to starting time");
        }
    }

    /// <summary>
    /// Reset weather selection to default (Mild)
    /// </summary>
    public void ResetWeather()
    {
        if (resetWeatherToMild)
        {
            if (weatherButtonGroup != null)
            {
                weatherButtonGroup.SelectMild();
                if (debugMode)
                    Debug.Log("Weather reset to Mild");
            }

            if (weatherStatusDisplay != null)
            {
                weatherStatusDisplay.SelectMild();
                if (debugMode)
                    Debug.Log("Weather status display reset to Mild");
            }
        }
    }

    /// <summary>
    /// Reset all tracking statistics
    /// </summary>
    public void ResetStatistics()
    {
        // Reset R-Value tracking
        if (rValueCalculator != null)
        {
            rValueCalculator.ResetSamples();
            if (debugMode)
                Debug.Log("R-Value tracker reset");
        }

        // Reset peak infection tracking
        if (peakInfectionTracker != null)
        {
            peakInfectionTracker.ResetPeak();
            if (debugMode)
                Debug.Log("Peak infection tracker reset");
        }

        // Reset infection rate tracking
        if (infectionRateTracker != null)
        {
            infectionRateTracker.ResetSamples();
            if (debugMode)
                Debug.Log("Infection rate tracker reset");
        }

        // Force update population counter
        if (populationCounter != null)
        {
            populationCounter.ForceUpdate();
            if (debugMode)
                Debug.Log("Population counter refreshed");
        }

        // Force update virus stats display
        if (virusStatsDisplay != null)
        {
            virusStatsDisplay.ForceUpdate();
            if (debugMode)
                Debug.Log("Virus stats display refreshed");
        }

        // Force update outbreak status tracker
        if (outbreakStatusTracker != null)
        {
            outbreakStatusTracker.ForceUpdate();
            if (debugMode)
                Debug.Log("Outbreak status tracker refreshed");
        }
    }

    /// <summary>
    /// Reset population to default value
    /// </summary>
    public void ResetPopulation()
    {
        if (resetToDefaultPopulation && populationController != null)
        {
            populationController.ResetToDefault();
            if (debugMode)
                Debug.Log($"Population reset to default: {defaultPopulation}");
        }
    }

    /// <summary>
    /// Reset only the clock
    /// </summary>
    [ContextMenu("Reset Clock Only")]
    public void ResetClockOnly()
    {
        ResetClock();
    }

    /// <summary>
    /// Reset only weather settings
    /// </summary>
    [ContextMenu("Reset Weather Only")]
    public void ResetWeatherOnly()
    {
        ResetWeather();
    }

    /// <summary>
    /// Reset only statistics tracking
    /// </summary>
    [ContextMenu("Reset Statistics Only")]
    public void ResetStatisticsOnly()
    {
        ResetStatistics();
    }

    /// <summary>
    /// Get summary of current UI states for debugging
    /// </summary>
    [ContextMenu("Log UI Status")]
    public void LogUIStatus()
    {
        if (debugMode)
        {
            Debug.Log("=== UI Status Summary ===");

            if (simulationClock != null)
                Debug.Log($"Clock: {simulationClock.GetCurrentDayName()} {simulationClock.GetCurrentHour()}:00");

            if (weatherButtonGroup != null)
                Debug.Log($"Weather: {weatherButtonGroup.GetCurrentSelection()}");

            if (rValueCalculator != null)
                Debug.Log($"R-Value: {rValueCalculator.GetCurrentRValue():F2}");

            if (peakInfectionTracker != null)
                Debug.Log($"Peak Infections: {peakInfectionTracker.GetPeakInfections()}");

            if (infectionRateTracker != null)
                Debug.Log($"Infection Rate: {infectionRateTracker.GetCurrentRate():F1} ({infectionRateTracker.GetCurrentTrend()})");

            if (populationController != null)
                Debug.Log($"Population: {populationController.GetCurrentPopulation()}");
        }
    }

    /// <summary>
    /// Check if all required components are found
    /// </summary>
    public bool AllComponentsFound()
    {
        return simulationClock != null &&
               weatherButtonGroup != null &&
               weatherStatusDisplay != null &&
               rValueCalculator != null &&
               peakInfectionTracker != null &&
               infectionRateTracker != null &&
               populationCounter != null &&
               virusStatsDisplay != null &&
               outbreakStatusTracker != null &&
               populationController != null;
    }

    /// <summary>
    /// Add a button to the reset buttons array at runtime
    /// </summary>
    public void AddResetButton(Button button)
    {
        if (button == null) return;

        // Create new array with additional space
        System.Collections.Generic.List<Button> buttonList = new System.Collections.Generic.List<Button>();

        if (resetUIButtons != null)
            buttonList.AddRange(resetUIButtons);

        // Add new button if not already in list
        if (!buttonList.Contains(button))
        {
            buttonList.Add(button);
            button.onClick.AddListener(ResetAllUI);

            if (debugMode)
                Debug.Log($"Added reset button: {button.name}");
        }

        resetUIButtons = buttonList.ToArray();
    }

    /// <summary>
    /// Remove a button from the reset buttons array
    /// </summary>
    public void RemoveResetButton(Button button)
    {
        if (button == null || resetUIButtons == null) return;

        System.Collections.Generic.List<Button> buttonList = new System.Collections.Generic.List<Button>(resetUIButtons);

        if (buttonList.Remove(button))
        {
            button.onClick.RemoveListener(ResetAllUI);
            resetUIButtons = buttonList.ToArray();

            if (debugMode)
                Debug.Log($"Removed reset button: {button.name}");
        }
    }

    /// <summary>
    /// Add an input field to the reset input fields array at runtime
    /// </summary>
    public void AddResetInputField(TMP_InputField inputField)
    {
        if (inputField == null) return;

        // Create new array with additional space
        System.Collections.Generic.List<TMP_InputField> inputFieldList = new System.Collections.Generic.List<TMP_InputField>();

        if (resetInputFields != null)
            inputFieldList.AddRange(resetInputFields);

        // Add new input field if not already in list
        if (!inputFieldList.Contains(inputField))
        {
            inputFieldList.Add(inputField);

            // Add appropriate listener based on current trigger setting
            switch (inputFieldTrigger)
            {
                case InputFieldTrigger.OnEndEdit:
                    inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
                    break;
                case InputFieldTrigger.OnValueChanged:
                    inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
                    break;
            }

            if (debugMode)
                Debug.Log($"Added reset input field: {inputField.name}");
        }

        resetInputFields = inputFieldList.ToArray();
    }

    /// <summary>
    /// Remove an input field from the reset input fields array
    /// </summary>
    public void RemoveResetInputField(TMP_InputField inputField)
    {
        if (inputField == null || resetInputFields == null) return;

        System.Collections.Generic.List<TMP_InputField> inputFieldList = new System.Collections.Generic.List<TMP_InputField>(resetInputFields);

        if (inputFieldList.Remove(inputField))
        {
            inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
            inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            resetInputFields = inputFieldList.ToArray();

            if (debugMode)
                Debug.Log($"Removed reset input field: {inputField.name}");
        }
    }

    /// <summary>
    /// Change the input field trigger type and update all listeners
    /// </summary>
    public void SetInputFieldTrigger(InputFieldTrigger newTrigger)
    {
        if (inputFieldTrigger == newTrigger) return;

        inputFieldTrigger = newTrigger;

        // Re-setup all input fields with new trigger
        SetupResetInputFields();

        if (debugMode)
            Debug.Log($"Input field trigger changed to: {newTrigger}");
    }

    void OnDestroy()
    {
        // Clean up all button listeners
        if (resetUIButtons != null)
        {
            foreach (Button btn in resetUIButtons)
            {
                if (btn != null)
                {
                    btn.onClick.RemoveListener(ResetAllUI);
                }
            }
        }

        // Clean up legacy button listener
        if (legacyResetButton != null)
        {
            legacyResetButton.onClick.RemoveListener(ResetAllUI);
        }

        // Clean up input field listeners
        if (resetInputFields != null)
        {
            foreach (TMP_InputField inputField in resetInputFields)
            {
                if (inputField != null)
                {
                    inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
                    inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
                }
            }
        }
    }
}