using System.IO;
using ColossalFramework.PlatformServices;
using HarmonyLib;

namespace Lumina.Patching
{
    [HarmonyPatch(typeof(Workshop), "UpdateItem", typeof(PublishedFileId), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string[]))]
    public static class WorkshopUpdateItemPatch
    {
        private static void Prefix(string contentPath, ref string[] tags)
        {
            string[] files = Directory.GetFiles(contentPath, "*.light");

            if (files.Length > 0)
            {
                tags = new[] { SteamHelper.kSteamTagMod, "Lumina Style" };
            }
        }
    }
}
