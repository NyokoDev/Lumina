using UnityEngine;

namespace Lumina.Helpers
{
    /// <summary>
    /// Time Manager class.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        void Update()
        {
        OnUpdate();
        }

        void OnUpdate()
        {
            EffectsTab.UpdateNightFog();
        }
        void Start()
        {
            Logger.Log("TimeManager attached.");
        }
    }
}
