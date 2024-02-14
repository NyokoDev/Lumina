namespace Lumina
{
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Lumina panel tab base.
    /// </summary>
    internal class PanelTabBase
    {
        /// <summary>
        /// Layout margin.
        /// </summary>
        protected const float Margin = 5f;

        /// <summary>
        /// Text header layout height.
        /// </summary>
        protected const float HeaderHeight = 20f;

        /// <summary>
        /// Checkbox UI layout height.
        /// </summary>
        protected const float CheckHeight = 20f;

        /// <summary>
        /// Slider UI layout height.
        /// </summary>
        protected const float SliderHeight = 40f;

        /// <summary>
        /// Panel control width.
        /// </summary>
        protected const float ControlWidth = LuminaPanel.ContentWidth - (Margin * 2f);

        // Layout constants - private.
        private const float ValueLabelWidth = 50f;
        private const float SliderTextScale = 0.8f;
        private const float SliderTextHeight = 14f;

        /// <summary>
        /// Adds a Lumina slider to the given UIComponent.
        /// </summary>
        /// <param name="panel">Parent component.</param>
        /// <param name="label">Slider text label.</param>
        /// <param name="minValue">Slider minimum value.</param>
        /// <param name="maxValue">Slider maximum value.</param>
        /// <param name="index">Slider setting index.</param>
        /// <param name="currentY">Relative Y position reference.</param>
        /// <returns>New UISlider.</returns>
        protected UISlider AddSlider(UIComponent panel, string label, float minValue, float maxValue, int index, ref float currentY)
        {
            // Title label.
            UILabel titleLabel = UILabels.AddLabel(panel, Margin, currentY, label, textScale: SliderTextScale);

            // Value label.
            UILabel newValueLabel = UILabels.AddLabel(panel, ControlWidth, currentY, "0", ValueLabelWidth, SliderTextScale, UIHorizontalAlignment.Right);

            // Add slider.
            UISlider newSlider = UISliders.AddBudgetSlider(panel, Margin, currentY + SliderTextHeight, ControlWidth, maxValue);
            newSlider.minValue = minValue;
            newSlider.stepSize = 0.0001f;
            newSlider.objectUserData = new LuminaSliderData
            {
                ValueLabel = newValueLabel,
                Index = index,
            };

            // Value display event handler.
            newSlider.eventValueChanged += (c, value) =>
            {
                if (c.objectUserData is LuminaSliderData sliderData)
                {
                    sliderData.ValueLabel.text = value.ToString("N4");
                }
            };

            // Set initial value to update label.
            newSlider.value = 0f;

            currentY += SliderHeight;
            return newSlider;
        }

        protected UISlider AddGlobalSlider(UIComponent panel, string label, float minValue, float maxValue, int index, ref float currentY)
        {
            // Title label.
            UILabel titleLabel = UILabels.AddLabel(panel, Margin, currentY, label, textScale: SliderTextScale);

            // Value label.
            UILabel newValueLabel = UILabels.AddLabel(panel, ControlWidth, currentY, "0", ValueLabelWidth, SliderTextScale, UIHorizontalAlignment.Right);

            // Add slider.
            UISlider newSlider = UISliders.AddBudgetSlider(panel, Margin, currentY + SliderTextHeight, 300f, maxValue);
            newSlider.minValue = minValue;
            newSlider.stepSize = 0.0001f;
            newSlider.objectUserData = new LuminaSliderData
            {
                ValueLabel = newValueLabel,
                Index = index,
            };

            // Value display event handler.
            newSlider.eventValueChanged += (c, value) =>
            {
                if (c.objectUserData is LuminaSliderData sliderData)
                {
                    sliderData.ValueLabel.text = value.ToString("N4");
                }
            };

            // Set initial value to update label.
            newSlider.value = 0f;

            currentY += SliderHeight;
            return newSlider;
        }


        protected UISlider AddDynamicSlider(UIComponent panel, string label, float minValue, float maxValue, int index, ref float currentY)
        {
            // Title label.
            UILabel titleLabel = UILabels.AddLabel(panel, Margin, currentY, label, textScale: SliderTextScale);

            // Value label.
            UILabel newValueLabel = UILabels.AddLabel(panel, ControlWidth, currentY, "0", ValueLabelWidth, SliderTextScale, UIHorizontalAlignment.Right);

            // Add slider.
            UISlider newSlider = UISliders.AddBudgetSlider(panel, Margin, currentY + SliderTextHeight, ControlWidth, maxValue);
            newSlider.minValue = minValue;
            newSlider.stepSize = 0.25f;
            newSlider.objectUserData = new LuminaSliderData
            {
                ValueLabel = newValueLabel,
                Index = index,
            };

            // Value display event handler.
            newSlider.eventValueChanged += (c, value) =>
            {
                if (c.objectUserData is LuminaSliderData sliderData)
                {
                    sliderData.ValueLabel.text = value.ToString("N4");
                }
            };

            // Set initial value to update label.
            newSlider.value = 1f;

            currentY += SliderHeight;
            return newSlider;
        }

        /// <summary>
        /// Slider data class.
        /// </summary>
        public class LuminaSliderData
        {
            public UILabel ValueLabel;
            public int Index;
        }
    }
}
