using AlgernonCommons.UI;

namespace Lumina
{
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using Lumina.OptionsTabs;
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
            m_BackgroundSprite = "UnlockingPanel";


            UISprite imageSprite = this.AddUIComponent<UISprite>();

            imageSprite.height = 43f;
            imageSprite.relativePosition = new Vector3(290f, 10f);
            imageSprite.width = 114f;
            imageSprite.atlas = UITextures.LoadSingleSpriteAtlas("..\\Resources\\logo");
            imageSprite.spriteName = "normal";
            imageSprite.zOrder = 5;

            UISprite image2Sprite = this.AddUIComponent<UISprite>();

            image2Sprite.height = 1000f;
            image2Sprite.relativePosition = new Vector3(0f, -50f);
            image2Sprite.width = 1000f;
            image2Sprite.atlas = UITextures.LoadSingleSpriteAtlas("..\\Resources\\bck");
            image2Sprite.spriteName = "normal";
            image2Sprite.zOrder = 1;

            Tabstrip();
        }
    

    /// <summary>
    /// Creates the tabstrips.
    /// </summary>
    public void Tabstrip()
    {

        UITabstrip tabStrip = UITabstrips.AddTabstrip(this, 0f, 0f, OptionsPanelManager<OptionsPanel>.PanelWidth, OptionsPanelManager<OptionsPanel>.PanelHeight, out UITabContainer _);
        MainTab updatedTab = new Lumina.OptionsTabs.MainTab(tabStrip, 0);
        LegacyTab legacyTab = new Lumina.OptionsTabs.LegacyTab(tabStrip, 1);

        // Select the first tab.
        tabStrip.selectedIndex = -1;
    }

            
        
    



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

