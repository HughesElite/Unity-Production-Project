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
}