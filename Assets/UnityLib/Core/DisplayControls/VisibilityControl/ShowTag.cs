using UnityEngine;
using System.Collections;

namespace Nettle {

public class ShowTag : MonoBehaviour {

    public VisibilityManager Manager;
    public float ShowTimeOffset = 0.0f;
    public string Tag;

    private float _timeSinceShow = 0.0f;
    private bool _active = false;
    private bool _showed = false;

	public void Show() {
        _timeSinceShow = 0.0f;
        _active = true;
        Debug.LogError("Show");
    }

    public void Reset() {
        _timeSinceShow = 0.0f;
        _active = false;
        _showed = false;
    }

    public void Update() {
        if (_active && !_showed) {
            _timeSinceShow += Time.deltaTime;
        }

        if (!_showed && _timeSinceShow > ShowTimeOffset) {
            Manager.BeginSwitch(Tag);
            _showed = true;
        }
    }
}
}
