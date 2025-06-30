using HarmonyLib;
using ColossalFramework.UI;
using System.Linq;
using Lumina;

[HarmonyPatch(typeof(OptionsGraphicsPanel), "Awake")]
public static class Patch_OptionsGraphicsPanel_Awake
{
    public static void Prefix(OptionsGraphicsPanel __instance)
    {
        // Defer execution until Unity sets up UI components
        __instance.StartCoroutine(InjectBorderlessOption(__instance));
    }

    private static System.Collections.IEnumerator InjectBorderlessOption(OptionsGraphicsPanel instance)
    {
        // Wait one frame to ensure UI elements are initialized
        yield return null;

        var dropdown = instance.Find<UIDropDown>("Fullscreens");

        if (dropdown == null)
        {
            Logger.Log("[Lumina] Could not find 'Fullscreens' dropdown.");
            yield break;
        }

        var items = dropdown.items.ToList();

        if (!items.Contains("Fullscreen Borderless"))
        {
            items.Add("Fullscreen Borderless");
            dropdown.items = items.ToArray();
        }

        dropdown.eventSelectedIndexChanged += (component, index) =>
        {
            if (dropdown.items[index] == "Fullscreen Borderless")
            {
                BorderlessFullscreen.SetBorderless(); // Your static method
                Logger.Log("[Lumina] Fullscreen Borderless applied.");
            }
        };
    }
}
