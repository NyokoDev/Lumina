namespace Lumina.OptionsTabs
{
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using System.Diagnostics;

    /// <summary>
    /// LegacyTab class.
    /// </summary>
    public sealed class LegacyTab
    {
        private UITabstrip tabStrip;
        private const float Margin = 5f;
        private const float LeftMargin = 30f;
        private const float GroupMargin = 40f;
        private const float LabelWidth = 0f;
        private const float TabHeight = 20f;

        /// <summary>
        /// Main panel to call from.
        /// </summary>
        public static UIPanel panel;

        public UILabel SSAALabel;

        public UISlider SSAAConfig;
        public UIButton SSAAButton;
        public UILabel SSAALabel2;

        public UICheckBox LowerVRAMUSAGE;
        public UICheckBox UnlockSliderCheckbox;

        public UISlider AOSlider;
        public UISlider AOSliderRadius;

        public UILabel AORadiusTitleLabel;
        public UILabel AOIntensityTitleLabel;

        internal LegacyTab(UITabstrip tabStrip, int tabIndex)
        {

            panel = UITabstrips.AddTextTab(tabStrip, "Miscellanous", tabIndex, out UIButton _, autoLayout: false);

            float currentY = Margin;
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

            UILabel TitleAO = UILabels.AddLabel(panel, LeftMargin, currentY, Translations.Translate("Ambient Occlusion | Global Configuration"), textScale: 0.9f, alignment: UIHorizontalAlignment.Center);
            currentY += 40f;


            float labelWidth = panel.width - (Margin * 3f); // Adjust as needed
            float labelHeight = 0.8f; // Adjust as needed

            AOIntensityTitleLabel = UILabels.AddLabel(panel, 80f, currentY, Translations.Translate("Intensity"), labelWidth, labelHeight);
            UILabel AOIntensityLabel = UILabels.AddLabel(panel, LeftMargin, currentY, "AOINTENSITY", panel.width - (Margin * 2f), 0.9f);
            currentY += 20f;
            AOSlider = UISliders.AddBudgetSlider(panel, LeftMargin, currentY, 500f, 15f);
            currentY += 50f;
            AOSlider.value = LuminaLogic.AOIntensity;
            AOIntensityLabel.text = AOSlider.value.ToString();

            AOSlider.eventValueChanged += (sender, value) =>
            {

                LuminaLogic.AOIntensity = value;
                AOIntensityLabel.text = value.ToString();
            };




            AORadiusTitleLabel = UILabels.AddLabel(panel, 80f, currentY, Translations.Translate("Radius"), labelWidth, labelHeight);
            UILabel AORadiusLabel = UILabels.AddLabel(panel, LeftMargin, currentY, "AORADIUS", panel.width - (Margin * 2f), 0.9f);


            currentY += 20f;
            AOSliderRadius = UISliders.AddBudgetSlider(panel, LeftMargin, currentY, 500f, 15f);
            currentY += 50f;
            AOSliderRadius.value = LuminaLogic.AORadius;
            AORadiusLabel.text = AOSliderRadius.value.ToString();
            AOSliderRadius.eventValueChanged += (sender, value) =>
            {

                LuminaLogic.AORadius = value;
                AORadiusLabel.text = value.ToString();
            };





            UILabel TitleLabel1 = UILabels.AddLabel(panel, LeftMargin, currentY, Translations.Translate("Dynamic Resolution | Global Configuration"), textScale: 0.9f, alignment: UIHorizontalAlignment.Center);
            currentY += 40f;

            UILabel enable = UILabels.AddLabel(panel, LeftMargin, currentY, Translations.Translate("RESTART_TEXT"), textScale: 0.7f, alignment: UIHorizontalAlignment.Center);
            currentY += 50f;

            UIButton enableDRbutton = UIButtons.AddSmallerButton(panel, LeftMargin, currentY, Translations.Translate("Activate"));
            currentY += 50f;

            enableDRbutton.eventClicked += (sender, args) =>
            {
                LuminaLogic.DynResEnabled = !LuminaLogic.DynResEnabled;
                var value = LuminaLogic.DynResEnabled ? "Enabled" : "Disabled";
                ModSettings.Save();
            }
            ;

            ///
            /// Options if Dynamic Resolution is enabled.
            ///
            void HandleButtonClick(float value)
            {
                Loading.ActiveDRManager?.SetSSAAFactor(value);
            }


            if (LuminaLogic.DynResEnabled)
            {
                enableDRbutton.text = "Deactivate";
                enableDRbutton.height = 30f;

                SSAALabel = UILabels.AddLabel(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_TEXT), panel.width - (Margin * 2f), 0.8f);
                currentY += 20f;


                SSAAConfig = UISliders.AddBudgetSlider(panel, LeftMargin, currentY, 500f, DynamicResolutionManager.MaximumDRValue); // Main DR Slider.
                SSAAConfig.value = DynamicResolutionCamera.AliasingFactor;
                currentY += 20f;


                SSAAConfig.eventValueChanged += (c, value) =>
                {
                    SSAALabel2.text = SSAAConfig.value.ToString();
                };
                SSAALabel2 = UILabels.AddLabel(panel, LeftMargin, currentY, SSAAConfig.value.ToString(), panel.width - (Margin * 2f), 0.9f);
                currentY += 15f;

                SSAAButton = UIButtons.AddButton(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.SSAA_SLIDER_TEXT));
                SSAAButton.horizontalAlignment = UIHorizontalAlignment.Center;
                currentY += 35f;
                SSAAButton.eventClicked += (c, p) => HandleButtonClick(SSAAConfig.value);

                LowerVRAMUSAGE = UICheckBoxes.AddLabelledCheckBox(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.LOWERVRAMUSAGE));
                currentY += 30f;
                LowerVRAMUSAGE.isChecked = DynamicResolutionManager.LowerVRAMUsage;
                LowerVRAMUSAGE.eventCheckChanged += (c, isChecked) =>
                {
                    if (isChecked != DynamicResolutionManager.LowerVRAMUsage)
                    {
                        DynamicResolutionManager.LowerVRAMUsage = isChecked;
                    }
                };

                UnlockSliderCheckbox = UICheckBoxes.AddLabelledCheckBox(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.UnlockSliderLabel));
                currentY += 25f;
                UnlockSliderCheckbox.isChecked = DynamicResolutionManager.UnlockSlider;
                UnlockSliderCheckbox.eventCheckChanged += (c, isChecked) =>
                {

                    UnlockSliderNotif notification = NotificationBase.ShowNotification<UnlockSliderNotif>();
                    notification.AddParas("Unlocking the Dynamic Resolution slider comes with a cautionary note, as it may lead to potential instability within the game. Before proceeding, we would like to bring to your attention the possibility of encountering issues related to game stability, including potential implications for your GPU performance. Could you confirm your decision to unlock the Dynamic Resolution slider?");
                    notification._yesButton.eventClicked += (sender, args) =>
                    {
                        DynamicResolutionManager.MaximumDRValue = 10f;
                        UnlockSliderCheckbox.isChecked = true;
                    };
                    notification._noButton.eventClicked += (sender, args) =>
                    {
                        UnlockSliderCheckbox.isChecked = false;
                        DynamicResolutionManager.MaximumDRValue = 4f;
                    };

                };
            }
            else
            {
                enableDRbutton.text = "Activate";
            }
        }
    }
}

        
       
