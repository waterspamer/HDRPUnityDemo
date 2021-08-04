using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.SocialPlatforms;
using UnityEngine.Rendering.PostProcessing;


namespace Nettle {

    public class EnvironmentController : MonoBehaviour {

        [System.Serializable]
        public class EnvironmentSettings {
            [Range(0, 1)]
            public float AO = 1;
            [Range(0, 1)]
            public float SM = 1;
            public float LightIntensity = 1;
            public Color LightColor = Color.white;
            public float RealtimeShadowsStrength = 1;
            [Range(0,8)]
            public float SkyboxInternsityMultiplier = 1;
            public bool OverridePostProcessing = false;
            [Range(0,4)]
            public float RealtimeAO = 0.6f;            
            public float PostExposure = 0;
            [Range(-100,100)]
            public float ColorTemperature = 0;

            public EnvironmentSettings() {
            }
           
            public EnvironmentSettings(EnvironmentSettings prototype) {
                AO = prototype.AO;
                SM = prototype.SM;
                LightIntensity = prototype.LightIntensity;
                LightColor = prototype.LightColor;
                RealtimeShadowsStrength = prototype.RealtimeShadowsStrength;
                SkyboxInternsityMultiplier = prototype.SkyboxInternsityMultiplier;
            }
        }

        [System.Serializable]
        public class EnvironmentZoneSettings : EnvironmentSettings {
            public string ZoneName = "";

            public EnvironmentZoneSettings() {
            }

            public EnvironmentZoneSettings(EnvironmentSettings prototype) :base(prototype){
            }

            public EnvironmentZoneSettings(EnvironmentZoneSettings prototype) : base(prototype) {
                ZoneName = prototype.ZoneName;
            }
        }


        [System.Serializable]
        public class EnvironmentSettingsContainer {
            public EnvironmentZoneSettings[] ZoneSettings = new EnvironmentZoneSettings[0];
            public EnvironmentSettings DefaultSettings = new EnvironmentSettings();
        }
        
        public bool SetInUpdate = false;
        public bool SetInEditMode = true;
        public Light MainLight = null;
        public PostProcessVolume _postProcessVolume;
        public VisibilityZoneViewer ZoneViewer;
        public EnvironmentSettingsContainer Settings = new EnvironmentSettingsContainer();
        private EnvironmentSettings _currentSettings;

        private AmbientOcclusion _ambientOcclusionSettings;
        private ColorGrading _colorGradingSettings;

        private string LoadFilePath {
            get {
                string path = GetFilePath(gameObject.scene.name);
                if (File.Exists(path)){
                    return path;
                } else {
                    return GetFilePath("");
                }

            }
        }

        private string SaveFilePath {
            get {
                return GetFilePath(gameObject.scene.name);
            }
        }

        private string GetFilePath(string sceneName) {
            if (!string.IsNullOrEmpty(sceneName)) {
                sceneName = "_" + sceneName;
            }
            return Application.streamingAssetsPath + "/EnvironmentSettings" + sceneName + ".xml";
        }

        private void Reset() {
            Light[] lights = FindObjectsOfType<Light>();
            if (lights.Length > 0) {
                MainLight = lights.Where(x => x.type == LightType.Directional).FirstOrDefault();
            }
            if (MainLight != null) {
                Settings.DefaultSettings.LightColor = MainLight.color;
                Settings.DefaultSettings.LightIntensity = MainLight.intensity;
                Settings.DefaultSettings.RealtimeShadowsStrength = MainLight.shadowStrength;
                Settings.DefaultSettings.SkyboxInternsityMultiplier = RenderSettings.ambientIntensity;
            }
            ZoneViewer = FindObjectOfType<VisibilityZoneViewer>();
        }

        private void Start() {
            if (_postProcessVolume != null)
            {            
                 _postProcessVolume.profile.TryGetSettings(out _ambientOcclusionSettings);
                 _postProcessVolume.profile.TryGetSettings(out _colorGradingSettings);
            }
            LoadXML();
            if (ZoneViewer != null) {
                ZoneViewer.OnShowZone.AddListener(OnShowZone);
                OnShowZone(ZoneViewer.ActiveZone);
            }
            else {
                ResetSettings();
            }
        }

        public void Apply(EnvironmentSettings settings) {
            _currentSettings = settings;
            Apply();
        }

        public void ResetSettings() {
            Apply(Settings.DefaultSettings);
        }

        private void Apply() {
            if (_currentSettings == null) {
                return;
            }
            Shader.SetGlobalFloat("AO_Intensity", _currentSettings.AO);
            Shader.SetGlobalFloat("SM_Intensity", _currentSettings.SM);
            if (MainLight != null) {
                MainLight.intensity = _currentSettings.LightIntensity;
                MainLight.color = _currentSettings.LightColor;
                MainLight.shadowStrength = _currentSettings.RealtimeShadowsStrength;
            }
            RenderSettings.ambientIntensity = _currentSettings.SkyboxInternsityMultiplier;

            if (_currentSettings.OverridePostProcessing && _postProcessVolume != null)
            {
                if (_ambientOcclusionSettings != null)
                {
                    _ambientOcclusionSettings.intensity.value = _currentSettings.RealtimeAO;
                }
                if (_colorGradingSettings != null)
                {
                    _colorGradingSettings.postExposure.value = _currentSettings.PostExposure;
                    _colorGradingSettings.temperature.value = _currentSettings.ColorTemperature;
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private int _oldZoneSettingsCount = 0;
        private void OnValidate() {
            if (Settings.ZoneSettings.Length > _oldZoneSettingsCount) {
                for (int i = _oldZoneSettingsCount; i < Settings.ZoneSettings.Length; i++) {
                    EnvironmentZoneSettings copy =  new EnvironmentZoneSettings(Settings.DefaultSettings);
                    Settings.ZoneSettings[i] = copy;
                }
            }
            _oldZoneSettingsCount = Settings.ZoneSettings.Length;
            if (SetInEditMode) {
                Apply(Settings.DefaultSettings);
            }
            
        }


        private void OnShowZone(VisibilityZone zone) {
            if (Settings.ZoneSettings.Length > 0) {
                EnvironmentZoneSettings settings = null;
                try {
                    settings = Settings.ZoneSettings.Where(x => x.ZoneName == zone.gameObject.name).FirstOrDefault();
                }
                catch { }
                if (settings != null) {
                    Apply(settings);
                }
                else {
                    ResetSettings();
                }
            }
            else {
                ResetSettings();
            }
        }

        private void Update() {
            if (SetInUpdate) {
                Apply();
            }            
        }

        [EasyButtons.Button]
        public void SaveXML() {
            if (!Directory.Exists(Application.streamingAssetsPath)) {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            if (File.Exists(SaveFilePath)) {
                File.Delete(SaveFilePath);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(EnvironmentSettingsContainer));
            Stream output = File.Open(SaveFilePath, FileMode.Create);
            serializer.Serialize(output, Settings);
            output.Close();
        }

        [EasyButtons.Button]
        public void LoadXML() {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Load Environment settings XML");
#endif
            if (!File.Exists(LoadFilePath)) {
                return;
            }
            Stream input = File.Open(LoadFilePath, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(EnvironmentSettingsContainer));
            object deserializedSettings = null;
            try {
                deserializedSettings = serializer.Deserialize(input);
            }
            catch (System.Exception ex) {
                Debug.LogError("Error loading environment settings: " + ex.Message);
            }
            input.Close();
            if (deserializedSettings != null) {
                Settings = deserializedSettings as EnvironmentSettingsContainer;
            }
            _oldZoneSettingsCount = Settings.ZoneSettings.Length;
            if (!Application.isPlaying && SetInEditMode) {
                ResetSettings();
            }
        }
    }
}