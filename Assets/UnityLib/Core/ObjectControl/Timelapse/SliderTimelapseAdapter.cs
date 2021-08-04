using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

namespace Nettle {

[Serializable]
public class SliderTimelapseAdapterFloatEvent : UnityEvent<float> { }

[Serializable]
public class SliderTimelapseAdapterLongEvent : UnityEvent<long> { }

public class SliderTimelapseAdapter : MonoBehaviour {

    public SliderTimelapseAdapterFloatEvent OnChangeForSlider;
    public SliderTimelapseAdapterLongEvent OnChangeForTimelapse;

    public void SetValueToTimelapse(float value) {
        if (OnChangeForTimelapse != null)
            OnChangeForTimelapse.Invoke((long)value);
    }

    public void SetValueToSlider(long value) {
        if (OnChangeForSlider != null)
            OnChangeForSlider.Invoke(value);
    }
}
}
