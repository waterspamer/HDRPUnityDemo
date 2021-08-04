using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nettle {
    public class EnvironmentControllerGUI : MonoBehaviour {
        public Slider AO;
        public Slider SM;
        public Slider LightIntensity;
        public Slider RealtimeShadowStrength;
        public Slider SkyboxIntensity;
        public Slider RealtimeAO;
        public Slider PostExposure;
        public Slider Temperature;
        public InputField AOValue;
        public InputField SMValue;
        public InputField LightIntensityValue;
        public InputField RealtimeShadowStrengthValue;
        public InputField SkyboxIntensityValue;
        public Toggle OverridePostProcessing;
        public InputField RealtimeAOValue;
        public InputField PostExposureValue;
        public InputField TemperatureValue;

        private EnvironmentController _enviromentController;
        private bool _dontInvalidate = false;

        private void OnEnable() {
            _enviromentController = FindObjectOfType<EnvironmentController>();
            if (_enviromentController == null) {
                Debug.LogError("No environment controller found");
                gameObject.SetActive(false);
                return;
            }
            ResetSliders();
        }

        private void ResetSliders() {
            _dontInvalidate = true;
            AO.value = _enviromentController.Settings.DefaultSettings.AO;
            SM.value = _enviromentController.Settings.DefaultSettings.SM;
            LightIntensity.value = _enviromentController.Settings.DefaultSettings.LightIntensity;
            RealtimeShadowStrength.value = _enviromentController.Settings.DefaultSettings.RealtimeShadowsStrength;
            SkyboxIntensity.value = _enviromentController.Settings.DefaultSettings.SkyboxInternsityMultiplier;
            OverridePostProcessing.isOn = _enviromentController.Settings.DefaultSettings.OverridePostProcessing;
            RealtimeAO.value = _enviromentController.Settings.DefaultSettings.RealtimeAO;
            Temperature.value = _enviromentController.Settings.DefaultSettings.ColorTemperature;
            PostExposure.value = _enviromentController.Settings.DefaultSettings.PostExposure;
            _dontInvalidate = false;
            Invalidate();
        }

        public void Invalidate() {
            if (_dontInvalidate) {
                return;
            }
            _enviromentController.Settings.DefaultSettings.AO = AO.value;
            _enviromentController.Settings.DefaultSettings.SM = SM.value;
            _enviromentController.Settings.DefaultSettings.LightIntensity = LightIntensity.value;
            _enviromentController.Settings.DefaultSettings.RealtimeShadowsStrength = RealtimeShadowStrength.value;
            _enviromentController.Settings.DefaultSettings.SkyboxInternsityMultiplier = SkyboxIntensity.value;
            _enviromentController.Settings.DefaultSettings.OverridePostProcessing = OverridePostProcessing.isOn;            
            _enviromentController.Settings.DefaultSettings.RealtimeAO = RealtimeAO.value;
            _enviromentController.Settings.DefaultSettings.PostExposure = PostExposure.value;
            _enviromentController.Settings.DefaultSettings.ColorTemperature = Temperature.value;
            AOValue.text = RoundValue(AO.value);
            SMValue.text = RoundValue(SM.value);
            LightIntensityValue.text = RoundValue(LightIntensity.value);
            RealtimeShadowStrengthValue.text = RoundValue(RealtimeShadowStrength.value);
            SkyboxIntensityValue.text = RoundValue(SkyboxIntensity.value);
            RealtimeAOValue.text = RoundValue(RealtimeAO.value);
            PostExposureValue.text = RoundValue(PostExposure.value);
            TemperatureValue.text = RoundValue(Temperature.value);
            _enviromentController.Apply(_enviromentController.Settings.DefaultSettings);
        }

        public void ReadValues() {
            _enviromentController.Settings.DefaultSettings.AO = float.Parse(AOValue.text);
            _enviromentController.Settings.DefaultSettings.SM = float.Parse(SMValue.text);
            _enviromentController.Settings.DefaultSettings.LightIntensity = float.Parse(LightIntensityValue.text);
            _enviromentController.Settings.DefaultSettings.RealtimeShadowsStrength = float.Parse(RealtimeShadowStrengthValue.text);
            _enviromentController.Settings.DefaultSettings.SkyboxInternsityMultiplier = float.Parse(SkyboxIntensityValue.text);
            _enviromentController.Settings.DefaultSettings.OverridePostProcessing = OverridePostProcessing.isOn;
            _enviromentController.Settings.DefaultSettings.RealtimeAO = float.Parse(RealtimeAOValue.text);
            _enviromentController.Settings.DefaultSettings.PostExposure = float.Parse(PostExposureValue.text);
            _enviromentController.Settings.DefaultSettings.ColorTemperature = float.Parse(TemperatureValue.text);
            ResetSliders();
        }

        private string RoundValue(float value) {
            return (Mathf.Round(value * 1000) / 1000).ToString();
        }

        public void SaveXML() {
            _enviromentController.SaveXML();
        }

        public void LoadXML() {
            _enviromentController.LoadXML();
            _enviromentController.Apply(_enviromentController.Settings.DefaultSettings);
            ResetSliders();
        }
    }
}
