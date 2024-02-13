using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace Lumina.Shaders.AO
{
    internal class AO : MonoBehaviour
    {
        private AmbientOcclusionModel _ambientOcclusionModel;
        

        public void Start()
        {
            AssetManager.Instance.AssetBundle = AssetBundleUtils.LoadAssetBundle("renderit");
            UnityEngine.Debug.Log("[LUMINA] Succesfully loaded AO Shader.");
            _ambientOcclusionModel = new AmbientOcclusionModel();
            AmbientOcclusionModel.Settings settings = _ambientOcclusionModel.settings;
            _ambientOcclusionModel.enabled = true;
            settings.intensity = LuminaLogic.AOIntensity;
            settings.radius = LuminaLogic.AOIntensity;
            UnityEngine.Debug.Log("[LUMINA] Ambient Occlusion enabled.");
        }
    }
}

