using AlgernonCommons.UI;
using ColossalFramework.UI;
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using UnityEngine;
using UnityEngine.PostProcessing;
using static UnityEngine.PostProcessing.AmbientOcclusionModel;

namespace Lumina.Shaders.AO
{
    internal class AO : MonoBehaviour
    {
        private Camera _camera = null;
        private PostProcessingBehaviour _postProcessingBehaviour = null;
        public AmbientOcclusionModel _ambientOcclusionModel = null;

        public bool Initialized = false;
        private UIPanel _buttonPanel;

        private UITextureAtlas _LuminaAtlas;

        private UIButton _button;

        /// <summary>
        /// Initialize PostProcessing profile.
        /// </summary>
        public void Start()
        {
            try
            {
                if (_camera == null)
                {
                    _camera = Camera.main;
                    if (_camera == null)
                    {
                        throw new Exception("No camera found in the scene.");
                    }
                }

                AssetManager.Instance.AssetBundle = AssetBundleUtils.LoadAssetBundle("renderit");

                if (_postProcessingBehaviour == null)
                {
                    _postProcessingBehaviour = _camera.gameObject.GetComponent<PostProcessingBehaviour>();
                    if (_postProcessingBehaviour == null)
                    {
                        _postProcessingBehaviour = _camera.gameObject.AddComponent<PostProcessingBehaviour>();
                        _postProcessingBehaviour.profile = ScriptableObject.CreateInstance<PostProcessingProfile>();
                    }
                }

                if (_postProcessingBehaviour == null || _postProcessingBehaviour.profile == null)
                {
                    throw new Exception("PostProcessingBehaviour or its profile is null.");
                }

                _postProcessingBehaviour.enabled = true;

                if (_ambientOcclusionModel == null)
                {
                    _ambientOcclusionModel = new AmbientOcclusionModel();
                }

                // Setting Ambient Occlusion settings
                _postProcessingBehaviour.profile.ambientOcclusion.enabled = true;
                _ambientOcclusionModel.enabled = true;
                AmbientOcclusionModel.Settings settings = _ambientOcclusionModel.settings;

                // Adjusting Ambient Occlusion intensity and radius
                settings.intensity = 4f;
                settings.radius = 0.3f;
                settings.sampleCount = SampleCount.Medium;
                settings.downsampling = true;
                settings.forceForwardCompatibility = false;
                settings.ambientOnly = false;
                settings.highPrecision = false;

                // Applying modified settings
                _postProcessingBehaviour.profile.ambientOcclusion.settings = settings;
                _LuminaAtlas = LoadResources();

                Initialized = true;
                Debug.Log("[LUMINA] Successfully loaded AO Shader.");
                Debug.Log("[LUMINA] Ambient Occlusion enabled.");
                CreateUI();
                Debug.Log("[LUMINA] Called CreateUI().");
                UpdateUI();
                Debug.Log("[LUMINA] Called UpdateUI().");
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred in Start(): {ex.Message}");
            }
        }

        public static UITextureAtlas CreateTextureAtlas(string atlasName, string[] spriteNames, string assemblyPath)
        {
            int maxSize = 1024;
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            Texture2D[] textures = new Texture2D[spriteNames.Length];
            Rect[] regions = new Rect[spriteNames.Length];

            for (int i = 0; i < spriteNames.Length; i++)
            {
                textures[i] = LoadTextureFromAssembly(assemblyPath + spriteNames[i] + ".png");
            }

            regions = texture2D.PackTextures(textures, 2, maxSize);

            UITextureAtlas textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            Material material = UnityEngine.Object.Instantiate(UIView.GetAView().defaultAtlas.material);
            material.mainTexture = texture2D;
            textureAtlas.material = material;
            textureAtlas.name = atlasName;

            for (int i = 0; i < spriteNames.Length; i++)
            {
                UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo
                {
                    name = spriteNames[i],
                    texture = textures[i],
                    region = regions[i],
                };

                textureAtlas.AddSprite(item);
            }

            return textureAtlas;
        }

        public static Texture2D LoadTextureFromAssembly(string path)
        {
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            byte[] array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);

            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D.LoadImage(array);

            return texture2D;
        }

        public static UITextureAtlas GetAtlas(string name)
        {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++)
            {
                if (atlases[i].name == name)
                    return atlases[i];
            }

            return UIView.GetAView().defaultAtlas;
        }

        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false)
        {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++)
            {
                Texture2D texture2D = atlas.sprites[i].texture;

                if (locked)
                {
                    RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    RenderTexture active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
            {
                textures[atlas.count + i] = newTextures[i];
            }

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++)
            {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo
                {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        private UITextureAtlas LoadResources()
        {
            try
            {
                if (_LuminaAtlas == null)
                {
                    string[] spriteNames = new string[]
                    {
                        "ADV"
                    };

                    _LuminaAtlas = CreateTextureAtlas("LuminaAtlas", spriteNames, "Lumina.Resources.");

                    UITextureAtlas defaultAtlas = GetAtlas("Ingame");
                    Texture2D[] textures = new Texture2D[]
                    {
                        defaultAtlas["OptionLandscapingDisabled"].texture,
                        defaultAtlas["OptionBaseFocused"].texture,
                        defaultAtlas["OptionLandscapingHovered"].texture,
                        defaultAtlas["OptionLandscapingPressed"].texture,
                        defaultAtlas["LandscapingOptionBrushLargeDisabled"].texture
                    };

                    AddTexturesInAtlas(_LuminaAtlas, textures);
                }

                return _LuminaAtlas;
            }
            catch (Exception e)
            {
                Debug.Log("[LUMINA] ModManager:LoadResources -> Exception: " + e.Message);
                return null;
            }
        }


        public void Update()
        {
            try
            {
                if (Initialized)
                UpdateAmbientOcclusion();
            }
            catch (Exception e)
            {
                Debug.Log("[LUMINA] ModManager:Update -> Exception: " + e.Message);
            }
        }

        public static UIPanel CreatePanel(string name)
        {
            UIPanel panel = UIView.GetAView()?.AddUIComponent(typeof(UIPanel)) as UIPanel;
            panel.name = name;

            return panel;
        }

        public static UIButton CreateButton(UIComponent parent, string name, UITextureAtlas atlas, string spriteName)
        {
            UIButton button = parent.AddUIComponent<UIButton>();
            button.name = name;
            button.atlas = atlas;

            button.normalBgSprite = "OptionLandscapingDisabled";
            button.hoveredBgSprite = "OptionLandscapingHovered";
            button.pressedBgSprite = "OptionLandscapingPressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            button.normalFgSprite = spriteName;
            button.hoveredFgSprite = spriteName;
            button.pressedFgSprite = spriteName;
            button.disabledFgSprite = spriteName;

            return button;
        }



        private void UpdateUI()
        {
            try
            {
                _buttonPanel.isVisible = LuminaLogic.ShowButton;
                _buttonPanel.absolutePosition = new Vector3(LuminaLogic.ButtonPositionX, LuminaLogic.ButtonPositionY);
            }
            catch (Exception e)
            {
                Debug.Log("[LUMINA] ModManager:UpdateUI -> Exception: " + e.Message);
            }
        }

        private void CreateUI()
        {
            try
            {
                _buttonPanel = CreatePanel("LuminaButtonPanel");
                _buttonPanel.isVisible = LuminaLogic.ShowButton;
                _buttonPanel.zOrder = 25;
                _buttonPanel.size = new Vector2(36f, 36f);
                _buttonPanel.eventMouseMove += (component, eventParam) =>
                {
                    if (eventParam.buttons.IsFlagSet(UIMouseButton.Right))
                    {
                        var ratio = UIView.GetAView().ratio;
                        component.position = new Vector3(component.position.x + (eventParam.moveDelta.x * ratio), component.position.y + (eventParam.moveDelta.y * ratio), component.position.z);
                        LuminaLogic.ButtonPositionX = component.absolutePosition.x;
                        LuminaLogic.ButtonPositionY = component.absolutePosition.y;
                        ModSettings.Save();

                    }
                };

                _button = CreateButton(_buttonPanel, "LuminaInButton", _LuminaAtlas, "ADV");
                _button.tooltip = "Lumina";
                _button.size = new Vector2(36f, 36f);
                _button.relativePosition = new Vector3(0f, 0f);
                _button.eventClick += (component, eventParam) =>
                {
                    if (!eventParam.used)
                    {
                  

                            StandalonePanelManager<LuminaPanel>.Create();
                    }
                    else
                    {
                        StandalonePanelManager<LuminaPanel>.Panel?.Close();
                    }


                    eventParam.Use();
                };


            }
            catch (Exception e)
            {

                Debug.Log("[LUMINA] ModManager:CreateUI -> Error: " + e.Message);
            }
        }

        private void UpdateAmbientOcclusion()
        {
            try
            {
                AmbientOcclusionModel.Settings settings = _ambientOcclusionModel.settings;

                settings.intensity = LuminaLogic.AOIntensity;
                settings.radius = LuminaLogic.AORadius;
                _ambientOcclusionModel.settings = settings;
                _ambientOcclusionModel.enabled = true;

                _postProcessingBehaviour.profile.ambientOcclusion.settings = _ambientOcclusionModel.settings;
                _postProcessingBehaviour.profile.ambientOcclusion.enabled = _ambientOcclusionModel.enabled;
            }
            catch (Exception e)
            {
                Debug.Log("[LUMINA] ModManager:UpdateAmbientOcclusion -> Exception: " + e.Message);
            }
        }
    }
}
