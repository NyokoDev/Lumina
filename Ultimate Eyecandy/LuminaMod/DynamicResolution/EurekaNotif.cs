using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using ColossalFramework.UI;

public class EurekaNotif : ListNotification
{
    // Don't Show Again button.
    internal UIButton _noButton;
    internal UIButton _yesButton;

    /// <summary>
    /// Gets the 'No' button (button 1) instance.
    /// </summary>
    public UIButton NoButton => _noButton;

    /// <summary>
    /// Gets the 'Yes' button (button 2) instance.
    /// </summary>
    public UIButton YesButton => _yesButton;

    /// <summary>
    /// Gets the number of buttons for this panel (for layout).
    /// </summary>
    protected override int NumButtons => 1;



    /// <summary>
    /// Adds buttons to the message box.
    /// </summary>
    public override void AddButtons()
    {

        // Add yes button.
        _yesButton = AddButton(1, NumButtons, Translations.Translate("UnlockSlider"), Close);

    }
}
