using System;
using Lumina;
using UnityEngine;


/*
 * The ShaderStructure class encapsulates the structure of shaders used in this project. The complete code for ShaderStructure has been sourced from the Dynamic Resolution project, available at:
 * https://github.com/d235j/Skylines-DynamicResolution/tree/cc7d04df204b74c1ba781dc3a5f492ba30ce6b61
 * This class plays a crucial role in defining the structure and behavior of shaders within our Lumina project. By incorporating the Dynamic Resolution code, we leverage proven implementations and practices, fostering code reuse and maintaining consistency with the Dynamic Resolution project's shader-related functionality. Any modifications or enhancements to the ShaderStructure class should be carefully synchronized with the corresponding code in the Dynamic Resolution repository to ensure compatibility and benefit from ongoing improvements made by the Dynamic Resolution project.
 */


namespace Lumina { 
public class CameraHook : MonoBehaviour
{

    public static CameraHook instance = null;

#if DEBUG
    public static Rect mainCameraRect;
    public static Rect undergroundCameraRect;
#endif

    private RenderTexture rt;

        public float userSSAAFactor = 1f;
        public float currentSSAAFactor { get; set; }

        public float sliderMaximum = 3.0f;

    private bool initialized = false;

    public Rect cameraPixelRect;

    private GameObject dummyGameObject;
    private CameraRenderer cameraRenderer;

    public bool showConfigWindow = false;

#if DEBUG
    private Rect windowRect = new Rect(64, 64, 350, 230);
#else
    private Rect windowRect = new Rect(64, 64, 350, 170);
#endif




    public CameraController cameraController;
  

        private Texture2D bgTexture;
    private GUISkin skin;

    private float dtAccum = 0.0f;
    private int frameCount = 0;
    private float fps = 0.0f;

    void OnDestroy()
    {
        GetComponent<Camera>().enabled = true;
        Destroy(dummyGameObject);
    }

    public void Awake()
    {
        instance = this;

        currentSSAAFactor = userSSAAFactor = ShaderStructure.ssaaFactor;
        SaveConfig();

        cameraController = FindObjectOfType<CameraController>();

        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, Color.grey);
        bgTexture.Apply();
    }

    public void SaveConfig()
    {
       
        ShaderStructure.ssaaFactor = userSSAAFactor;
     
            ModSettings.Save();
    }

    public void SetInGameAA(bool state)
    {
        var camera = gameObject.GetComponent<Camera>();
        if (!state)
        {
            if (camera.GetComponent<SMAA>() != null)
            {
                Destroy(camera.gameObject.GetComponent<SMAA>());
            }
        }
        else
        {
            if (camera.GetComponent<SMAA>() == null)
            {
                camera.gameObject.AddComponent<SMAA>();
            }
        }
    }

    public int width
    {
        get { return (int)(Screen.width * currentSSAAFactor); }
    }
    public int height
    {
        get { return (int)(Screen.height * currentSSAAFactor); }
    }

    public int internalWidth
    {
        get { return (int)(cameraPixelRect.width * currentSSAAFactor); }
    }
    public int internalHeight
    {
        get { return (int)(cameraPixelRect.height * currentSSAAFactor); }
    }

    public void SetSSAAFactor(float factor, bool lowerVRAMUsage)
    {
        var width = Screen.width * factor;
        var height = Screen.height * factor;

        Destroy(rt);
        rt = new RenderTexture((int)width, (int)height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

        var hook = dummyGameObject.GetComponent<CameraRenderer>();
        hook.fullResRT = rt;

        if (hook.halfVerticalResRT != null)
        {
            Destroy(hook.halfVerticalResRT);
        }

        if (!lowerVRAMUsage)
        {
            hook.halfVerticalResRT = new RenderTexture(Screen.width, (int)height, 0);
        }
        else
        {
            hook.halfVerticalResRT = null;
        }

        Destroy(CameraRenderer.mainCamera.targetTexture);
        CameraRenderer.mainCamera.targetTexture = rt;

        currentSSAAFactor = factor;
        userSSAAFactor = factor;

        initialized = true;
    }

    public void Initialize()
    {
        SetInGameAA(false);

        var camera = gameObject.GetComponent<Camera>();
        cameraPixelRect = camera.pixelRect;
        camera.enabled = false;

        var width = Screen.width * userSSAAFactor;
        var height = Screen.height * userSSAAFactor;
        rt = new RenderTexture((int)width, (int)height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

        dummyGameObject = new GameObject();
        var dummy = dummyGameObject.AddComponent<Camera>();
        dummy.cullingMask = 0;
        dummy.depth = -3;
        dummy.tag = "MainCamera";
        dummy.pixelRect = cameraPixelRect;

        cameraRenderer = dummyGameObject.AddComponent<CameraRenderer>();
        cameraRenderer.fullResRT = rt;
        cameraRenderer.halfVerticalResRT = new RenderTexture(Screen.width, (int)height, 0);

        CameraRenderer.mainCamera = camera;

        CameraRenderer.mainCamera.targetTexture = null;
        CameraRenderer.mainCamera.pixelRect = cameraPixelRect;

        currentSSAAFactor = userSSAAFactor;
        initialized = true;

        SaveConfig();
    }

    void Update()
    {
        frameCount++;
        dtAccum += Time.deltaTime;

        if (dtAccum >= 1.0f)
        {
            fps = frameCount;
            dtAccum = 0.0f;
            frameCount = 0;
        }

        if (!initialized)
        {
            Initialize();
        }

        if (Input.GetKeyDown(KeyCode.F10) || (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R)))
        {
            showConfigWindow = !showConfigWindow;
        }
    }

    void DoConfigWindow(int wnd)
    {
        var width = cameraPixelRect.width * userSSAAFactor;
        var height = cameraPixelRect.height * userSSAAFactor;

        GUILayout.Label(String.Format("Internal resolution: {0}x{1}", (int)width, (int)height));
        GUILayout.BeginHorizontal();

        sliderMaximum = 3.0f;

        switch (ShaderStructure.sliderMaximumIndex)
        {
            case 2:
                sliderMaximum = 5.0f;
                break;
            case 1:
                sliderMaximum = 4.0f;
                break;
        }

        userSSAAFactor = GUILayout.HorizontalSlider(userSSAAFactor, 0.25f, sliderMaximum, GUILayout.Width(256));

        if (!ShaderStructure.unlockSlider)
        {
            if (userSSAAFactor <= 0.25f)
            {
                userSSAAFactor = 0.25f;
            }
            else if (userSSAAFactor <= 0.50f)
            {
                userSSAAFactor = 0.50f;
            }
            else if (userSSAAFactor <= 0.75f)
            {
                userSSAAFactor = 0.75f;
            }
            else if (userSSAAFactor <= 1.0f)
            {
                userSSAAFactor = 1.0f;
            }
            else if (userSSAAFactor <= 1.5f)
            {
                userSSAAFactor = 1.5f;
            }
            else if (userSSAAFactor <= 1.75f)
            {
                userSSAAFactor = 1.75f;
            }
            else if (userSSAAFactor <= 2.0f)
            {
                userSSAAFactor = 2.0f;
            }
            else if (userSSAAFactor <= 2.5f)
            {
                userSSAAFactor = 2.5f;
            }
            else if (userSSAAFactor <= 3.0f)
            {
                userSSAAFactor = 3.0f;
            }
            else if (userSSAAFactor <= 3.5f)
            {
                userSSAAFactor = 3.5f;
            }
            else if (userSSAAFactor <= 4.0f)
            {
                userSSAAFactor = 4.0f;
            }
            else if (userSSAAFactor <= 4.5f)
            {
                userSSAAFactor = 4.5f;
            }
            else if (userSSAAFactor <= 5.0f)
            {
                userSSAAFactor = 5.0f;
            }
        }

        GUILayout.Label(String.Format("{0} %", (int)(userSSAAFactor * 100.0f)));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Unlock slider (may degrade quality)");
        var unlockSlider = GUILayout.Toggle(ShaderStructure.unlockSlider, "");
        GUILayout.EndHorizontal();

        if (unlockSlider != ShaderStructure.unlockSlider)
        {
            ShaderStructure.unlockSlider = unlockSlider;
            SaveConfig();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Lower VRAM usage (will degrade quality)");
        var lowerRAMUsage = GUILayout.Toggle(ShaderStructure.lowerVRAMUsage, "");
        GUILayout.EndHorizontal();

        if (lowerRAMUsage != ShaderStructure.lowerVRAMUsage)
        {
            ShaderStructure.lowerVRAMUsage = lowerRAMUsage;
            SaveConfig();
        }

#if DEBUG
        GUILayout.BeginHorizontal();
        GUILayout.Label("Main camera rect: " + DebugFormatRect(mainCameraRect));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Underground camera rect: " + DebugFormatRect(undergroundCameraRect));
        GUILayout.EndHorizontal();
#endif

        GUILayout.Label("FPS: " + fps);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset"))
        {
            ShaderStructure.lowerVRAMUsage = false;
            SetSSAAFactor(1.0f, ShaderStructure.lowerVRAMUsage);
            userSSAAFactor = 1.0f;
            SaveConfig();
        }

        if (GUILayout.Button("Apply"))
        {
            SetSSAAFactor(userSSAAFactor, ShaderStructure.lowerVRAMUsage);
            SaveConfig();
        }

        GUILayout.EndHorizontal();
    }
    }

#if DEBUG
    private string DebugFormatRect(Rect rect)
    {
        if (rect == null)
        {
            return "(null)";
        }
        return "x=" + rect.x + " y=" + rect.y + " " + rect.width + "x" + rect.height;
    }
#endif
}



