using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Nettle {
    [CustomEditor(typeof(OverlayCameraMixer))]
    public class OverlayCameraMixerEditor : Editor {
        private ReorderableList _list;

        private void OnEnable() {

            _list = new ReorderableList(serializedObject, serializedObject.FindProperty("_cameras"), true, true, true, true);
            _list.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 16.0f;
            _list.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = _list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 30f, EditorGUIUtility.singleLineHeight), index.ToString());
                element.objectReferenceValue =
                    EditorGUI.ObjectField(new Rect(rect.x + 20, rect.y, 190f, EditorGUIUtility.singleLineHeight), element.objectReferenceValue, typeof(OverlayCameraEffect), true);
                if (element.objectReferenceValue != null) {
                    SerializedObject elementObj = new SerializedObject(element.objectReferenceValue);
                    SerializedProperty blendValue = elementObj.FindProperty("_blendValue");
                    if (blendValue != null) {
                        elementObj.Update();
                        Rect sliderRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 8, 210f, EditorGUIUtility.singleLineHeight);
                        blendValue.floatValue = EditorGUI.Slider(sliderRect, blendValue.floatValue, 0, 1);
                        elementObj.ApplyModifiedProperties();
                    }
                }
            };
            _list.drawHeaderCallback = rect => {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 130f, EditorGUIUtility.singleLineHeight), "Overlay Cameras");
            };
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            serializedObject.Update();
            _list.DoLayoutList();            
            serializedObject.ApplyModifiedProperties();
        }
    }
}