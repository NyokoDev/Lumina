using HarmonyLib;
using ColossalFramework.UI;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Logger = Lumina.Logger;

[HarmonyPatch(typeof(OptionsGraphicsPanel), "OnApplyGraphics")]
public static class Patch_OptionsGraphicsPanel_OnApplyGraphics
{
    public static bool Prefix(OptionsGraphicsPanel __instance)
    {
        // Skip patch if not Windows
        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
        {
            Logger.Log("[Lumina] Skipping borderless OnApplyGraphics patch on non-Windows platform.");
            return true; // run original method
        }

        if (Application.isEditor)
            return false; // skip original in editor

        int previousScreenWidth = Screen.width;
        int previousScreenHeight = Screen.height;
        bool previousFullscreen = Screen.fullScreen;

        var resolutionDropdownField = typeof(OptionsGraphicsPanel).GetField("m_ResolutionDropdown", BindingFlags.Instance | BindingFlags.NonPublic);
        var supportedResolutionsField = typeof(OptionsGraphicsPanel).GetField("m_SupportedResolutions", BindingFlags.Instance | BindingFlags.NonPublic);
        var fullscreenDropdownField = typeof(OptionsGraphicsPanel).GetField("m_FullscreenDropdown", BindingFlags.Instance | BindingFlags.NonPublic);

        if (resolutionDropdownField == null || supportedResolutionsField == null || fullscreenDropdownField == null)
        {
            Debug.LogError("[Lumina] Failed to reflect necessary private fields in OptionsGraphicsPanel.");
            return true; // fallback to original method
        }

        UIDropDown resolutionDropdown = resolutionDropdownField.GetValue(__instance) as UIDropDown;
        List<Resolution> supportedResolutions = supportedResolutionsField.GetValue(__instance) as List<Resolution>;
        UIDropDown fullscreenDropdown = fullscreenDropdownField.GetValue(__instance) as UIDropDown;

        if (resolutionDropdown == null || supportedResolutions == null || fullscreenDropdown == null)
        {
            Debug.LogError("[Lumina] Reflected fields returned null.");
            return true; // fallback
        }

        if (resolutionDropdown.selectedIndex < 0 || resolutionDropdown.selectedIndex >= supportedResolutions.Count)
        {
            Debug.LogError("[Lumina] Selected resolution index out of range.");
            return true;
        }

        Resolution selectedResolution = supportedResolutions[resolutionDropdown.selectedIndex];
        string selectedMode = fullscreenDropdown.selectedValue;

        if (selectedMode == "Fullscreen Borderless")
        {
            if (selectedResolution.width == previousScreenWidth &&
                selectedResolution.height == previousScreenHeight &&
                !Screen.fullScreen)
            {
                return false; // no change, skip original
            }

            BorderlessFullscreen.SetBorderless();

            PlayerPrefs.SetInt("ScreenWidth", selectedResolution.width);
            PlayerPrefs.SetInt("ScreenHeight", selectedResolution.height);
            PlayerPrefs.SetString("Lumina.FullscreenMode", "borderless");
            PlayerPrefs.Save();

            return false; // skip original OnApplyGraphics
        }

        return true;
    }
}
