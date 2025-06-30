using ColossalFramework.UI;
using HarmonyLib;
using Lumina;
using System.Linq;
using UnityEngine;
using Logger = Lumina.Logger;

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
        // Skip on non-Windows platforms
        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
        {
            Logger.Log("[Lumina] Skipping borderless option injection on non-Windows platform.");
            yield break;
        }

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

            // Create label
            var label = dropdown.parent.AddUIComponent<UILabel>();
            label.text = "Powered by Lumina";
            label.textScale = 0.8f;              // Slightly smaller text
            label.textAlignment = UIHorizontalAlignment.Center;
            label.autoSize = false;
            label.width = dropdown.width;
            label.height = 20;
            label.relativePosition = new UnityEngine.Vector3(dropdown.relativePosition.x, dropdown.relativePosition.y - label.height - 4); // 4px gap above dropdown
            label.textColor = new Color32(200, 200, 200, 255); // light gray

            dropdown.textColor = new Color32(255, 255, 255, 255);    // White text
            dropdown.color = new Color32(30, 30, 30, 255);           // Dark background

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
