using UnityEngine;

public class FXAAController : MonoBehaviour
{
    // Public variables for shader properties
    public Vector2 pos;
    public Vector4 fxaaConsolePosPos;
    public Texture tex;
    public Texture fxaaConsole360TexExpBiasNegOne;
    public Texture fxaaConsole360TexExpBiasNegTwo;
    public Vector2 fxaaQualityRcpFrame;
    public Vector4 fxaaConsoleRcpFrameOpt;
    public Vector4 fxaaConsoleRcpFrameOpt2;
    public Vector4 fxaaConsole360RcpFrameOpt2;
    public float fxaaQualitySubpix;
    public float fxaaQualityEdgeThreshold;
    public float fxaaQualityEdgeThresholdMin;
    public float fxaaConsoleEdgeSharpness;
    public float fxaaConsoleEdgeThreshold;
    public float fxaaConsoleEdgeThresholdMin;
    public Vector4 fxaaConsole360ConstDir;

    // Reference to the custom shader
    public Shader FXAAPixelShader;

    // Reference to the material using the custom shader
    private Material material;

    public void Start()
    {
        // Load the custom shader from the Resources folder
        FXAAPixelShader = Shader.Find("PPXS/FXAA3");

        if (FXAAPixelShader == null)
        {
            Debug.LogError("FXAAPixelShader not found. Make sure the shader is named correctly and placed in the project.");
            return;
        }

        // Create a new material using the custom shader
        material = new Material(FXAAPixelShader);

        // Set shader properties using public variables
        material.SetVector("pos", pos);
        material.SetVector("fxaaConsolePosPos", fxaaConsolePosPos);
        material.SetTexture("tex", tex);
        material.SetTexture("fxaaConsole360TexExpBiasNegOne", fxaaConsole360TexExpBiasNegOne);
        material.SetTexture("fxaaConsole360TexExpBiasNegTwo", fxaaConsole360TexExpBiasNegTwo);
        material.SetVector("fxaaQualityRcpFrame", fxaaQualityRcpFrame);
        material.SetVector("fxaaConsoleRcpFrameOpt", fxaaConsoleRcpFrameOpt);
        material.SetVector("fxaaConsoleRcpFrameOpt2", fxaaConsoleRcpFrameOpt2);
        material.SetVector("fxaaConsole360RcpFrameOpt2", fxaaConsole360RcpFrameOpt2);
        material.SetFloat("fxaaQualitySubpix", fxaaQualitySubpix);
        material.SetFloat("fxaaQualityEdgeThreshold", fxaaQualityEdgeThreshold);
        material.SetFloat("fxaaQualityEdgeThresholdMin", fxaaQualityEdgeThresholdMin);
        material.SetFloat("fxaaConsoleEdgeSharpness", fxaaConsoleEdgeSharpness);
        material.SetFloat("fxaaConsoleEdgeThreshold", fxaaConsoleEdgeThreshold);
        material.SetFloat("fxaaConsoleEdgeThresholdMin", fxaaConsoleEdgeThresholdMin);
        material.SetVector("fxaaConsole360ConstDir", fxaaConsole360ConstDir);
    }

    void Update()
    {
        // Apply the material to the current object's renderer
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }
}
