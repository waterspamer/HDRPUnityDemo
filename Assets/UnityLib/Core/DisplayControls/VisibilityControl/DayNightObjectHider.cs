using UnityEngine;
using System.Collections;

namespace Nettle {

public class DayNightObjectHider : MonoBehaviour {
    public DayNightController Controller;

    private void OnEnable() {
        if (Controller != null) {
            OnTimeOfDayChanged(Controller.TimeOfDay);
        }
    }


    private void OnDestroy() {
        if (Controller != null) {
            Controller.OnTimeOfDayChanged.RemoveListener(OnTimeOfDayChanged);
        }
    }

    private void Awake() {
        if (Controller != null) {
            Controller.OnTimeOfDayChanged.AddListener(OnTimeOfDayChanged);
        } else {
            Debug.Log("Controller is null");
        }
    }

    private void Start() {
        if (Controller != null) {
            OnTimeOfDayChanged(Controller.TimeOfDay);
            
        } else {
            Debug.Log("Controller is null");
        }
    }


    private void Reset() {
        if (Controller == null) {
            Controller = SceneUtils.FindObjectIfSingle<DayNightController>();
        }
    }

    void OnTimeOfDayChanged(TimeOfDay time) {
        gameObject.SetActive(time == TimeOfDay.Night);
        //Debug.LogFormat("Time: {0}, state: {1}", time, gameObject.activeSelf);
    }
}
}
