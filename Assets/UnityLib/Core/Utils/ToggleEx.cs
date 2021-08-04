using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nettle {

[RequireComponent(typeof(Toggle))]
public class ToggleEx : MonoBehaviour , IPointerClickHandler {

    public UnityEvent OnToggleSetOnEvent = new UnityEvent();
    public UnityEvent OnToggleSetOffEvent = new UnityEvent();
    public UnityEvent OnPointerClickEvent = new UnityEvent();

    private Toggle _toggle;
    //private bool _prevState;

    private void Awake () {
	    _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(Toggle);
        //if (_toggle != null) {
        //    _prevState = _toggle.isOn;
        //}
	}

    /*private void Update () {
        if (_toggle != null) {
            if (_prevState != _toggle.isOn) {
                if (_toggle.isOn) {
                    OnToggleSetOnEvent.Invoke();
                } else {
                    OnToggleSetOffEvent.Invoke();
                }
                _prevState = _toggle.isOn;
            }
            //if (_prevState != _toggle.isOn) {
            //    if (_toggle.isOn) {
            //        OnToggleSetOnEvent.Invoke();
            //    }
            //    _prevState = _toggle.isOn;
            //} else {
            //    if (!_toggle.isOn) {
            //        OnToggleSetOnEvent.Invoke();
            //    }
            //}
        }
	}*/

    private void Toggle(bool on) {
        if (on) {
            OnToggleSetOnEvent.Invoke();
        } else {
            OnToggleSetOffEvent.Invoke();
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnPointerClickEvent.Invoke();
    }
}
}
