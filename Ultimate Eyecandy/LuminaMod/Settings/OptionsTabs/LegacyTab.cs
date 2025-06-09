namespace Lumina.OptionsTabs
{
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.Plugins;
    using ColossalFramework;
    using ColossalFramework.UI;
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.IO;

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
        public float currentY = Margin;

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

        public UIButton enableDRbutton;

        internal LegacyTab(UITabstrip tabStrip, int tabIndex)
        {
            panel = UITabstrips.AddTextTab(tabStrip, "Miscellanous", tabIndex, out UIButton _, autoLayout: false);
            // This will reconstruct the UI for both activation and deactivation
            RefreshTab();
        }

        private void HandleButtonClick(float value)
        {
            DynamicResolutionCamera.AliasingFactor = value;
            ModSettings.Save();
        }

        private void CreateDynamicResolutionText()
        {
            // Modern, visually distinct header
            UILabel titleLabel = UILabels.AddLabel(
                panel,
                LeftMargin,
                currentY,
                Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_ONBOARDING),
                panel.width - (Margin * 2f),
                1.15f // Larger text scale for emphasis
            );
            titleLabel.textColor = new UnityEngine.Color32(79, 195, 247, 255); // Modern blue
            titleLabel.wordWrap = true;
            titleLabel.padding = new UnityEngine.RectOffset(0, 0, 8, 8); // Extra vertical padding
            currentY += 44f; // Adjust for larger text and padding

            UILabel restartLabel = UILabels.AddLabel(
                panel,
                LeftMargin,
                currentY,
                Translations.Translate("RESTART_TEXT"),
                panel.width - (Margin * 2f),
                0.8f
            );
            restartLabel.textColor = new UnityEngine.Color32(180, 180, 180, 255); // Subtle gray
            restartLabel.wordWrap = true;
            currentY += 32f;
        }

        private void CreateDynamicResolutionButton()
        {
            try
            {
                string buttonText = LuminaLogic.DynResEnabled
                    ? Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_DEACTIVATE)
                    : Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_ACTIVATE);

                UIButton enableDRbutton = UIButtons.AddSmallerButton(panel, LeftMargin, currentY, buttonText);
                currentY += 50f;

                enableDRbutton.eventClicked += (sender, args) =>
                {
                    try
                    {
                        LuminaLogic.DynResEnabled = !LuminaLogic.DynResEnabled;

                        // Immediately update button text if just enabled
                        if (LuminaLogic.DynResEnabled)
                        {
                            Logger.Log("Dynamic Resolution activated.");
                            enableDRbutton.text = Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_DEACTIVATE);
                        }
                        else
                        {
                            Logger.Log("Dynamic Resolution deactivated.");
                            enableDRbutton.text = Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_ACTIVATE);
                        }

                        ModSettings.Save();
                        RefreshTab();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error while toggling dynamic resolution or refreshing plugins: {ex}");
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Log($"Error while creating dynamic resolution button: {ex}");
            }
        }
            
        private void RefreshTab()
        {
            if (panel != null)
            {
                panel.components.Clear();
                currentY = Margin;

                // Add UI limitation warning label at the top
                UILabel warningLabel = UILabels.AddLabel(
                    panel,
                    LeftMargin,
                    currentY,
                    "⚠️ Due to Cities: Skylines UI limitations, this panel may not update or hide options immediately.\n" +
                    "If options do not update, please close and reopen the options panel.\n" +
                    "If changed in-game (not from the main menu), a full game restart is required.",
                    panel.width - (Margin * 2f),
                    0.8f
                );
                warningLabel.textColor = new UnityEngine.Color32(255, 200, 40, 255); // Optional: yellow/orange for visibility
                warningLabel.wordWrap = true;
                currentY += 48f; // Adjust for your layout

                // Clear all dynamic UI references
                SSAALabel = null;
                SSAAConfig = null;
                SSAAButton = null;
                SSAALabel2 = null;
                LowerVRAMUSAGE = null;
                UnlockSliderCheckbox = null;
                AOSlider = null;
                AOSliderRadius = null;
                AORadiusTitleLabel = null;
                AOIntensityTitleLabel = null;
                enableDRbutton = null;

                CreateOpenLogsButton();
                CreateDynamicResolutionText();
                CreateDynamicResolutionButton();

                if (LuminaLogic.DynResEnabled)
                {
                    // Use local variables for all dynamic resolution UI elements
                    UILabel ssaalabel = UILabels.AddLabel(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_TEXT), panel.width - (Margin * 2f), 0.8f);
                    currentY += 20f;

                    UISlider ssaaConfig = UISliders.AddBudgetSlider(panel, LeftMargin, currentY, 500f, DynamicResolutionManager.MaximumDRValue);
                    ssaaConfig.value = DynamicResolutionCamera.AliasingFactor;
                    currentY += 20f;

                    UILabel ssaalabel2 = UILabels.AddLabel(panel, LeftMargin, currentY, ssaaConfig.value.ToString(), panel.width - (Margin * 2f), 0.9f);
                    currentY += 15f;

                    ssaaConfig.eventValueChanged += (c, value) =>
                    {
                        ssaalabel2.text = ssaaConfig.value.ToString();
                    };

                    UIButton ssaaButton = UIButtons.AddButton(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.SSAA_SLIDER_TEXT));
                    ssaaButton.horizontalAlignment = UIHorizontalAlignment.Center;
                    currentY += 35f;
                    ssaaButton.eventClicked += (c, p) =>
                    {
                        HandleButtonClick(ssaaConfig.value);
                        Logger.Log($"SSAA value set to {ssaaConfig.value}");
                    };

                    UICheckBox lowerVRAMUsage = UICheckBoxes.AddLabelledCheckBox(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.LOWERVRAMUSAGE));
                    currentY += 30f;
                    lowerVRAMUsage.isChecked = DynamicResolutionManager.LowerVRAMUsage;
                    lowerVRAMUsage.eventCheckChanged += (c, isChecked) =>
                    {
                        if (isChecked != DynamicResolutionManager.LowerVRAMUsage)
                        {
                            DynamicResolutionManager.LowerVRAMUsage = isChecked;
                            Logger.Log($"Lower VRAM Usage set to {isChecked}");
                        }
                    };
                    UICheckBox unlockSliderCheckbox = UICheckBoxes.AddLabelledCheckBox(panel, LeftMargin, currentY, Translations.Translate(LuminaTR.TranslationID.UnlockSliderLabel));
                    currentY += 25f;
                    unlockSliderCheckbox.isChecked = DynamicResolutionManager.UnlockSlider;
                    unlockSliderCheckbox.eventCheckChanged += (c, isChecked) =>
                    {
                        // Get GPU name dynamically
                        string gpuName = UnityEngine.SystemInfo.graphicsDeviceName;

                        string warningText =
                            "Unlocking the Dynamic Resolution slider comes with a cautionary note.\n\n" +
                            $"Your detected GPU: {gpuName}\n\n" +
                            "Raising the slider beyond the default maximum can significantly increase GPU workload, " +
                            "leading to much higher temperatures, power usage, and the risk of graphical glitches or instability. " +
                            "On some systems, this may cause the game to stutter, crash, or even force your computer to shut down if hardware limits are exceeded.\n\n" +
                            "Only proceed if you understand these risks and have a capable GPU. " +
                            "If you experience issues, reset the slider to a lower value or restart the game.\n\n" +
                            "**Note:** Changes to this setting may not take full effect until you close and reopen the options menu.";


                        UnlockSliderNotif notification = NotificationBase.ShowNotification<UnlockSliderNotif>();
                        notification.AddParas(warningText);
                        notification._yesButton.eventClicked += (sender, args) =>
                        {
                            DynamicResolutionManager.MaximumDRValue = 10f;
                            unlockSliderCheckbox.isChecked = true;
                            DynamicResolutionManager.UnlockSlider = true;
                            Logger.Log("Dynamic Resolution slider unlocked (max value set to 10). You might need to reopen the options menu to see the changes.");
                            EurekaNotif notification2 = NotificationBase.ShowNotification<EurekaNotif>();
                            notification2.AddParas("Dynamic Resolution slider unlocked (max value set to 10). You might need to reopen the options menu to see the changes.");
                            Save();
                        };
                        notification._noButton.eventClicked += (sender, args) =>
                        {
                            unlockSliderCheckbox.isChecked = false;
                            DynamicResolutionManager.MaximumDRValue = 4f;
                            DynamicResolutionManager.UnlockSlider = false;
                            Logger.Log("Dynamic Resolution slider lock retained (max value set to 4).");
                            EurekaNotif notification2 = NotificationBase.ShowNotification<EurekaNotif>();
                            notification2.AddParas("Dynamic Resolution slider lock retained (max value set to 4). You might need to reopen the options menu to see the changes.");
                            Save();
                        };
                    };
                }
            }
        }

        private void Save()
        {
            ModSettings.Save();
        }

        private void CreateOpenLogsButton()
        {
            UIButton openLogsButton = UIButtons.AddSmallerButton(panel, LeftMargin, currentY, "Open Logs Folder");
            currentY += 50f;

            if (openLogsButton != null)
            {
                openLogsButton.eventClicked += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(Logger.logsPath))
                    {
                        string modPath = Singleton<PluginManager>.instance.FindPluginInfo(Assembly.GetAssembly(typeof(LuminaMod))).modPath;
                        Logger.logsPath = Path.Combine(modPath, "Logs");
                    }

                    try
                    {
                        Process.Start(Logger.logsPath);
                        Logger.Log("Logs folder opened.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to open logs folder: {ex}");
                    }
                };
            }
            else
            {
                Logger.Log("Failed to create the Open Logs button.");
            }
        }
    }
}
