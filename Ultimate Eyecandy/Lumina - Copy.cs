using ICities;
using UnityEngine;

public class ShadowEffect : MonoBehaviour
{

    public Light directionalLight;
    public float shadowIntensity = 0.5f;
    public float shadowBias = 0.1f;

    public void OnEnabled()
    {
        UpdateShadowSettings();
    }

    public void OnDisabled()
    {
        ResetShadowSettings();
    }

    private void Update()
    {
        UpdateShadowSettings();
    }

    private void UpdateShadowSettings()
    {
        if (directionalLight != null)
        {
            directionalLight.shadowStrength = shadowIntensity;
            directionalLight.shadowBias = shadowBias;
        }
    }

    private void ResetShadowSettings()
    {
        if (directionalLight != null)
        {
            directionalLight.shadowStrength = 1f; // Reset to default shadow strength
            directionalLight.shadowBias = 0.05f; // Reset to default shadow bias
        }
    }
}
