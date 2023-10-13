using UnityEngine;

public class AdvancedLogic 
{
    public static float m_Exposure
    {
        get
        {
            DayNightProperties dayNightProperties = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            return (dayNightProperties != null && dayNightProperties.enabled) ? dayNightProperties.m_Exposure : 0.1f;
        }
        set
        {
            float clampedValue = Mathf.Clamp(value, 0.0f, 5.0f); // Clamp the value between 0 and 5
            DayNightProperties dayNightProperties = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            if (dayNightProperties != null)
            {
                dayNightProperties.m_Exposure = clampedValue;
            }
        }
    }

    public static float SkyRayleighScattering
    {
        get
        {
            DayNightProperties dayNightProperties = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            return (dayNightProperties != null && dayNightProperties.enabled) ? dayNightProperties.m_RayleighScattering : 0.0f;
        }
        set
        {
            float clampedValue = Mathf.Clamp(value, 0.0f, 5.0f); // Clamp the value between 0 and 5
            DayNightProperties dayNightProperties = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            if (dayNightProperties != null)
            {
                dayNightProperties.m_RayleighScattering = clampedValue;
            }
        }
    }

    public static float SkyMieScattering
    {
        get
        {
            DayNightProperties dayNightProperties = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            return (dayNightProperties != null && dayNightProperties.enabled) ? dayNightProperties.m_MieScattering : 0.0f;
        }
        set
        {
            float clampedValue = Mathf.Clamp(value, 0.0f, 5.0f); // Clamp the value between 0 and 5
            DayNightProperties dayNightProperties = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            if (dayNightProperties != null)
            {
                dayNightProperties.m_MieScattering = clampedValue;
            }
        }
    }

    public static float DayNightSunIntensity
    {
        get
        {
            DayNightProperties dayNightProperties = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            return (dayNightProperties != null && dayNightProperties.enabled) ? dayNightProperties.m_SunIntensity : 0.1f;
        }
        set
        {
            float clampedValue = Mathf.Clamp(value, 0.0f, 8.0f); // Clamp the value between 0 and 8
            DayNightProperties dayNightProperties = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            if (dayNightProperties != null)
            {
                dayNightProperties.m_SunIntensity = clampedValue;
            }
        }
    }


        
        
        // Custom property to control Time.timeScale
    public static float CustomTimeScale
    {
        get
        {
            return Time.timeScale;
        }
        set
        {
            Time.timeScale = value;
        }
    }



}
