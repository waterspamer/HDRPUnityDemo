using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

public class LinkedLineRenderer : MonoBehaviour {

    [SerializeField]
    private LineRenderer _renderer;

    public List<Transform> Targets = new List<Transform>(2);

    private void OnValidate() {
        EditroInit();
    }

    private void Reset() {
        EditroInit();
    }

    private void EditroInit() {
        if (!_renderer) {
            _renderer = GetComponent<LineRenderer>();
        }
        Update();
    }

    [Button]
    public void Update() {
        if (_renderer != null) {
            for (int i = 0; i < Targets.Count; i++) {
                _renderer.SetPosition(i, Targets[i].position);
            }
        }
    }
}
