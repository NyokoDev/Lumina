using AlgernonCommons;
using DigitalRuby.RainMaker;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

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
        private AudioClip _rainMediumClip;
        private AudioClip _rainHeavyClip;
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

        [SerializeField] private AudioMixerGroup rainAudioMixerGroup; // assign this in inspector or load it

        private void LoadAssets()
        {
            GameObject rainPrefab = loadedBundle.LoadAsset<GameObject>("assets/rainmaker/prefab/rainprefab.prefab");
            Material rainMat = loadedBundle.LoadAsset<Material>("assets/rainmaker/prefab/rainmaterial.mat");
            Material rainExplosionMat = loadedBundle.LoadAsset<Material>("assets/rainmaker/prefab/rainexplosionmaterial.mat");
            Material rainMistMat = loadedBundle.LoadAsset<Material>("assets/rainmaker/prefab/rainmistmaterial.mat");
            Shader rainShader = loadedBundle.LoadAsset<Shader>("assets/rainmaker/prefab/rainshader.shader");
            AudioClip clipRainLight = loadedBundle.LoadAsset<AudioClip>("assets/rainmaker/prefab/rain_light.ogg");
            AudioClip clipRainMedium = loadedBundle.LoadAsset<AudioClip>("assets/rainmaker/prefab/rain_medium.ogg");
            AudioClip clipRainHeavy = loadedBundle.LoadAsset<AudioClip>("assets/rainmaker/prefab/rain_heavy.ogg");
            AudioClip clipWind = loadedBundle.LoadAsset<AudioClip>("assets/rainmaker/prefab/wind_normal1.ogg");
            Texture2D rainTexture = loadedBundle.LoadAsset<Texture2D>("assets/rainmaker/prefab/raintexture.png");

            if (rainPrefab != null)
            {
                GameObject instance = Instantiate(rainPrefab);
                Logger.Log("Instantiated rain prefab.");

                // Attach RainScript to the root prefab instance
                instance.AddComponent<RainScript>();

                // Find the 'RainFallParticleSystem' child GameObject
                Transform rainFallTransform = instance.transform.Find("RainFallParticleSystem");
                if (rainFallTransform != null)
                {
                    GameObject rainFall = rainFallTransform.gameObject;

                    // Add the RainCollision component directly
                    rainFall.AddComponent<RainCollision>();
                    Logger.Log("Added RainCollision to RainFallParticleSystem.");
                }
                else
                {
                    Logger.Log("RainFallParticleSystem not found in rain prefab.");
                }
            }
            else
            {
                Logger.Log("rainPrefab is null.");
            }


            BaseRainScript.rainMaterial = rainMat;
            BaseRainScript.rainExplosionMaterial = rainExplosionMat;
            BaseRainScript.rainMistMaterial = rainMistMat;

            // Make sure you have a MonoBehaviour reference. Assuming this is a MonoBehaviour class:
            MonoBehaviour monoBehaviour = this;

            if (clipRainLight != null)
                BaseRainScript.audioSourceRainLight = new LoopingAudioSource(monoBehaviour, clipRainLight, rainAudioMixerGroup);

            if (clipRainMedium != null)
                BaseRainScript.audioSourceRainMedium = new LoopingAudioSource(monoBehaviour, clipRainMedium, rainAudioMixerGroup);

            if (clipRainHeavy != null)
                BaseRainScript.audioSourceRainHeavy = new LoopingAudioSource(monoBehaviour, clipRainHeavy, rainAudioMixerGroup);

            if (clipWind != null)
                BaseRainScript.audioSourceWind = new LoopingAudioSource(monoBehaviour, clipWind, rainAudioMixerGroup);

            BaseRainScript.audioSourceRainCurrent = BaseRainScript.audioSourceRainLight;

            if (rainMat == null) Logger.Log("rainmaterial.mat NOT found in AssetBundle.");
            if (rainExplosionMat == null) Logger.Log("rainexplosionmaterial.mat NOT found in AssetBundle.");
            if (rainMistMat == null) Logger.Log("rainmistmaterial.mat NOT found in AssetBundle.");
            if (rainShader == null) Logger.Log("rainshader.shader NOT found in AssetBundle.");
            if (clipRainLight == null) Logger.Log("rain_light.ogg NOT found in AssetBundle.");
            if (clipRainMedium == null) Logger.Log("rain_medium.ogg NOT found in AssetBundle.");
            if (clipRainHeavy == null) Logger.Log("rain_heavy.ogg NOT found in AssetBundle.");
            if (clipWind == null) Logger.Log("wind_normal1.ogg NOT found in AssetBundle.");
            if (rainTexture == null) Logger.Log("raintexture.png NOT found in AssetBundle.");
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
