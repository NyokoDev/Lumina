using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using System.Diagnostics;
using System.Reflection;
using UnifiedUI.GUI;

namespace Lumina.OptionsTabs
{
    /// <summary>
    /// MainTab class.
    /// </summary>
    public sealed class MainTab
    {
        private UITabstrip tabStrip;
        private int v;
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;
        private const float LabelWidth = 40f;
        private const float TabHeight = 20f;
        public static UIPanel panel;


        internal MainTab(UITabstrip tabStrip, int tabIndex)
        {
           
            panel = UITabstrips.AddTextTab(tabStrip, "Main", tabIndex, out UIButton _, autoLayout: false);
            float currentY = Margin;
            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(panel, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (c, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            currentY += languageDropDown.parent.height + GroupMargin;

            // Hotkey control.
            OptionsKeymapping uuiKeymapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("HOTKEY"), ModSettings.ToggleKey.Keybinding);
            currentY += uuiKeymapping.Panel.height + GroupMargin;

            UIButton supportbutton = UIButtons.AddSmallerButton(panel, LeftMargin, currentY, "Support");
            currentY += 50f;
            supportbutton.eventClicked += (sender, args) =>
            {
                Process.Start("https://discord.gg/gdhyhfcj7A");
            };

            UIButton guidesbutton = UIButtons.AddSmallerButton(panel, LeftMargin, currentY, "Guides & Help");
            currentY += 50f;
            guidesbutton.eventClicked += (sender, args) =>
            {
                Process.Start("https://cslmods.wikitide.org/wiki/Guide_for_Lumina");
            };



            UILabel version = UILabels.AddLabel(panel, LabelWidth, currentY, Assembly.GetExecutingAssembly().GetName().Version.ToString(), textScale: 0.7f, alignment: UIHorizontalAlignment.Center);
        }
    }
}