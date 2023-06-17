using System;
using System.Diagnostics;
using System.Reflection;
using ICities;
using HarmonyLib;
using LuminaTR.TranslationFramework;
using UnityEngine;

namespace Lumina
{
    public class LuminaMod : LoadingExtensionBase, IUserMod
    {
        private GameObject gameobj;
        private static LuminaLogic logic;

        public string Name { get { return "Lumina"; } }

        public string Description { get { return Translation.Instance.GetTranslation(LuminaTR.Locale.TranslationID.MOD_DESCRIPTION); } }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            OnEnabled();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (gameobj != null)
            {
                UnityEngine.Object.Destroy(gameobj);
                gameobj = null;
                logic = null;
            }

        }

        private Harmony harmony;

        private void OpenLUTEditor()
        {
            string lutEditorPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\255710\2983036781\LUT Editor\";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = lutEditorPath;
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }

        public void OnEnabled()
        {

            while (true)
            {
                var obj = GameObject.Find("Lumina");
                if (!obj) break;
                GameObject.DestroyImmediate(obj);
            }
            gameobj = new GameObject("Lumina");
            logic = gameobj.AddComponent<LuminaLogic>();

            harmony = new Harmony("com.nyoko.lumina.patch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnDisabled()
        {
            while (true)
            {
                var obj = GameObject.Find("Lumina");
                if (!obj) break;

                GameObject.DestroyImmediate(obj);
            }
            harmony = null;
        }

        public class Raycaster : ToolBase
        {
            public static float RayDistance(float angle, Vector3 transform)
            {
                Vector3 direction = Quaternion.AngleAxis(angle, transform) * Camera.main.transform.forward;
                //Vector3 direction = Camera.main.transform.forward;
                ToolBase.RaycastInput input = new ToolBase.RaycastInput(new Ray(Camera.main.transform.position, direction), Camera.main.farClipPlane);
                input.m_ignoreBuildingFlags = Building.Flags.None;
                input.m_ignoreNodeFlags = NetNode.Flags.None;
                input.m_ignoreSegmentFlags = NetSegment.Flags.None;
                input.m_ignorePropFlags = PropInstance.Flags.None;
                input.m_buildingService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
                input.m_netService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
                input.m_netService2 = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
                input.m_propService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
                ToolBase.RaycastOutput rayOutput;
                if (ToolBase.RayCast(input, out rayOutput)) return Vector3.Distance(Camera.main.transform.position, rayOutput.m_hitPos);
                else return 1000f;
            }
        }

        public static float GetShadowBias()
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

            if (LuminaLogic.forceLowBias) return Mathf.Clamp(final_bias, 0.20f, 1f) - 0.19f;
            return final_bias;
        }
    }
}