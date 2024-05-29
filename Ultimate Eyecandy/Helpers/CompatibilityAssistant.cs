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
        public static void ShowLightColorsNotification()
        {
            LightColorsNotification notification = NotificationBase.ShowNotification <LightColorsNotification>();
            notification.AddParas("1 or several incompatible mods have been detected. For optimal use of Lumina, turn off the conflicting mods since both have identical functions. This message will stop appearing once all incompatibilities have been resolved, otherwise proceed with your action. Check Lumina.LogFile for more information.");
        }

        public static void CheckAll()
        {

            Logger.Log("You can disregard the compatibility messages if you wish to mix various mods together. Always remember this can cause unexpected behaviour.");

            string[] potentialConflicts = { "renderit", "thememixer" };
            if (ModUtils.IsAnyModsEnabled(potentialConflicts))
            {
                Logger.Log("Several incompatibilities with other mods found. Any unexpected behavior is caused by these mods: Render it or Theme Mixer 2/2.5");
                CompatibilityAssistant.ShowLightColorsNotification();
            }

                if (CompatibilityHelper.IsAnyLightColorsManipulatingModsEnabled())
            {
                CompatibilityAssistant.ShowLightColorsNotification();
                Logger.Log("Several incompatibilities have been found for Light Colors Manipulating Mods: Relight-Daylight Classic-NaturalLighting");
            }

            if (ModUtils.IsModEnabled("skyboxreplacer"))
            {
                Logger.Log("1 incompatibility has been found for Skybox replacement: Cubemap Replacer");

            }
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


                _noButton = AddButton(2, NumButtons, Translations.Translate("LockSlider"), Close);

            }
        }


    }

