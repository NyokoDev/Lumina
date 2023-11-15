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
