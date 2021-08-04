using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nettle {

public class SetTextByToggleState : MonoBehaviour {
    public string OnText;
    public string OffText;
    [SerializeField]
    private Toggle _toggle;
    [SerializeField]
    private Text _targetText;

    private void Awake() {
        if (_toggle == null) {
            _toggle = GetComponent<Toggle>();
        }
        if (_toggle != null) {
            if (_targetText == null) {
                _targetText = _toggle.GetComponent<Text>();
            }
            _toggle.onValueChanged.AddListener(SetTextByState);
            SetTextByState(_toggle.isOn);
        }
    }

    public void SetTextByState(bool state) {
        if (_targetText == null) { return; }

        _targetText.text= state ? OnText : OffText;
    }
}
}
