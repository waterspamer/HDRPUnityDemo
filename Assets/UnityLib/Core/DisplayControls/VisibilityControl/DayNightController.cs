using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

public enum TimeOfDay { Day, Night }

[Serializable]
public class OnTimeOfDayChangedEvent : UnityEvent<TimeOfDay> { }

public class DayNightController : MonoBehaviour {
    public VisibilityZoneViewer Viewer;
    public TimeOfDay DefaultState = TimeOfDay.Day;
    public TimeOfDay TimeOfDay {
        get { return _timeOfDay; }
        set {
            _timeOfDay = value;
            //Debug.Log("Change time to " + _timeOfDay);
            if (OnTimeOfDayChanged != null)
                OnTimeOfDayChanged.Invoke(value);
        }
    }
    public OnTimeOfDayChangedEvent OnTimeOfDayChanged = new OnTimeOfDayChangedEvent();

    private TimeOfDay _timeOfDay = TimeOfDay.Day;

    private void Reset() {
        if (Viewer == null) {
            Viewer = SceneUtils.FindObjectIfSingle<VisibilityZoneViewer>();
        }
    }

    private void Awake() {
        if (Viewer != null) {
            Viewer.OnShowZone.AddListener(OnShowZone);
        }
    }

    public void Start() {
        TimeOfDay = TimeOfDay;
    }

    public void OnShowZone(VisibilityZone zone) {
        var zoneSettings = zone.gameObject.GetComponent<DayNightZone>();

        if (zoneSettings != null) {
            if (TimeOfDay == TimeOfDay.Day && !zoneSettings.DayZone) {
                TimeOfDay = TimeOfDay.Night;
            } else if (TimeOfDay == TimeOfDay.Night && !zoneSettings.NightZone) {
                TimeOfDay = TimeOfDay.Day;
            }
            return;
        }

        //Set to default state if no settings found
        if (TimeOfDay != DefaultState) {
            TimeOfDay = DefaultState;
        }
    }

    public void DayNightToggle() {
        TimeOfDay = TimeOfDay == TimeOfDay.Day ? TimeOfDay.Night : TimeOfDay.Day;
    }

    public void SetTimeOfDay(string time) {
        var newTime = time.ToLower();
        if (newTime == "day") {
            SetTimeOfDay(TimeOfDay.Day);
        } else if (newTime == "night") {
            SetTimeOfDay(TimeOfDay.Night);
        } else {
            Debug.LogError("Time not found: " + time);
        }
    }

    public void SetTimeOfDay(TimeOfDay time) {
        if (TimeOfDay != time) {
            TimeOfDay = time;
        }
    }
}
}
