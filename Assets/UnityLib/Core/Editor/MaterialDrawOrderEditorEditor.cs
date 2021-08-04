using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Nettle {

[CustomEditor(typeof(MaterialDrawOrderEditor))]
public class MaterialDrawOrderEditorEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
       // GUILayout.Label("ololo");
        var editor = (MaterialDrawOrderEditor) target;

        if (editor.material != null) {
            editor.material.renderQueue = EditorGUILayout.IntField("Render queue", editor.material.renderQueue);
        } else {
            GUILayout.Label("Material is not assigned!");
        }
        
    }
}
}
