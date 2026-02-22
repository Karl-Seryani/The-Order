using UnityEngine;

namespace TheOrder.Player
{
    /// <summary>
    /// Very subtle forward-facing ambient light on the camera.
    /// Like weak night vision — just enough to see what's ahead, fades to black at edges.
    /// </summary>
    public class PlayerAmbientLight : MonoBehaviour
    {
        [Header("Light Settings")]
        [SerializeField] private float _intensity = 5f;
        [SerializeField] private float _range = 12f;
        [SerializeField] private float _spotAngle = 140f;
        [SerializeField] private float _innerSpotAngle = 130f;
        [SerializeField] private Color _color = new Color(0.6f, 0.65f, 0.75f, 1f);

        private void Start()
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam == null) return;

            var lightGO = new GameObject("PlayerAmbientLight_Runtime");
            lightGO.transform.SetParent(cam.transform, false);
            lightGO.transform.localPosition = Vector3.zero;
            lightGO.transform.localRotation = Quaternion.identity;

            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Spot;
            light.intensity = _intensity;
            light.range = _range;
            light.spotAngle = _spotAngle;
            light.innerSpotAngle = _innerSpotAngle;
            light.color = _color;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
            light.lightmapBakeType = LightmapBakeType.Realtime;
        }
    }
}
