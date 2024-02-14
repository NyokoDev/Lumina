using HarmonyLib;
using Lumina.Shaders.AO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.PostProcessing;
using UnityEngine;

namespace Lumina.Patches
{

    /// <summary>
    /// This patch is derived from 'Render It' mod.
    /// </summary>
    [HarmonyPatch(typeof(MaterialFactory), "Get")]
    public static class MaterialFactoryGetPatch
    {
        public static bool Prefix(Dictionary<string, Material> ___m_Materials, ref Material __result, string shaderName)
        {
            UnityEngine.Debug.Log("Started MaterialFactory patch.");
            try
            {
                if (!___m_Materials.TryGetValue(shaderName, out var value))
                {
                    Shader val = AssetBundleUtils.Find(AssetManager.Instance.AssetBundle, shaderName);
                    if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
                    {
                        throw new ArgumentException($"Shader not found ({shaderName})");
                    }
                    value = new Material(val)
                    {
                        name = string.Format("PostFX - {0}", shaderName.Substring(shaderName.LastIndexOf("/") + 1)),
                        hideFlags = (HideFlags)52
                    };
                    ___m_Materials.Add(shaderName, value);
                }
                __result = value;
                return false;
            }
            catch (Exception ex)
            {
                Debug.Log((object)("[LUMINA] MaterialFactoryGetPatch:Prefix -> Exception: " + ex.Message));
                return false;
            }
        }
    }
}
