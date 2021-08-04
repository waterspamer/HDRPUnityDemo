using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Nettle {

[CustomEditor(typeof(Timelapse))]
public class TimelapseControllerEditor : Editor {

    public override void OnInspectorGUI() {

        var timelapse = (Timelapse)target;

        base.OnInspectorGUI();

        GUILayout.Label("Current date: " + timelapse.CurrenTime);

        if (GUILayout.Button("Show timelapse window")) {
            TimelapseControllerWindow.Init(timelapse);
        }
    }
}
}
