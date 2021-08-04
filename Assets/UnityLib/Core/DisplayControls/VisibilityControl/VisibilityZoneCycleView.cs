using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Nettle {

public class VisibilityZoneCycleView : MonoBehaviour
{
    [HideInInspector] public VisibilityZoneEditor[] Zones;
    public VisibilityZoneViewer Viewer;
    public KeyCode SwitchKey = KeyCode.Mouse1;

    private int _curID = 0;

    private void Update()
    {
        if (Input.GetKeyUp(SwitchKey))
        {
            _curID = (_curID + 1)%Zones.Length;
            Viewer.ShowZone(Zones[_curID].Zone.name);
        }
    }

    public void NextZone()
    {
        var index = Zones.Select(s => s.Zone).ToList().IndexOf(Viewer.ActiveZone) + 1;
        if (index >= Zones.Length)
            index = 0;

        Viewer.ShowZone(Zones[index].Zone.name);
    }

    public void PrevZone()
    {
        var index = Zones.Select(s => s.Zone).ToList().IndexOf(Viewer.ActiveZone) - 1;
        if (index < 0)
            index = Zones.Length - 1;

        Viewer.ShowZone(Zones[index].Zone.name);
    }
}

[Serializable]
public class VisibilityZoneEditor
{
    [SerializeField]
    public VisibilityZone Zone;
}

#if UNITY_EDITOR
[CustomEditor(typeof(VisibilityZoneCycleView))]
public class VisibilityZoneCycleViewEditor : Editor
{
    private ReorderableList _list;
    //private VisibilityZoneCycleView _source;

    private void OnEnable()
    {
        //_source = target as VisibilityZoneCycleView;

        _list = new ReorderableList(serializedObject, serializedObject.FindProperty("Zones"), true, true, true, true);
        _list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 30f, EditorGUIUtility.singleLineHeight), index.ToString());

            EditorGUI.PropertyField(new Rect(rect.x + 30, rect.y, rect.xMax - (rect.x + 30), EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Zone"), GUIContent.none);
        };
        _list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 200f, EditorGUIUtility.singleLineHeight), "Zones");
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        _list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
}
