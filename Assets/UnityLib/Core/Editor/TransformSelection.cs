using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TransformSelection : EditorWindow
{
    private enum TransformSpace
    {
        Self, World, Parent
    }

    private static TransformSelection _window;
    [MenuItem("Nettle/Transform Selection")]
    private static void ShowWindow()
    {
        _window = CreateInstance<TransformSelection>();
        _window.titleContent.text = "Transform Selection";
        _window.Show();
    }

    private Vector3 _translation;
    private Vector3 _rotation;
    private Vector3 _scale = new Vector3(1,1,1);
    private TransformSpace _space = TransformSpace.Self;

    private void OnGUI()
    {
        _space = (TransformSpace)EditorGUILayout.EnumPopup("Transformation space", _space);
        _translation = EditorGUILayout.Vector3Field("Translation", _translation);
        _rotation = EditorGUILayout.Vector3Field("Rotation", _rotation);
        _scale = EditorGUILayout.Vector3Field("Scale", _scale);
        if (GUILayout.Button("Apply Transformation"))
        {
            DoTransform();
        }
    }

    private void DoTransform()
    {
        Vector3 translation = _translation;
        Vector3 rotation = _rotation;
        Vector3 scale = _scale;

        foreach (Transform t in Selection.transforms)
        {
            Undo.RecordObject(t, "Transform selection");
            // Vector3 rotationPoint = Vector3.zero;
            Transform spaceTransform = null;
            if (_space == TransformSpace.Parent && t.parent != null)
            {
                spaceTransform = t.parent;
             //   rotationPoint = t.parent.position;
            }
            else if (_space == TransformSpace.Self)
            {
                spaceTransform = t;
               // rotationPoint = t.position;
            }
            if (spaceTransform != null)
            {
                translation = spaceTransform.TransformVector(translation);
                rotation = (Quaternion.AngleAxis(_rotation.x, spaceTransform.right) 
                * Quaternion.AngleAxis(_rotation.y, spaceTransform.up)
                * Quaternion.AngleAxis(_rotation.z, spaceTransform.forward)).eulerAngles;                
            }
            t.Translate(translation, Space.World);
            t.Rotate(rotation,Space.World);
            Vector3 newScale = Vector3.Scale(t.localScale, scale);
            t.localScale = newScale;
        }
    }
}
