using UnityEngine;
using UnityEngine.UI;

public class SliderValueAdapter : MonoBehaviour {

    public Slider Slider;

    void Reset() {
        EditorInit();
    }

    void OnValidate() {
        EditorInit();
    }

    void EditorInit() {
        if (!Slider) {
            Slider = GetComponent<Slider>();
        }
    }

    public int Value {
        set { Slider.value = value; }
    }

    public int Length {
        set { Slider.maxValue = value - 1; }
    }
}
