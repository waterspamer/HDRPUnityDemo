using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Nettle {

[CustomEditor(typeof(TextureAnimation))]
public class TextureAnimationEditor : Editor {
    private ReorderableList _list;
    //private Config _source;

    private void OnEnable() {
        //_source = target as Config;

        _list = new ReorderableList(serializedObject, serializedObject.FindProperty("TexAnimations"), true, true, true, true);
        _list.drawElementCallback = (rect, index, isActive, isFocused) => {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 30f, EditorGUIUtility.singleLineHeight), index.ToString());

            EditorGUI.LabelField(new Rect(rect.x + 20f, rect.y, 70f, EditorGUIUtility.singleLineHeight), "Mat index:");
            EditorGUI.PropertyField(new Rect(rect.x + 85f, rect.y, 30f, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("MaterialIndex"), GUIContent.none);

            EditorGUI.LabelField(new Rect(rect.x + 130f, rect.y, 110f, EditorGUIUtility.singleLineHeight), "Animation rate: x:");
            var animRateX =
            EditorGUI.FloatField(new Rect(rect.x + 240f, rect.y, 40f, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("UvAnimationRate").vector2Value.x);

            EditorGUI.LabelField(new Rect(rect.x + 290f, rect.y, 20f, EditorGUIUtility.singleLineHeight), "y:");
            var animRateY =
            EditorGUI.FloatField(new Rect(rect.x + 310, rect.y, 40f, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("UvAnimationRate").vector2Value.y);

            element.FindPropertyRelative("UvAnimationRate").vector2Value = new Vector2(animRateX, animRateY);

            EditorGUI.LabelField(new Rect(rect.x + 360f, rect.y, 90f, EditorGUIUtility.singleLineHeight), "Texture name:");
            element.FindPropertyRelative("TextureName").stringValue = 
            EditorGUI.TextField(new Rect(rect.x + 460f, rect.y, rect.xMax - (rect.x + 460f), EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("TextureName").stringValue);
        };
        _list.drawHeaderCallback = rect => {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 200f, EditorGUIUtility.singleLineHeight), "Textures animations");
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
