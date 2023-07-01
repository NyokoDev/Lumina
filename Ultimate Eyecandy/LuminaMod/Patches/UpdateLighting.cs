namespace Lumina.Patches
{
    using System;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(DayNightProperties))]
    [HarmonyPatch("UpdateLighting")]
    class UpdateLighting
    {
        private static bool s_forceLowBias = false;

        /// <summary>
        /// Gets or sets a value indicating whether shadow bias should be forced to a low value.
        /// </summary>
        internal static bool ForceLowBias { get => s_forceLowBias; set => s_forceLowBias = value; }

        public static void Postfix()
        {
            RenderManager.instance.MainLight.shadowBias = GetShadowBias();
        }


        private static float GetShadowBias()
        {
            var rayDistance = Raycaster.RayDistance(0, Camera.main.transform.forward);
            var rayDistanceMax = rayDistance;

            rayDistance = Raycaster.RayDistance(10, Camera.main.transform.right);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(-10, Camera.main.transform.right);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(10, Camera.main.transform.up);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(20, Camera.main.transform.up);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(-10, Camera.main.transform.up);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(-20, Camera.main.transform.up);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistanceMax = Mathf.Clamp(rayDistanceMax, 0f, 1000f);
            if (rayDistanceMax == 0) rayDistanceMax = 1000;

            var heightMultiplier = 0.6f; // multiplier - how much height affects bias
            var heightUpper = 10f; // divider - affects height above which shadow bias is no longer relevant
            var heightLower = 20f; // subtracter - affects height below which shadow bias is no longer relevant
            var heightAngleRatio = 0.65f; // multiplier (0-1) - higher values means angle affects shadow bias more at increased height
            var angleBaseAffect = 0.15f; // lower limit for how much angle affects bias
            var rayAffect = 16; // divider (1/value) - how much raycasting is mixed into the height value
            var cameraController = ToolsModifierControl.cameraController;

            // calculates relevant height by subtracting terrain height from camera height
            var height = cameraController.transform.position.y - cameraController.m_currentHeight;
            if (rayDistanceMax > height) height = ((rayAffect - 1) * height + rayDistanceMax) / rayAffect;

            // calculates camera angle: 0 = at horizon, 90 = down
            var angle = cameraController.m_targetAngle.y; if (angle > 90f || angle < 0f) angle = 0;

            // calculates relevant angle, where we're looking: 0 = down, 1 = at horizon
            var calc_angle = 1 - (angle / 90f);

            // calculates relevant height: 0 = close to ground, 1 = very high
            var calc_height = Mathf.Clamp(((height - heightLower) * heightMultiplier / heightUpper), 0f, 100f) / 100;

            // calculates bias, increasing height and angle increases leads to higher values
            var calc_bias = calc_height + (((calc_height * heightAngleRatio) + angleBaseAffect) * calc_angle);

            // makes bias increase more quickly when going above 0.2
            var exp_bias = Math.Pow((calc_bias + 0.8f), 1.5f) - 0.8f;

            // clamps bias to lowest and highest useful values
            var final_bias = Mathf.Clamp(Convert.ToSingle(exp_bias), 0.1f, 1f);

            if (s_forceLowBias) return Mathf.Clamp(final_bias, 0.20f, 1f) - 0.19f;
            return final_bias;
        }
    }
}
