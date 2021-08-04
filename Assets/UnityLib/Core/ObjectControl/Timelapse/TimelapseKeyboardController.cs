using System;
using UnityEngine;
using System.Collections;

namespace Nettle {

[Serializable]
public class DateTimeKeyBinding {
    public KeyCode Key;
    public DateTimeUnityEditor Date;
}

public class TimelapseKeyboardController : MonoBehaviour {

    public Timelapse TimelapseComponent;
    public DateTimeKeyBinding[] DateKeys;

    public KeyCode PlusKey = KeyCode.RightArrow;
    public KeyCode MinusKey = KeyCode.LeftArrow;

    public int DaysPerSecond = 30;

    private void Reset() {
        if (!TimelapseComponent) {
            TimelapseComponent = GetComponent<Timelapse>();
        }
    }

    private void OffsetTime(double days) {
        if (!TimelapseComponent) { return; }

        var curTime = TimelapseComponent.CurrenTime;

        TimelapseComponent.SetTime(curTime.AddDays(days));
    }

    private void Update() {
        if (!TimelapseComponent) { return; }

        if (DateKeys != null && DateKeys.Length > 0) {
            foreach (var dateKey in DateKeys) {
                if (Input.GetKeyUp(dateKey.Key)) {
                    TimelapseComponent.SetTime(dateKey.Date.ToDateTime());
                    break;
                }
            }
        }

        float offset = 0.0f;
        if (Input.GetKey(PlusKey)) {
            offset = DaysPerSecond * Time.smoothDeltaTime;
        } else if(Input.GetKey(MinusKey)) {
            offset = -DaysPerSecond * Time.smoothDeltaTime;
        }
        if (Mathf.Abs(offset) > 0.001f) {
            OffsetTime((double) offset);
        }
    }
}
}
