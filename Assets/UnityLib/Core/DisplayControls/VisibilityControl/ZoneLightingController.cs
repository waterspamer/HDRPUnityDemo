using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;

namespace Nettle {

public class ZoneLightingController : MonoBehaviour {
    [Serializable]
    public class ZoneLightingSettings {
        public AmbientMode AmbientSource;
        public Color AmbientColor;
        public float AmbientIntensity;
        public Color LightColor;
        public float LightIntensity;
        public DefaultReflectionMode ReflectionMode;
        public Cubemap CustomReflectionCubemap;
        public Material SkyboxMaterial;
        public VisibilityZone[] Zones;
    }

    public VisibilityZoneViewer Viewer;
    public Light MainLight;
    public ZoneLightingSettings[] Settings;

    private ZoneLightingSettings _startSettings;

    private void Start() {
        _startSettings = new ZoneLightingSettings();

        _startSettings.AmbientColor = RenderSettings.ambientLight;
        _startSettings.AmbientIntensity = RenderSettings.ambientIntensity;
        _startSettings.SkyboxMaterial = RenderSettings.skybox;


        if (!MainLight) { return; }

        _startSettings.LightColor = MainLight.color;
        _startSettings.LightIntensity = MainLight.intensity;

        if (Viewer == null) {
            Viewer = SceneUtils.FindObjectIfSingle<VisibilityZoneViewer>();
        }

        if (Viewer != null) {
            Viewer.OnShowZone.AddListener(OnShowZone);
        }
    }

    
    public void OnShowZone(VisibilityZone zone) {
        if (Settings == null) {
            return;
        }

        var s = Settings.FirstOrDefault(v => v.Zones != null && v.Zones.Any(z => z == zone));
        if (s == null) {
            s = _startSettings;
        }
        RenderSettings.ambientMode = s.AmbientSource;
        RenderSettings.ambientIntensity = s.AmbientIntensity;
        RenderSettings.ambientLight = s.AmbientColor;
        RenderSettings.customReflection = s.CustomReflectionCubemap;
        RenderSettings.defaultReflectionMode = s.ReflectionMode;
        RenderSettings.skybox = s.SkyboxMaterial;
        DynamicGI.UpdateEnvironment();

        if (!MainLight) { return; }
        MainLight.color = s.LightColor;
        MainLight.intensity = s.LightIntensity;
    }
}
}
