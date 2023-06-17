using HarmonyLib;

namespace Lumina.Patches
{
    [HarmonyPatch(typeof(DayNightProperties))]
    [HarmonyPatch("UpdateLighting")]
    class UpdateLighting
    {
        static void Postfix()
        {
            RenderManager.instance.MainLight.shadowBias = LuminaMod.GetShadowBias();
        }
    }
}
