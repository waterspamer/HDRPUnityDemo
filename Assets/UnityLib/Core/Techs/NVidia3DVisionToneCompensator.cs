using Nettle;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NVidia3DVisionToneCompensator : MonoBehaviour {

    [ConfigField]
    public bool CompensationEnabled = false;
    private PostProcessVolume _postProcessVolume;
    [ConfigField]
    public float PositiveCompensationPower = 57;
    [ConfigField]
    public float NegativeCompensationPower = 30;

    [Header("Debug")]
    [ConfigField]
    public bool DebugUIEnabled = false;
    public Text AngleText;
    public Text NegativePowerText;
    public Text PositivePowerText;

    private ColorGrading _colorGrading;
    private float _defaultTemperature;
    private bool _postProcessingValid = false;
    private bool _stereoEyesValid = false;

    public PostProcessVolume PostProcessVolume{
        get
        {
            return _postProcessVolume;
        }
        set
        {
            if (value == null)
            {
                return;
            }
            if (_colorGrading != null && _colorGrading.temperature!=null)
            {
                //Restore previous volume to default color temperature
                _colorGrading.temperature.value = _defaultTemperature;
            }            
            ColorGrading newColorGrading = value.profile.GetSetting<ColorGrading>();
            if (newColorGrading != null)
            {
                _colorGrading = newColorGrading;
                _postProcessVolume = value;
                _colorGrading.temperature.overrideState = true;
                _defaultTemperature =_colorGrading.temperature.value;
                _postProcessingValid = true;
            }
            else
            {
                Debug.LogWarning("[ToneCompensator:] The selected volume does not have color grading effect");
            }
        }
    }

    void Start() {
        TryFindVolume();
        if (AngleText) {
            AngleText.enabled = DebugUIEnabled;
        }
        if (NegativePowerText) {
            NegativePowerText.enabled = DebugUIEnabled;
        }
        if (PositivePowerText) {
            PositivePowerText.enabled = DebugUIEnabled;
        }
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            _postProcessingValid = false;
            TryFindVolume();
        }
    }


    private void TryFindVolume()
    {
        List<PostProcessVolume> _volumes =  SceneUtils.FindObjectsOfType<PostProcessVolume>(true);
        PostProcessVolume globalVolume = _volumes.Find(x => x.isGlobal);
        if (globalVolume != null)
        {
            PostProcessVolume = globalVolume;
        }
    }

    // Update is called once per frame
    void Update() {
        float angle = 0;
        if (CompensationEnabled&& StereoEyes.Instance.CameraMode != EyesCameraMode.Mono  && StereoEyes.Instance!=null && _postProcessingValid) {
            Vector3 screenUp = StereoEyes.Instance.transform.parent.forward;
            Vector3 screenForward = -StereoEyes.Instance.transform.parent.up;
            Vector3 screenSpaceEyesUP = Vector3.ProjectOnPlane(StereoEyes.Instance.transform.up, screenForward);
            angle = Vector3.SignedAngle(screenSpaceEyesUP, screenUp, screenForward);

            float temperature = -Mathf.Cos(((angle + 45) * 2) * Mathf.Deg2Rad);
            temperature = temperature > 0 ? temperature * PositiveCompensationPower : temperature * NegativeCompensationPower;
            _colorGrading.temperature.value = _defaultTemperature + temperature;
        }else
        {
            if (_postProcessingValid)
            {
                _colorGrading.temperature.value = _defaultTemperature;
            }
        }

        if (DebugUIEnabled) {
            AngleText.text = angle.ToString();

            if (Input.GetKeyUp(KeyCode.Space)) {
                CompensationEnabled = !CompensationEnabled;
            }


            if (Input.GetKeyUp(KeyCode.Keypad1)) {
                NegativeCompensationPower--;
            }
            if (Input.GetKeyUp(KeyCode.Keypad4)) {
                NegativeCompensationPower++;
            }
            if (Input.GetKeyUp(KeyCode.Keypad2)) {
                PositiveCompensationPower--;
            }
            if (Input.GetKeyUp(KeyCode.Keypad5)) {
                PositiveCompensationPower++;
            }

            NegativePowerText.text = NegativeCompensationPower.ToString();
            PositivePowerText.text = PositiveCompensationPower.ToString();

        }


    }
}
