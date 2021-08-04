using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nettle {

public class SliderEx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [Serializable]
    public class UnityEventBool : UnityEvent<bool> { }

    public UnityEvent MinValueReached = new UnityEvent();
    public UnityEvent MaxValueReached = new UnityEvent();

    public UnityEvent MinValueZoneExited = new UnityEvent();
    public UnityEvent MaxValueZoneExited = new UnityEvent();

    public UnityEvent PointerDown = new UnityEvent();
    public UnityEvent PointerUp = new UnityEvent();

    private Slider _target;

    private float _prevValue = 0.0f;

    private bool _atMinZone = false;
    private bool _atMaxZone = false;
    private bool _pointerPressed = false;

    private void Awake() {
        if (_target == null) {
            _target = GetComponent<Slider>();
            _prevValue = _target.value;
        }
    }

    private void Update() {
        if (Math.Abs(_prevValue - _target.value) < 0.001f) { return; }

        var curValue = _target.value;

        if (!_atMinZone && Mathf.Abs(curValue - _target.minValue) < 0.001f) {
            MinValueReached.Invoke();
            _atMinZone = true;
        }

        if (!_atMaxZone && Mathf.Abs(curValue - _target.maxValue) < 0.001f) {
            MaxValueReached.Invoke();
            _atMaxZone = true;
        }

        if(_atMinZone && Mathf.Abs(curValue - _target.minValue) > 0.001f) {
            MinValueZoneExited.Invoke();
            _atMinZone = false;
        }

        if (_atMaxZone && Mathf.Abs(curValue - _target.maxValue) > 0.001f) {
            MaxValueZoneExited.Invoke();
            _atMaxZone = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        PointerDown.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData) {
        PointerUp.Invoke();
    }
}
}
