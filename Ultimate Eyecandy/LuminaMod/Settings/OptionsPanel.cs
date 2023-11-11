namespace Lumina
{
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using static Lumina.EffectsTab;

    /// <summary>
    /// The mod's settings options panel.
    /// </summary>
    public sealed class OptionsPanel : OptionsPanelBase
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;

        /// <summary>
        /// Performs on-demand panel setup.
        /// </summary>
        protected override void Setup()
        {
            autoLayout = false;
            float currentY = Margin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(this, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (c, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            currentY += languageDropDown.parent.height + GroupMargin;

            // Hotkey control.
            OptionsKeymapping uuiKeymapping = OptionsKeymapping.AddKeymapping(this, LeftMargin, currentY, Translations.Translate("HOTKEY"), ModSettings.ToggleKey.Keybinding);
            currentY += uuiKeymapping.Panel.height + GroupMargin;

            UICheckBox CompatibilityHelper = UICheckBoxes.AddLabelledCheckBox(this, LeftMargin, currentY, Translations.Translate("IGNORECheckbox"), 16, (float)0.8, null); // Approach for compatibility control

            if (LuminaLogic.CompatibilityDisabled == true)
            {
                CompatibilityHelper.isChecked = true;
            }
            else
            {
                CompatibilityHelper.isChecked = false;
            }

            CompatibilityHelper.eventCheckChanged += (c, index) =>
            {
                IgnoreWarningNotif notification = NotificationBase.ShowNotification<IgnoreWarningNotif>();
                notification.AddParas("Ignore compatibiity notices? Ignoring compatibility notices can lead to overlapping functions between other mods and potential problems between mods that do the same thing. Would you like to proceed?");

                notification._yesButton.eventClicked += (sender, args) =>
                {
                    LuminaLogic.CompatibilityDisabled = true;
                    CompatibilityHelper.isChecked = true;
                };
                notification._noButton.eventClicked += (sender, args) =>
                {
                    LuminaLogic.CompatibilityDisabled = false;
                    CompatibilityHelper.isChecked = false;
                };


            };
            
         
        }

        /// <summary>
        /// Provides a panel to alert the user of ignoring compatibility notices.
        /// </summary>
        private void IgnoreWarning()
        {
            
        }
        public class IgnoreWarningNotif : ListNotification
        {
            // Don't Show Again button.
            public UIButton _noButton;
            public UIButton _yesButton;

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
            protected override int NumButtons => 2;

            /// <summary>
            /// Adds buttons to the message box.
            /// </summary>
            public override void AddButtons()
            {

                _yesButton = AddButton(1, NumButtons, Translations.Translate("Turn on the compatibility-disabled mode"), Close);
      
                _noButton = AddButton(2, NumButtons, Translations.Translate("Enable the default compatibility settings."), Close);

            
                
            }
        }
    }
}
