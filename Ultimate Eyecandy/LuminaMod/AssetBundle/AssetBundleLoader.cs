using AlgernonCommons;
using DigitalRuby.RainMaker;
using System.IO;
using UnityEngine;

namespace Lumina.Bundles
{
    public class RainmakerBundleLoader : MonoBehaviour
    {
        private AssetBundle loadedBundle;

        // Assets references
        private GameObject _rainPrefab;
        private Material _rainMaterial;
        private Shader _rainShader;
        private AudioClip _rainLightClip;
        private Texture2D _rainTexture;
        private GameObject rainInstance;

        void Start()
        {
            
            string assemblyDir = AssemblyUtils.AssemblyPath;
            string bundlePath = Path.Combine(Path.Combine(assemblyDir, "Resources"), Path.Combine("Shaders", "rainmaker"));

            Logger.Log("Bundle path: " + bundlePath);
            var attributes = File.GetAttributes(bundlePath);
            Logger.Log($"Attributes: {attributes}");  // Should NOT include FileAttributes.Directory


            if (!File.Exists(bundlePath))
            {
                Logger.Log($"AssetBundle file not found at: {bundlePath}");
                return;
            }

            AssetBundle shaderAssetBundle = new WWW(bundlePath)?.assetBundle;
            loadedBundle = AssetBundle.LoadFromFile(bundlePath);
            if (loadedBundle == null)
            {
                Logger.Log("Failed to load AssetBundle from path.");
                return;
            }

            Logger.Log("AssetBundle loaded successfully.");
            LogAllAssetsInBundle();
            LoadAssets();
        
        }

        private void LogAllAssetsInBundle()
        {
            string[] assetNames = loadedBundle.GetAllAssetNames();
            if (assetNames == null || assetNames.Length == 0)
            {
                Logger.Log("No assets found in AssetBundle.");
            }
            else
            {
                Logger.Log($"Found {assetNames.Length} assets in AssetBundle:");
                foreach (var assetName in assetNames)
                {
                    Logger.Log(" - " + assetName);
                }
            }
        }

        private void LoadAssets()
        {
            // Use exact asset names as printed by GetAllAssetNames()
            _rainPrefab = loadedBundle.LoadAsset<GameObject>("rainprefab.prefab");
            _rainMaterial = loadedBundle.LoadAsset<Material>("rainmaterial.mat");
            _rainShader = loadedBundle.LoadAsset<Shader>("rainshader.shader");
            _rainLightClip = loadedBundle.LoadAsset<AudioClip>("rain_light.ogg");
            _rainTexture = loadedBundle.LoadAsset<Texture2D>("raintexture.png");

            if (_rainPrefab != null)
            {
                Instantiate(_rainPrefab);
                Logger.Log("Instantiated rain prefab.");
                rainInstance = new GameObject("RainLumina");
                rainInstance.AddComponent<DemoScript>();
            }
            else Logger.Log("rainprefab.prefab NOT found in AssetBundle.");

            if (_rainMaterial == null) Logger.Log("rainmaterial.mat NOT found in AssetBundle.");
            if (_rainShader == null) Logger.Log("rainshader.shader NOT found in AssetBundle.");
            if (_rainLightClip == null) Logger.Log("rain_light.ogg NOT found in AssetBundle.");
            if (_rainTexture == null) Logger.Log("raintexture.png NOT found in AssetBundle.");
        }

        private void OnDestroy()
        {
            if (loadedBundle != null)
            {
                loadedBundle.Unload(false);
                Logger.Log("AssetBundle unloaded.");
            }
        }
    }
}
