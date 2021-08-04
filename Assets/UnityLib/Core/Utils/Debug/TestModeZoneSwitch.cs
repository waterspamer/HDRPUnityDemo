using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nettle {

public class TestModeZoneSwitch : MonoBehaviour {
    [ConfigField("Active")] public bool Active = false;
    [ConfigField("SwitchTime")] public float SwitchTime = 15.0f;
    [ConfigField("Sortalphabetically")] public bool SortAlphabetically = true;

    private VisibilityZone[] _sceneZones;
    private VisibilityZoneViewer _viewer;
    private int _curZoneId = -1;
    private float _lastSwitchTime = 0.0f;

    private void Awake() {
        _viewer = SceneUtils.FindObjectIfSingle<VisibilityZoneViewer>();

        if (_viewer != null) {
            _sceneZones = GameObject.FindObjectsOfType<VisibilityZone>();
            if (SortAlphabetically) {
                Array.Sort(_sceneZones, (x, y) => String.CompareOrdinal(x.name, y.name));
            }
        }
    }

    private void ShowNextZone() {
        _curZoneId = (int)Mathf.Repeat(_curZoneId + 1, _sceneZones.Length);
        var nextZone = _sceneZones[_curZoneId];

        if (nextZone != null) {
            _viewer.ShowZone(nextZone.name);
        }
    }

    private void Update() {
        if (!Active || _viewer == null || _sceneZones == null || _sceneZones.Length < 1) { return; }
        Debug.Log(_lastSwitchTime);
        if (Time.realtimeSinceStartup - _lastSwitchTime > SwitchTime) {
            ShowNextZone();
            _lastSwitchTime = Time.realtimeSinceStartup;
        }
    }
}
}
