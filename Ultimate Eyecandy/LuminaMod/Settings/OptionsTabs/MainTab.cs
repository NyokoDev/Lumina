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
        string[] UIStyles = new string[] { "Transparent", "Normal" };

        public string[] VisibilityStatus = new string[] { "Both", "Only UUI" };

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



            UIDropDown UIStyleDropdown = UIDropDowns.AddLabelledDropDown(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.UISTYLE) );
            UIStyleDropdown.items = UIStyles;
            UIStyleDropdown.selectedValue = LuminaLogic.BackgroundStyle;
            currentY += 80f;
            if (LuminaLogic.BackgroundStyle == "LuminaNormal")
            {
                UIStyleDropdown.selectedValue = "Transparent";
            }
            else if (LuminaLogic.BackgroundStyle == "UnlockingItemBackground")
            {
                UIStyleDropdown.selectedValue = "Normal";
            }
            UIStyleDropdown.eventSelectedIndexChanged += (component, value) =>
            {
                int index = UIStyleDropdown.selectedIndex;
                if (UIStyles[index] == "Transparent")
                {
                    LuminaLogic.BackgroundStyle = "LuminaNormal";
                    ModSettings.Save();
                }
                else if (UIStyles[index] == "Normal")
                {
                    LuminaLogic.BackgroundStyle = "UnlockingItemBackground";
                    ModSettings.Save();
                }
            };
            currentY += 30f;


            /// Button Visibility Status dropdown
            UIDropDown ButtonVisibleToggle = UIDropDowns.AddLabelledDropDown(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.VISIBILITY_STATUS));
            ButtonVisibleToggle.items = VisibilityStatus;
            if (LuminaLogic.ShowButton == false)
            {
                ButtonVisibleToggle.selectedValue = "Only UUI";
            }
            else if (LuminaLogic.ShowButton == true)
            {
                ButtonVisibleToggle.selectedValue = "Both";
            }
            ButtonVisibleToggle.eventSelectedIndexChanged += (component, value) =>
            {
                int index = ButtonVisibleToggle.selectedIndex;
                if (VisibilityStatus[index] == "Only UUI")
                {
                    LuminaLogic.ShowButton = false;
                    ModSettings.Save();
                }
                else if (VisibilityStatus[index] == "Both")
                {
                    LuminaLogic.ShowButton = true;
                    ModSettings.Save();
                }
            };
            currentY += 50f;


            UIButton supportbutton = UIButtons.AddSmallerButton(panel, LeftMargin, currentY, "Support");
            currentY += 50f;
            supportbutton.eventClicked += (sender, args) =>
            {
                Process.Start("https://discord.gg/gdhyhfcj7A");
            };

            UIButton guidesbutton = UIButtons.AddSmallerButton(panel, LeftMargin, currentY, "Documentation");
            currentY += 50f;
            guidesbutton.eventClicked += (sender, args) =>
            {
                Process.Start("https://skylinx.gitbook.io/lumina-for-cs1");
            };


            UILabel TitleAO = UILabels.AddLabel(panel, LeftMargin, currentY, Translations.Translate("MORE_STYLES"), textScale: 0.8f, alignment: UIHorizontalAlignment.Center);
            currentY += 40f;

            UIButton WorkshopButton = UIButtons.AddIconButton(panel, LeftMargin, currentY, 50f, UITextures.InGameAtlas, "Workshop");

            WorkshopButton.normalBgSprite = "WorkshopButton";
            WorkshopButton.hoveredBgSprite = "WorkshopButtonHovered";
            WorkshopButton.pressedBgSprite = "WorkshopButtonPressed";
            WorkshopButton.eventClicked += (sender, args) =>
            {
                Process.Start("https://steamcommunity.com/workshop/browse/?appid=255710&browsesort=toprated&section=readytouseitems&requiredtags%5B%5D=lumina%20style");
            };



            currentY += 60f;
            UILabel version = UILabels.AddLabel(panel, LabelWidth, currentY, Assembly.GetExecutingAssembly().GetName().Version.ToString(), textScale: 0.7f, alignment: UIHorizontalAlignment.Center);
        }
    }
}