namespace Lumina
{
    using UnityEngine;

    /// <summary>
    /// Simple class to access ToolBase's raycaster.
    /// </summary>
    public class Raycaster : ToolBase
    {
        /// <summary>
        /// Retuns the raycast distance for the given angle and transform.
        /// </summary>
        /// <param name="angle">Ray angle.</param>
        /// <param name="transform">Ray transform.</param>
        /// <returns>Raycast distance (1000f if raycast failed).</returns>
        public static float RayDistance(float angle, Vector3 transform)
        {
            Vector3 direction = Quaternion.AngleAxis(angle, transform) * Camera.main.transform.forward;

            RaycastInput input = new ToolBase.RaycastInput(new Ray(Camera.main.transform.position, direction), Camera.main.farClipPlane);
            input.m_ignoreBuildingFlags = Building.Flags.None;
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            input.m_ignorePropFlags = PropInstance.Flags.None;
            input.m_buildingService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_netService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_netService2 = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_propService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);

            if (RayCast(input, out RaycastOutput rayOutput))
            {
                return Vector3.Distance(Camera.main.transform.position, rayOutput.m_hitPos);
            }
            return 1000f;
        }
    }
}
