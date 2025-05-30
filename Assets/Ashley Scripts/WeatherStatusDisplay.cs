using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonMessageDisplay : MonoBehaviour
{
    [Header("Button References")]
    public Button hotButton;
    public Button mildButton;
    public Button coldButton;

    [Header("Text Component")]
    public TextMeshProUGUI messageText;

    [Header("Custom Messages")]
    [TextArea(2, 4)]
    public string hotMessage = "Hot weather selected!\nVirus spreads faster indoors.";
    [TextArea(2, 4)]
    public string mildMessage = "Mild weather selected!\nNormal transmission conditions.";
    [TextArea(2, 4)]
    public string coldMessage = "Cold weather selected!\nVirus spreads slower outdoors.";

    [Header("Display Options")]
    public bool showLabel = false;
    public string customLabel = "Weather Status";

    [Header("Default Selection")]
    public ButtonType defaultSelected = ButtonType.Mild;

    public enum ButtonType
    {
        Hot,
        Mild,
        Cold
    }

    private ButtonType currentSelection;

    void Start()
    {
        // Get TextMeshPro component if not assigned
        if (messageText == null)
            messageText = GetComponent<TextMeshProUGUI>();

        if (messageText == null)
        {
            Debug.LogError("ButtonMessageDisplay: No TextMeshPro component found! Please assign one or attach this script to a TextMeshPro object.");
            enabled = false;
            return;
        }

        // Connect button click events
        SetupButtonListeners();

        // Set default selection
        currentSelection = defaultSelected;
        UpdateMessage();
    }

    void SetupButtonListeners()
    {
        if (hotButton != null)
            hotButton.onClick.AddListener(() => OnButtonClicked(ButtonType.Hot));

        if (mildButton != null)
            mildButton.onClick.AddListener(() => OnButtonClicked(ButtonType.Mild));

        if (coldButton != null)
            coldButton.onClick.AddListener(() => OnButtonClicked(ButtonType.Cold));
    }

    void OnButtonClicked(ButtonType buttonType)
    {
        currentSelection = buttonType;
        UpdateMessage();
        Debug.Log("Button clicked: " + buttonType);
    }

    void UpdateMessage()
    {
        if (messageText == null) return;

        string message = GetMessageForSelection(currentSelection);

        if (showLabel)
        {
            messageText.text = customLabel + ": " + message;
        }
        else
        {
            messageText.text = message;
        }
    }

    string GetMessageForSelection(ButtonType selection)
    {
        switch (selection)
        {
            case ButtonType.Hot:
                return hotMessage;

            case ButtonType.Mild:
                return mildMessage;

            case ButtonType.Cold:
                return coldMessage;

            default:
                return "No selection";
        }
    }

    // Public methods for external control
    public void SetMessages(string hot, string mild, string cold)
    {
        hotMessage = hot;
        mildMessage = mild;
        coldMessage = cold;
        UpdateMessage(); // Refresh display with new messages
    }

    public void SetHotMessage(string message)
    {
        hotMessage = message;
        if (currentSelection == ButtonType.Hot)
            UpdateMessage();
    }

    public void SetMildMessage(string message)
    {
        mildMessage = message;
        if (currentSelection == ButtonType.Mild)
            UpdateMessage();
    }

    public void SetColdMessage(string message)
    {
        coldMessage = message;
        if (currentSelection == ButtonType.Cold)
            UpdateMessage();
    }

    public ButtonType GetCurrentSelection()
    {
        return currentSelection;
    }

    public void SetLabel(string newLabel)
    {
        customLabel = newLabel;
        UpdateMessage();
    }

    public void ForceUpdate()
    {
        UpdateMessage();
    }

    // Public methods to programmatically select buttons
    public void SelectHot()
    {
        OnButtonClicked(ButtonType.Hot);
    }

    public void SelectMild()
    {
        OnButtonClicked(ButtonType.Mild);
    }

    public void SelectCold()
    {
        OnButtonClicked(ButtonType.Cold);
    }

    // For debugging
    [ContextMenu("Force Message Update")]
    void ForceMessageUpdate()
    {
        ForceUpdate();
        Debug.Log("Current selection: " + currentSelection + " - Message: " + GetMessageForSelection(currentSelection));
    }

    [ContextMenu("Test All Messages")]
    void TestAllMessages()
    {
        Debug.Log("Hot: " + hotMessage);
        Debug.Log("Mild: " + mildMessage);
        Debug.Log("Cold: " + coldMessage);
    }

    void OnDestroy()
    {
        // Clean up button listeners to prevent memory leaks
        if (hotButton != null)
            hotButton.onClick.RemoveAllListeners();
        if (mildButton != null)
            mildButton.onClick.RemoveAllListeners();
        if (coldButton != null)
            coldButton.onClick.RemoveAllListeners();
    }
}