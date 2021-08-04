using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererConnector : MonoBehaviour {
    public Transform[] Transforms;
    public LineRenderer LineRenderer;
    public bool AutoDisableRenderer = true;

	// Update is called once per frame
	void Update () {
        Reconnect();
    }

    private void OnEnable() {
        if (AutoDisableRenderer) {
            LineRenderer.enabled = true;
            Reconnect();
        }
    }

    private void OnDisable() {
        if (AutoDisableRenderer) {
            LineRenderer.enabled = false;
        }
    }

    private void Reconnect() {
        LineRenderer.positionCount = Transforms.Length;
        for (int i = 0; i < Transforms.Length; i++) {
            LineRenderer.SetPosition(i, Transforms[i].position);
        }
    }
}
