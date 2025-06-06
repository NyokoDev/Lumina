using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using ColossalFramework.UI;
using Lumina.CompatibilityPolice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lumina.Helpers
{
    internal class CompatibilityAssistant
    {
        public enum LightColorsConflictType
        {
            RenderIt,
            LightColorMods,
            CubemapReplacer
        }


        public static void ShowLightColorsNotification(LightColorsConflictType conflictType)
        {
            LightColorsNotification notification = NotificationBase.ShowNotification<LightColorsNotification>();

            switch (conflictType)
            {
                case LightColorsConflictType.RenderIt:
                    notification.AddParas("Render It mod detected alongside Lumina. These mods share some similar features, which may overlap.");
                    break;

                case LightColorsConflictType.LightColorMods:
                    notification.AddParas("One or more light color modification mods (Relight, Daylight Classic, Natural Lighting) are active. These mods can interfere with Lumina’s lighting adjustments. They can work together, but disable conflicting mods if you notice issues.");
                    break;
                case LightColorsConflictType.CubemapReplacer:
                    notification.AddParas("Incompatibility detected: 'Cubemap Replacer' directly conflicts with Lumina’s Skybox feature and can cause crashes. Please disable or remove this mod.");
                    break;
                default:
                    notification.AddParas("One or more incompatible mods have been detected. For optimal use of Lumina, turn off the conflicting mods since both have identical functions. Check Lumina.LogFile for more information.");
                    break;
            }
        }

        public static void CheckAll()
        {
            if (!LuminaLogic.Compatibility)
            {
                // Compatibility is false, so skip all checks
                Logger.Log("Skipped all compatibility checks.");
                return;
            }

            Logger.Log("You can disregard the compatibility messages if you wish to mix various mods together. Always remember this can cause unexpected behaviour.");

            string[] potentialConflicts = { "renderit" };
            if (ModUtils.IsAnyModsEnabled(potentialConflicts))
            {
                Logger.Log("Render It is compatible with Lumina, but some settings may overlap. If you notice unusual visuals, consider disabling one of the mods.");
                ShowLightColorsNotification(LightColorsConflictType.RenderIt);
            }

            if (CompatibilityHelper.IsAnyLightColorsManipulatingModsEnabled())
            {
                ShowLightColorsNotification(LightColorsConflictType.LightColorMods);
                Logger.Log("Light color mods like Relight, Daylight Classic, and Natural Lighting may interfere with Lumina’s lighting adjustments. They can work together, but disable them if you notice inconsistencies.");
            }

            if (ModUtils.IsModEnabled("skyboxreplacer"))
            {
                ShowLightColorsNotification(LightColorsConflictType.CubemapReplacer);
                Logger.Log("Incompatibility detected: 'Cubemap Replacer' conflicts with Lumina’s Skybox replacement. Please remove it to avoid serious issues like crashes.");
            }
        }


        public class LightColorsNotification : ListNotification
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
            protected override int NumButtons => 2;



            /// <summary>
            /// Adds buttons to the message box.
            /// </summary>
            public override void AddButtons()
            {
                // Add yes button.
                _yesButton = AddButton(1, NumButtons, Translations.Translate("UnlockSlider"), Close);

                // Add no button with SetNoForCompatibility() called before Close().
                _noButton = AddButton(2, NumButtons, Translations.Translate("LockSlider"), () =>
                {
                    SetNoForCompatibility();
                    Close();
                });
            }

            private void SetNoForCompatibility()
            {
                LuminaLogic.Compatibility = false;

                // Save settings
                ModSettings.Save();

            }
        }
    }
}




