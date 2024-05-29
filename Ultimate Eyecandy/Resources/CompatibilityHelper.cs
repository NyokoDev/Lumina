using Lumina;

namespace Lumina.CompatibilityPolice
{
    public static class CompatibilityHelper
    {
        public static readonly string[] LIGHT_COLORS_MANIPULATING_MODS = { "naturallighting", "lightingrebalance", "daylightclassic", "softershadows", "renderit" };

        public static readonly string[] SKY_MANIPULATING_MODS = { "thememixer" };

        public static readonly string[] FOG_MANIPULATING_MODS = { "fogcontroller", "fogoptions", "daylightclassic" };

        public static bool IsAnyLightColorsManipulatingModsEnabled()
        {
            if (ModUtils.IsAnyModsEnabled(LIGHT_COLORS_MANIPULATING_MODS))
            {
                return true;
            }

            return false;
        }

        public static bool IsAnySkyManipulatingModsEnabled()
        {
            if (ModUtils.IsAnyModsEnabled(SKY_MANIPULATING_MODS))
            {
                return true;
            }

            return false;
        }

        public static bool IsAnyFogManipulatingModsEnabled()
        {
            if (ModUtils.IsAnyModsEnabled(FOG_MANIPULATING_MODS))
            {
                return true;
            }

            return false;
        }
    }
}
