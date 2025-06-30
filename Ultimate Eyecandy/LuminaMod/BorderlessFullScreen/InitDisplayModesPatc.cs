using HarmonyLib;
using ColossalFramework.UI;
using System.Linq;
using Lumina;
using UnityEngine;
using Logger = Lumina.Logger;

[HarmonyPatch(typeof(OptionsGraphicsPanel), "InitDisplayModes")]
public static class Patch_DisplayModes_Postfix
{
    static void Postfix(OptionsGraphicsPanel __instance)
    {
        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
        {
            Logger.Log("[Lumina] Skipping borderless option injection on non-Windows platform.");
            return;
        }

        var dropdown = __instance.Find<UIDropDown>("Fullscreens");
        if (dropdown == null) return;

        var items = dropdown.items.ToList();

        if (!items.Contains("Fullscreen Borderless"))
        {
            items.Add("Fullscreen Borderless");
            dropdown.items = items.ToArray();
        }

        // Reassign handler so we don't double-bind
        dropdown.eventSelectedIndexChanged -= OnDropdownChanged;
        dropdown.eventSelectedIndexChanged += OnDropdownChanged;
    }

    static void OnDropdownChanged(UIComponent component, int index)
    {
        var dropdown = component as UIDropDown;
        if (dropdown != null && dropdown.items[index] == "Fullscreen Borderless")
        {
            BorderlessFullscreen.SetBorderless();
            Logger.Log("[Lumina] Borderless fullscreen mode applied.");
        }
    }
}
