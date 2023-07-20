using UnityEngine;

public class SunShaftsCompositeShaderProperties : MonoBehaviour
{
    public Shader sunShaftsCompositeShader;
    public Texture2D mainTexture;
    public Texture2D colorBuffer;
    public Texture2D skyboxTexture;
    public Color sunThreshold;
    public Color sunColor;
    public float blurRadius; // Use a single float for blurRadius
    public Vector4 sunPosition;
    public Vector2 mainTexTexelSize;
}
