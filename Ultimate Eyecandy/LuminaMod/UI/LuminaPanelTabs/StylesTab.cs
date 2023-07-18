namespace Lumina
{
    using System;
    using System.Linq;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;

    /// <summary>
    /// Lumina panel tab for setting shadow options.
    /// </summary>
    internal sealed class StylesTab : PanelTabBase
    {
        // Components.
        UIList _styleList;
        UITextField _nameField;
        UIButton _saveButton;
        UIButton _deleteButton;
        UIButton _loadButton;

        // Selection.
        private LuminaStyle _selectedItem;

        /// <summary>
        /// Gets or sets the currently selected style.
        /// </summary>
        private LuminaStyle SelectedItem
        {
            get => _selectedItem;

            set
            {
                // Don't do anything if no change.
                if (_selectedItem != value)
                {
                    _selectedItem = value;

                    // Toggle button state.
                    _deleteButton.isEnabled = value != null && value.IsLocal;
                    _loadButton.isEnabled = value != null;

                    // Reset name textfield.
                    _nameField.text = value.StyleName;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StylesTab"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal StylesTab(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.STYLES_MOD_NAME), tabIndex, out UIButton _);

            float currentY = Margin;

            // Vehicle selection list.
            _styleList = UIList.AddUIList<StyleSelectionRow>(
                panel,
                0f,
                0f,
                ControlWidth,
                200f,
                20f);
            _styleList.EventSelectionChanged += (c, selectedItem) => SelectedItem = selectedItem as LuminaStyle;
            currentY += _styleList.height + Margin;

            // Load theme button.
            _loadButton = UIButtons.AddButton(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LOAD_TEXT));
            _loadButton.eventClicked += (c, p) =>
            {
                if (SelectedItem != null)
                {
                    SelectedItem.Apply();
                    RefreshDisplayList();
                }
            };
            _loadButton.Disable();

            // Delete theme button.
            _deleteButton = UIButtons.AddButton(panel, ControlWidth - 200f, currentY, Translations.Translate(LuminaTR.TranslationID.DELETE_TEXT));
            _deleteButton.eventClicked += (c, p) =>
            {
                if (SelectedItem != null)
                {
                    StyleManager.DeleteStyle(SelectedItem);
                    RefreshDisplayList();
                }
            };
            _deleteButton.Disable();
            currentY += _deleteButton.height + Margin;

            // Theme name textfield and label.
            UILabel nameLabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.STYLENAME_TEXT));
            currentY += nameLabel.height;
            _nameField = UITextFields.AddTextField(panel, Margin, currentY, ControlWidth);

            // Save theme button.
            currentY += _nameField.height + Margin;
            _saveButton = UIButtons.AddButton(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.SAVE_TEXT));
            _saveButton.eventClicked += (c, p) =>
            {
                if (!_nameField.text.IsNullOrWhiteSpace())
                {
                    // Determine if this style exists.
                    if (StyleManager.StyleList.Find(x => x.IsLocal && x.StyleName.Equals(_nameField.text)) is LuminaStyle existingStyle)
                    {
                        // Existing style - update it and save.
                        existingStyle.UpdateStyle();
                        existingStyle.Save();
                    }
                    else
                    {
                        // No existing style - add new style.
                        try
                        {
                            LuminaStyle newStyle = new LuminaStyle(_nameField.text, true);
                            newStyle.Save();
                            StyleManager.StyleList.Add(newStyle);

                            // Refresh list and current selection.
                            RefreshDisplayList();
                            _styleList.FindItem(newStyle);
                        }
                        catch (Exception e)
                        {
                            Logging.LogException(e, "exception creating new style");
                        }
                    }
                }
            };

            // Load styles and populate initial list.
            StyleManager.LoadStyles();
            RefreshDisplayList();
        }

        /// <summary>
        /// Refreshes the style list display.
        /// </summary>
        private void RefreshDisplayList()
        {
            _styleList.Data = new FastList<object>
            {
                m_buffer = StyleManager.StyleList.OrderBy(x => x.StyleName).ToArray(),
                m_size = StyleManager.StyleList.Count,
            };

            // Reselect previously selected item.
            _styleList.FindItem(SelectedItem);
        }
    }
}