using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActiveByToggle : MonoBehaviour {

    public string FindToggleByName;
    public Toggle Toggle;
    public bool Inverse;

    public void Awake() {
        if (!Toggle) {
            Toggle = FindObjectsOfType<Toggle>().First(v => v.name == FindToggleByName);
        }

        OnValueChanged(Toggle.isOn);
        Toggle.onValueChanged.AddListener(OnValueChanged);


    }

    private void OnValueChanged(bool state) {
        gameObject.SetActive(state ^ Inverse);
    }

    public void OnDestroy() {
        Toggle.onValueChanged.RemoveListener(OnValueChanged);
    }

}
