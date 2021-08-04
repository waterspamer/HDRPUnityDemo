using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Nettle {

public class TimelapseButton : MonoBehaviour {

    public DateTimeUnityEditor TimeToSet;
    public Slider Slider;

    public void SetTime() {
        if (Slider != null) {
            Slider.value = TimeToSet.ToDateTime().Ticks;
        }
    }
}
}
