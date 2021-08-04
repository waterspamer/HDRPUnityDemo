using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nettle {

[CustomPropertyDrawer(typeof(LocalizedAudio))]
public class LocalizedAudioDrawer : PropertyDrawer {
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        //EditorGUI.EnumPopup(new Rect(pos.x, pos.y, pos.width / 2, pos.height), (Language)prop.FindPropertyRelative("Language").enumValueIndex);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        prop.FindPropertyRelative("Volume").floatValue = EditorGUI.Slider(new Rect(pos.x + pos.width * 0.25f, pos.y, pos.width * 0.25f, pos.height), prop.FindPropertyRelative("Volume").floatValue,0,1);
        EditorGUI.PropertyField(new Rect(pos.x + pos.width * 0.5f, pos.y, pos.width * 0.25f, pos.height), prop.FindPropertyRelative("Language"), new GUIContent(""));        
        EditorGUI.PropertyField(new Rect(pos.x + pos.width * 0.75f, pos.y, pos.width * 0.25f, pos.height), prop.FindPropertyRelative("Audio"),new GUIContent(""));
        EditorGUI.indentLevel = indent;
    }
}
}
