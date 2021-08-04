using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nettle {


[Serializable]
public class TimeChanged : UnityEvent<DateTime> {
}

public class TimelapseSlider : MonoBehaviour {

    public Slider TlSlider;

    public TimeChanged TimeChangedEvent;

	void Start () {
	    if (!TlSlider) {
	        TlSlider = GetComponent<Slider>();
	    }
	}

    void Reset() {
        if (!TlSlider) {
            TlSlider = GetComponent<Slider>();
        }
    }

    public void OnTimeChanged(float time) {
        if (TimeChangedEvent != null) {
            TimeChangedEvent.Invoke(new DateTime((long)time));
        }
    }
}
}
