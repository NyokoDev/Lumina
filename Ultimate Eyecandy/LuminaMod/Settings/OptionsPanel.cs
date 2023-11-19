namespace Lumina
{
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEngine;
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
        private const float LabelWidth = 40f;
        private const float TabHeight = 20f;

        /// <summary>
        /// Performs on-demand panel setup.
        /// </summary>
        protected override void Setup()
        {
            autoLayout = false;
            float currentY = Margin;
            m_BackgroundSprite = "UnlockingPanel";

            UIPanel panel = this;
           

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


            UILabel miscellanous = UILabels.AddLabel(this, LabelWidth, currentY, "Lumina | Shader Configurations", textScale: 0.9f, alignment: UIHorizontalAlignment.Center);
            currentY += 31f;
            UILabel miscellanous2 = UILabels.AddLabel(this, LabelWidth, currentY, Translations.Translate("SHADER_CONFIG"), textScale: 0.5f, alignment: UIHorizontalAlignment.Center);
            currentY += 30f;

            UILabel enable = UILabels.AddLabel(this, LabelWidth, currentY, Translations.Translate("RESTART_TEXT"), textScale: 0.7f, alignment: UIHorizontalAlignment.Center);
            currentY += 35f;

            string status = LuminaLogic.DynResEnabled ? "Enabled" : "Disabled";

            UICheckBox enableDRbutton = UICheckBoxes.AddLabelledCheckBox(this, LeftMargin, currentY, Translations.Translate("IGNORECheckbox"), 16, (float)0.8, null);
            currentY += 35f;
            enableDRbutton.isChecked = LuminaLogic.DynResEnabled;
          

            UILabel drstatus = UILabels.AddLabel(this, LabelWidth, currentY, Translations.Translate("STATUS_LABEL") + status, textScale: 0.8f, alignment: UIHorizontalAlignment.Center);
            drstatus.color = UnityEngine.Color.black;
          
            currentY += 50f;
            enableDRbutton.eventClicked += (sender, args) =>
            {
                LuminaLogic.DynResEnabled = !LuminaLogic.DynResEnabled;
                var value = LuminaLogic.DynResEnabled ? "Enabled" : "Disabled";
                drstatus.text = Translations.Translate("STATUS_LABEL") + value;
                ModSettings.Save();
            };

            UILabel version = UILabels.AddLabel(this, LabelWidth, currentY, Assembly.GetExecutingAssembly().GetName().Version.ToString(), textScale: 0.7f, alignment: UIHorizontalAlignment.Center);



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
