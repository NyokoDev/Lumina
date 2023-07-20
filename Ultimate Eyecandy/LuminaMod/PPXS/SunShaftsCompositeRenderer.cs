using UnityEngine;

public class SunShaftsCompositeRenderer : MonoBehaviour
{
    public SunShaftsCompositeShaderProperties shaderProperties;
    public string shaderPath = "PPXS/SunShaftsComposite"; // The path to the shader inside the "PPXS" folder

    private Material material;

    public void Start()
    {
        if (shaderProperties == null)
        {
            Debug.LogError("SunShaftsCompositeRenderer: Shader properties not set properly!");
            return;
        }

        // Load the shader from the "PPXS" folder using Resources.Load
        Shader sunShaftsCompositeShader = Resources.Load<Shader>(shaderPath);

        if (sunShaftsCompositeShader == null)
        {
            Debug.LogError("SunShaftsCompositeRenderer: Failed to load the shader from " + shaderPath);
            return;
        }

        // Assign the loaded shader to the shaderProperties
        shaderProperties.sunShaftsCompositeShader = sunShaftsCompositeShader;

        // Create the material using the loaded shader
        material = new Material(shaderProperties.sunShaftsCompositeShader);

        // Set the shader properties
        material.SetTexture("_MainTex", shaderProperties.mainTexture);
        material.SetTexture("_ColorBuffer", shaderProperties.colorBuffer);
        material.SetTexture("_Skybox", shaderProperties.skyboxTexture);
        material.SetColor("_SunThreshold", shaderProperties.sunThreshold);
        material.SetColor("_SunColor", shaderProperties.sunColor);

        // Set the initial value of the blurRadius property in the material
        material.SetFloat("_BlurRadius", shaderProperties.blurRadius);

        // Other properties...
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Apply the material and draw the image
        Graphics.Blit(source, destination, material);
    }
}
