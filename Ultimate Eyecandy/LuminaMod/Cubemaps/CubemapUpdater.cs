namespace Lumina
{
    using System.IO;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Gameobject to update cubemap states.
    /// Based on BloodyPenguin's original cubemap replacer.
    /// </summary>
    public class CubemapUpdater : MonoBehaviour
    {
        private static string CubemapOuterSpace = CubemapManager.Vanilla;

        // Current cubemaps.
        private Cubemap _currentDayCubemap;
        private Cubemap _currentNightCubemap;
        private Cubemap _currentOuterSpaceCubemap;

        // Cached references.
        private Cubemap _cachedDayCubemap;
        private Cubemap _cachedNightCubemap;
        private Cubemap _cachedOuterSpaceCubemap;

        // Vanilla cubemap references.
        private Cubemap _vanillaDayCubemap;
        private Cubemap _vanillaNightCubemap;
        private Cubemap _vanillaOuterSpaceCubemap;

        // Generated cubemap references.
        private Cubemap _generatedDayCubemap;
        private Cubemap _generatedNightCubemap;
        private Cubemap _generatedOuterSpaceCubemap;

        // Status flags.
        private bool _wasNight;
        private bool _dayCubemapUpdated = true;
        private bool _nightCubemapUpdated = false;

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        public static CubemapUpdater Instance { get; private set; }

        /// <summary>
        /// Called by Unity every frame.
        /// Used to check and update cubemaps.
        /// </summary>
        public void Update()
        {
            // Check for day/night changes.
            bool isNight = SimulationManager.instance.m_isNightTime;
            if (isNight != _wasNight)
            {
                // Set cubemap update flags if daytime has changed.
                _dayCubemapUpdated |= _wasNight;
                _nightCubemapUpdated |= isNight;
            }

            // Check for changes to daytime cubemap.
            if (_currentDayCubemap != _cachedDayCubemap)
            {
                _cachedDayCubemap = _currentDayCubemap;
                _dayCubemapUpdated = true;
            }

            // Check for changes to nighttime cubemap.
            if (_currentNightCubemap != _cachedNightCubemap)
            {
                _cachedNightCubemap = _currentNightCubemap;
                _nightCubemapUpdated = true;
            }

            // Apply any changes to outer space cubemap.
            if (_currentOuterSpaceCubemap != _cachedOuterSpaceCubemap)
            {
                Object.FindObjectOfType<DayNightProperties>().m_OuterSpaceCubemap = _currentOuterSpaceCubemap;
                _cachedOuterSpaceCubemap = _currentOuterSpaceCubemap;
            }

            // Check if any updates needed.
            if (!isNight & _dayCubemapUpdated)
            {
                // Daytime cubemap change.
                if (_cachedDayCubemap != null)
                {
                    Shader.SetGlobalTexture("_EnvironmentCubemap", _cachedDayCubemap);
                }

                // Reset flags.
                _dayCubemapUpdated = false;
                _wasNight = isNight;
            }
            else if (isNight & _nightCubemapUpdated)
            {
                // Nighttime cubemap change.
                if (_cachedNightCubemap != null)
                {
                    Shader.SetGlobalTexture("_EnvironmentCubemap", _cachedNightCubemap);
                }

                // Reset flags.
                _nightCubemapUpdated = false;
                _wasNight = isNight;
            }
        }

        /// <summary>
        /// Called by Unity when the object is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            // Reset cubemap.
            Object.FindObjectOfType<DayNightProperties>().m_OuterSpaceCubemap = _cachedOuterSpaceCubemap;
            Shader.SetGlobalTexture("_EnvironmentCubemap", _cachedDayCubemap);
        }

        /// <summary>
        /// Called by Unity after object is initialized.
        /// </summary>
        public void Awake()
        {
            // Set instance reference.
            Instance = this;

            // Record vanilla cubemaps.
            _vanillaDayCubemap = Object.FindObjectOfType<RenderProperties>().m_cubemap;
            _vanillaNightCubemap = Object.FindObjectOfType<RenderProperties>().m_cubemap;
            _vanillaOuterSpaceCubemap = Object.FindObjectOfType<DayNightProperties>().m_OuterSpaceCubemap;

            // Set initial cubemaps.
            SetDayCubemap(CubemapManager.DayCubeMap);
            SetNightCubemap(CubemapManager.NightCubeMap);
            SetOuterSpaceCubemap(CubemapOuterSpace);

            // Record initial day-night state.
            _wasNight = SimulationManager.instance.m_isNightTime;
        }

        /// <summary>
        /// Sets the active daytime cubemap.
        /// </summary>
        /// <param name="cubemapCode">Cubemap code.</param>
        public void SetDayCubemap(string cubemapCode)
        {
            // Don't do anything if not ready.
            if (CubemapManager.Instance?.DayCubemapDescriptions == null)
            {
                return;
            }

            // Apply vanilla cubemap if that's what we're doing.
            if (cubemapCode.Equals(CubemapManager.Vanilla))
            {
                DestroyCubemap(ref _generatedDayCubemap);
                _currentDayCubemap = _vanillaDayCubemap;
                return;
            }

            // Update daytime cubemap.
            _currentDayCubemap = GenerateCubemap(CubemapManager.Instance.GetDayReplacement(cubemapCode));
        }

        /// <summary>
        /// Sets the active nighttime cubemap.
        /// </summary>
        /// <param name="cubemapCode">Cubemap code.</param>
        public void SetNightCubemap(string cubemapCode)
        {
            // Don't do anything if not ready.
            if (CubemapManager.Instance?.DayCubemapDescriptions == null)
            {
                return;
            }

            // Apply vanilla cubemap if that's what we're doing.
            if (CubemapManager.Vanilla.Equals(cubemapCode))
            {
                DestroyCubemap(ref _generatedNightCubemap);
                _currentNightCubemap = _vanillaNightCubemap;
                return;
            }

            // Update daytime cubemap.
            _currentNightCubemap = GenerateCubemap(CubemapManager.Instance.GetNightReplacement(cubemapCode));
        }

        /// <summary>
        /// Sets the active outer space cubemap.
        /// </summary>
        /// <param name="cubemapCode">Cubemap code.</param>
        public void SetOuterSpaceCubemap(string cubemapCode)
        {
            // Don't do anything if not ready.
            if (CubemapManager.Instance?.DayCubemapDescriptions == null)
            {
                return;
            }

            // Apply vanilla cubemap if that's what we're doing.
            if (CubemapManager.Vanilla.Equals(cubemapCode))
            {
                DestroyCubemap(ref _generatedOuterSpaceCubemap);
                _currentOuterSpaceCubemap = _vanillaOuterSpaceCubemap;
                return;
            }

            // Update daytime cubemap.
            _currentOuterSpaceCubemap = GenerateCubemap(CubemapManager.Instance.GetNightReplacement(cubemapCode));
        }

        /// <summary>
        /// Destroys the given cubemap.
        /// </summary>
        private void DestroyCubemap(ref Cubemap customCubemap)
        {
            // Don't do anything if already destroyed.
            if (customCubemap == null)
            {
                return;
            }

            // Destroy cubemap.
            GameObject.Destroy(customCubemap);
            customCubemap = null;
        }

        /// <summary>
        /// Generates a cubemap from the provided cubemap replacement.
        /// </summary>
        /// <param name="replacement">Cubemap replacement.</param>
        /// <returns>Generated cubemap.</returns>
        private Cubemap GenerateCubemap(CubemapReplacement replacement)
        {
            // Destroy any active cubemaps.
            if (replacement.IsOuterSpace)
            {
                DestroyCubemap(ref _generatedOuterSpaceCubemap);
            }
            else
            {
                if (replacement.TimePeriod == "night" || replacement.IsNight)
                {
                    DestroyCubemap(ref _generatedNightCubemap);
                }
                else if (replacement.TimePeriod == "day" || !replacement.IsNight)
                {
                    DestroyCubemap(ref _generatedDayCubemap);
                }
            }

            // Create new cubemap.
            Cubemap cubemap = new Cubemap(replacement.Size, TextureFormat.ARGB32, true)
            {
                name = "LuminaCubemap",
                wrapMode = TextureWrapMode.Clamp
            };

            // Load cubemap textures.
            string prefix = replacement.FilePrefix;
            if (replacement.SplitFormat)
            {
                // Split texture format.
                Texture2D positiveX = Util.LoadTextureFromFile(Path.Combine(replacement.Directory, prefix + "posx.png"));
                SetCubemapFace(cubemap, positiveX, CubemapFace.PositiveX, 0, 2);
                Object.Destroy(positiveX);
                Texture2D positiveY = Util.LoadTextureFromFile(Path.Combine(replacement.Directory, prefix + "posy.png"));
                SetCubemapFace(cubemap, positiveY, CubemapFace.PositiveY, 0, 2);
                Object.Destroy(positiveY);
                Texture2D positiveZ = Util.LoadTextureFromFile(Path.Combine(replacement.Directory, prefix + "posz.png"));
                SetCubemapFace(cubemap, positiveZ, CubemapFace.PositiveZ, 0, 2);
                Object.Destroy(positiveZ);
                Texture2D negativeX = Util.LoadTextureFromFile(Path.Combine(replacement.Directory, prefix + "negx.png"));
                SetCubemapFace(cubemap, negativeX, CubemapFace.NegativeX, 0, 2);
                Object.Destroy(negativeX);
                Texture2D negativeY = Util.LoadTextureFromFile(Path.Combine(replacement.Directory, prefix + "negy.png"));
                SetCubemapFace(cubemap, negativeY, CubemapFace.NegativeY, 0, 2);
                Object.Destroy(negativeY);
                Texture2D negativeZ = Util.LoadTextureFromFile(Path.Combine(replacement.Directory, prefix + "negz.png"));
                SetCubemapFace(cubemap, negativeZ, CubemapFace.NegativeZ, 0, 2);
                Object.Destroy(negativeZ);
            }
            else
            {
                // Combined texture format.
                Texture2D combinedTexture = Util.LoadTextureFromFile(Path.Combine(replacement.Directory, prefix + "cubemap.png"));
                SetCubemapFace(cubemap, combinedTexture, CubemapFace.PositiveX, 2, 1);
                SetCubemapFace(cubemap, combinedTexture, CubemapFace.PositiveY, 1, 0);
                SetCubemapFace(cubemap, combinedTexture, CubemapFace.PositiveZ, 1, 1);
                SetCubemapFace(cubemap, combinedTexture, CubemapFace.NegativeX, 0, 1);
                SetCubemapFace(cubemap, combinedTexture, CubemapFace.NegativeY, 1, 2);
                SetCubemapFace(cubemap, combinedTexture, CubemapFace.NegativeZ, 3, 1);
                Object.Destroy(combinedTexture);
            }
            
            // Set cubemap texture.
            cubemap.anisoLevel = 9;
            cubemap.filterMode = FilterMode.Trilinear;
            cubemap.SmoothEdges();
            cubemap.Apply();

            // Record generated maps.
            if (replacement.IsOuterSpace)
            {
                _generatedOuterSpaceCubemap = cubemap;
            }
            else
            {
                if (replacement.TimePeriod == "night" || replacement.IsNight)
                {
                    _generatedNightCubemap = cubemap;
                }
                else if (replacement.TimePeriod == "day" || !replacement.IsNight)
                {
                    _generatedDayCubemap = cubemap;
                }
            }

            return cubemap;
        }

        /// <summary>
        /// Sets a cubemap face.
        /// </summary>
        /// <param name="cubemap">Cubemap.</param>
        /// <param name="texture">Face texture.</param>
        /// <param name="face">Face to set.</param>
        /// <param name="positionX">Texture X position.</param>
        /// <param name="positionY">Texture Y position.</param>
        private void SetCubemapFace(Cubemap cubemap, Texture2D texture, CubemapFace face, int positionX, int positionY)
        {
            // Iterate through each pixel and copy to cubemap.
            for (int x = 0; x < cubemap.width; ++x)
            {
                for (int y = 0; y < cubemap.height; ++y)
                {
                    int xPos = positionX * cubemap.width + x;
                    int yPos = (2 - positionY) * cubemap.height + (cubemap.height - y - 1);
                    Color pixelColor = texture.GetPixel(xPos, yPos);
                    cubemap.SetPixel(face, x, y, pixelColor);
                }
            }
        }
    }
}