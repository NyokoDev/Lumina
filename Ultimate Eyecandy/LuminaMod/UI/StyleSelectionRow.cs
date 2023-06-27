// <copyright file="VehicleSelectionRow.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace Lumina
{
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// UIList row item for Lumina styles.
    /// </summary>
    public class StyleSelectionRow : UIListRow
    {
        /// <summary>
        /// Row height.
        /// </summary>
        public const float StyleRowHeight = 20f;

        // Layout constants - private.
        private const float ScrollMargin = 10f;

        // Style name label.
        private UILabel _styleNameLabel;

        /// <summary>
        /// Gets the height for this row.
        /// </summary>
        public override float RowHeight => StyleRowHeight;

        /// <summary>
        /// Generates and displays a row.
        /// </summary>
        /// <param name="data">Object data to display.</param>
        /// <param name="rowIndex">Row index number (for background banding).</param>
        public override void Display(object data, int rowIndex)
        {
            // Perform initial setup for new rows.
            if (_styleNameLabel == null)
            {
                // Add object name label.
                _styleNameLabel = AddLabel(Margin, width - Margin - ScrollMargin);
            }

            // Get building ID and set name label.
            if (data is LuminaStyle style)
            {
                _styleNameLabel.text = style.StyleName;
            }
            else
            {
                // Just in case (no valid style record).
                _styleNameLabel.text = string.Empty;
            }

            // Set initial background as deselected state.
            Deselect(rowIndex);
        }
    }
}