using UnityEngine;
using UnityEngine.Events;

public class QualitySettingsManager : MonoBehaviour
{
    public UnityEvent SetDefaultValuesEvent;

    private float _defaultShadowDistance;

    private void Awake() {
        SetDefaultValuesEvent.Invoke();
        _defaultShadowDistance = QualitySettings.shadowDistance;
    }


    public void SetShadowDistance(float distance) {
        QualitySettings.shadowDistance = distance;
    }

    public void ResetShadowDistance() {
        QualitySettings.shadowDistance = _defaultShadowDistance;
    }
}
