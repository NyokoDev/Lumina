namespace Lumina
{
    using AlgernonCommons.UI;
    using System.Diagnostics;

    /// <summary>
    /// The mod's settings options panel.
    /// </summary>
    public sealed class OptionsPanel : OptionsPanelBase
    {
        /// <summary>
        /// Performs on-demand panel setup.
        /// </summary>
        protected override void Setup()
        {
        }

        /// <summary>
        /// Opens the LUT editor.
        /// </summary>
        private void OpenLUTEditor()
        {
            // TODO: fix to use package path.
            string lutEditorPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\255710\2983036781\LUT Editor\";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = lutEditorPath;
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }
    }
}