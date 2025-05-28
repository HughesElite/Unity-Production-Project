using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExclusiveButtonGroup : MonoBehaviour
{
    [Header("Button References")]
    public Button hotButton;
    public Button mildButton;
    public Button coldButton;

    [Header("Description Text References")]
    public TextMeshProUGUI hotDescriptionText;
    public TextMeshProUGUI mildDescriptionText;
    public TextMeshProUGUI coldDescriptionText;

    [Header("Text Content")]
    [TextArea(2, 3)]
    public string hotSelectedText = "Increases virus transmission\nPeople stay indoors more";
    [TextArea(2, 3)]
    public string mildSelectedText = "Normal virus transmission\nOptimal conditions";
    [TextArea(2, 3)]
    public string coldSelectedText = "Reduces virus transmission\nPeople go outside more";

    [TextArea(2, 3)]
    public string defaultUnselectedText = "Click to select";

    [Header("Text Colors")]
    public Color hotTextColor = Color.red;
    public Color mildTextColor = Color.white;
    public Color coldTextColor = Color.green;
    public Color unselectedTextColor = Color.gray;

    [Header("Border Settings")]
    public Color borderColor = Color.black;
    public Vector2 borderDistance = new Vector2(2, 2);

    [Header("Default Selection")]
    public ButtonType defaultSelected = ButtonType.Mild;

    public enum ButtonType
    {
        Hot,
        Mild,
        Cold
    }

    private Outline hotOutline;
    private Outline mildOutline;
    private Outline coldOutline;

    private ButtonType currentSelected;

    void Start()
    {
        // Setup outlines for each button
        SetupButtonOutline(hotButton, out hotOutline);
        SetupButtonOutline(mildButton, out mildOutline);
        SetupButtonOutline(coldButton, out coldOutline);

        // Connect button click events
        if (hotButton != null)
            hotButton.onClick.AddListener(() => SelectButton(ButtonType.Hot));

        if (mildButton != null)
            mildButton.onClick.AddListener(() => SelectButton(ButtonType.Mild));

        if (coldButton != null)
            coldButton.onClick.AddListener(() => SelectButton(ButtonType.Cold));

        // Initialize all text to unselected state
        InitializeDescriptionTexts();

        // Set default selection
        SelectButton(defaultSelected);
    }

    private void InitializeDescriptionTexts()
    {
        // Set all descriptions to unselected state initially
        if (hotDescriptionText != null)
        {
            hotDescriptionText.text = defaultUnselectedText;
            hotDescriptionText.color = unselectedTextColor;
        }

        if (mildDescriptionText != null)
        {
            mildDescriptionText.text = defaultUnselectedText;
            mildDescriptionText.color = unselectedTextColor;
        }

        if (coldDescriptionText != null)
        {
            coldDescriptionText.text = defaultUnselectedText;
            coldDescriptionText.color = unselectedTextColor;
        }
    }

    private void SetupButtonOutline(Button button, out Outline outline)
    {
        outline = null;

        if (button != null)
        {
            // Add outline component if it doesn't exist
            outline = button.GetComponent<Outline>();
            if (outline == null)
            {
                outline = button.gameObject.AddComponent<Outline>();
            }

            // Configure outline
            outline.effectColor = borderColor;
            outline.effectDistance = borderDistance;
            outline.enabled = false; // Start hidden
        }
    }

    public void SelectButton(ButtonType buttonType)
    {
        // Hide all borders first
        HideAllBorders();

        // Show border for selected button
        switch (buttonType)
        {
            case ButtonType.Hot:
                if (hotOutline != null)
                    hotOutline.enabled = true;
                break;

            case ButtonType.Mild:
                if (mildOutline != null)
                    mildOutline.enabled = true;
                break;

            case ButtonType.Cold:
                if (coldOutline != null)
                    coldOutline.enabled = true;
                break;
        }

        currentSelected = buttonType;

        // Debug feedback
        Debug.Log($"Selected: {buttonType}");

        // Optional: Call different methods based on selection
        OnSelectionChanged(buttonType);
    }

    private void HideAllBorders()
    {
        if (hotOutline != null) hotOutline.enabled = false;
        if (mildOutline != null) mildOutline.enabled = false;
        if (coldOutline != null) coldOutline.enabled = false;
    }

    // Override this method to add custom behavior when selection changes
    protected virtual void OnSelectionChanged(ButtonType selectedButton)
    {
        // Reset all text to unselected state
        ResetAllDescriptionTexts();

        // Update the selected button's text and color
        switch (selectedButton)
        {
            case ButtonType.Hot:
                if (hotDescriptionText != null)
                {
                    hotDescriptionText.text = hotSelectedText;
                    hotDescriptionText.color = hotTextColor;
                }
                // Handle hot weather selection logic here
                break;

            case ButtonType.Mild:
                if (mildDescriptionText != null)
                {
                    mildDescriptionText.text = mildSelectedText;
                    mildDescriptionText.color = mildTextColor;
                }
                // Handle mild weather selection logic here
                break;

            case ButtonType.Cold:
                if (coldDescriptionText != null)
                {
                    coldDescriptionText.text = coldSelectedText;
                    coldDescriptionText.color = coldTextColor;
                }
                // Handle cold weather selection logic here
                break;
        }
    }

    private void ResetAllDescriptionTexts()
    {
        // Set all non-selected buttons to unselected text
        if (hotDescriptionText != null)
        {
            hotDescriptionText.text = defaultUnselectedText;
            hotDescriptionText.color = unselectedTextColor;
        }

        if (mildDescriptionText != null)
        {
            mildDescriptionText.text = defaultUnselectedText;
            mildDescriptionText.color = unselectedTextColor;
        }

        if (coldDescriptionText != null)
        {
            coldDescriptionText.text = defaultUnselectedText;
            coldDescriptionText.color = unselectedTextColor;
        }
    }

    // Public methods for external scripts to use
    public ButtonType GetCurrentSelection()
    {
        return currentSelected;
    }

    public void SelectHot()
    {
        SelectButton(ButtonType.Hot);
    }

    public void SelectMild()
    {
        SelectButton(ButtonType.Mild);
    }

    public void SelectCold()
    {
        SelectButton(ButtonType.Cold);
    }

    public bool IsButtonSelected(ButtonType buttonType)
    {
        return currentSelected == buttonType;
    }

    // Methods to update text content at runtime
    public void UpdateDescriptionTexts(string hot, string mild, string cold)
    {
        hotSelectedText = hot;
        mildSelectedText = mild;
        coldSelectedText = cold;

        // Refresh the current selection's text
        OnSelectionChanged(currentSelected);
    }

    public void UpdateTextColors(Color hot, Color mild, Color cold)
    {
        hotTextColor = hot;
        mildTextColor = mild;
        coldTextColor = cold;

        // Refresh the current selection's color
        OnSelectionChanged(currentSelected);
    }
}