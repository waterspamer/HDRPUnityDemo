using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nettle {

[CustomEditor(typeof(VisibilityControl))]
[CanEditMultipleObjects]
public class VisibilityControlEditor : Editor {
    private VisibilityControl control;

    private void OnEnable() {
        control = target as VisibilityControl;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        VisibilityFactor[] factors = control.Factors;
        if (factors.Length > 0) {
            EditorGUILayout.LabelField("Visibility factors:");
            foreach (VisibilityFactor factor in factors) {
                bool oldState = factor.State;
                bool newState = EditorGUILayout.Toggle(factor.Name, oldState);
                if (newState != oldState) {
                    control.SetVisibilityFactor(factor.Name, newState);
                }
            }
        }
    }
}
}
