using UnityEngine;
using System;
using UnityEngine.Events;

namespace Nettle {

public class EventByZones : MonoBehaviour {

    public VisibilityZoneViewer ZoneViewer;
    public VisibilityZone[] Zones;
    public UnityEvent ZoneShowed;
    public UnityEvent ZoneHided;

    private bool _isShowing = false;
    private bool _zoneInArray = false;
    private VisibilityZone _currentZone = null;

    private void Reset() {
        if (ZoneViewer == null) {
            ZoneViewer = SceneUtils.FindObjectIfSingle<VisibilityZoneViewer>();
        }
    }

    private void OnEnable() {
        OnZoneActiveEventHandler();
    }

    private void Awake() {
        Reset();

        if (ZoneViewer) {
            ZoneViewer.ActiveZoneChanged += OnZoneActiveEventHandler;
        }
    }

    private void OnDestroy() {

        if (ZoneViewer) {
            ZoneViewer.ActiveZoneChanged -= OnZoneActiveEventHandler;
        }
    }

    private void OnZoneActiveEventHandler() {
        if(ZoneViewer == null) { return; }

        _currentZone = ZoneViewer.ActiveZone;
        if (_currentZone == null) return;

        _zoneInArray = Array.Exists(Zones, z => z.name == _currentZone.name);

        if (_isShowing && !_zoneInArray) {
            _isShowing = false;
            if (ZoneHided != null) {
                ZoneHided.Invoke();
            }
        } else if (!_isShowing && _zoneInArray) {
            _isShowing = true;
            if (ZoneShowed != null) {
                ZoneShowed.Invoke();
            }
        }
    }
}
}
