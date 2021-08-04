using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;

namespace Nettle {

[CustomEditor(typeof(VisibilityZoneViewSwitch))]
public class VisibilityZoneViewSwitchEditor : Editor {
    private ReorderableList _list;

    private void OnEnable() {

        _list = new ReorderableList(serializedObject, serializedObject.FindProperty("ZoneTriggers"), true, true, true, true);
        _list.onMouseUpCallback += OnMouseEvent;
        _list.elementHeight = EditorGUIUtility.singleLineHeight + 8.0f;
        _list.drawElementCallback = (rect, index, isActive, isFocused) => {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            EditorGUI.LabelField(new Rect(rect.x, rect.y, 30f, EditorGUIUtility.singleLineHeight), index.ToString());
            EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, 100f, EditorGUIUtility.singleLineHeight), "Zone");

            element.FindPropertyRelative("Zone").objectReferenceValue =
                EditorGUI.ObjectField(new Rect(rect.x + 60, rect.y, 250f, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Zone").objectReferenceValue, typeof(VisibilityZone), true);

            var keyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

            var currentKey = keyCodes[element.FindPropertyRelative("Key").enumValueIndex];
            KeyCode newKey = (KeyCode)EditorGUI.EnumPopup(new Rect(rect.x + 330, rect.y, rect.width - 330f, EditorGUIUtility.singleLineHeight), currentKey);
            element.FindPropertyRelative("Key").enumValueIndex = keyCodes.ToList().IndexOf(newKey);


            //EditorGUI.Toggle(new Rect(rect.x + 70, rect.y, 20f, EditorGUIUtility.singleLineHeight),
            //element.FindPropertyRelative("Active").boolValue);

            //EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 100f, EditorGUIUtility.singleLineHeight), "Time:");

            //EditorGUI.PropertyField(new Rect(rect.x + 140, rect.y, rect.xMax - (rect.x + 140), EditorGUIUtility.singleLineHeight),
            //    element.FindPropertyRelative("TimeToShow"), GUIContent.none);

            //EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2f, 100f, EditorGUIUtility.singleLineHeight), "Element:");

            //EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y + EditorGUIUtility.singleLineHeight + 2f, rect.xMax - (rect.x + 60), EditorGUIUtility.singleLineHeight),
            //    element.FindPropertyRelative("ScenarioElement"), GUIContent.none);
        };
        _list.drawHeaderCallback = rect => {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 100f, EditorGUIUtility.singleLineHeight), "ZoneTriggers");

        };
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.Update();
        if (Event.current.type == EventType.DragExited) {
            GameObject rootGameObject;
            if (DragAndDrop.objectReferences.Length > 0 && (rootGameObject = DragAndDrop.objectReferences[0] as GameObject)) {
                Transform root = rootGameObject.transform;
                if (root.GetComponent<VisibilityZone>() == null) {
                    List<VisibilityZoneTrigger> visibilityZoneTriggers = ((VisibilityZoneViewSwitch)serializedObject.targetObject).GetComponent<VisibilityZoneViewSwitch>().ZoneTriggers;
                    for (int i = 0; i < root.childCount; i++) {
                        VisibilityZone visibilityZone = root.GetChild(i).GetComponent<VisibilityZone>();
                        if (visibilityZone != null && !visibilityZoneTriggers.Any(v => v.Zone == visibilityZone)) {
                            visibilityZoneTriggers.Add(new VisibilityZoneTrigger(visibilityZone));
                        }
                    }
                }
            }
        }
        _list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private void OnMouseEvent(ReorderableList rl) {
        Debug.Log("OnMouseEvent");
    }
}
}
