using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ObjectSelector : EditorWindow
{
    private static ObjectSelector _window;

    private float _marginHeight = 6;

    [MenuItem("Nettle/Object selector")]
    private static void ShowWindow()
    {
        _window = CreateInstance<ObjectSelector>();
        _window.titleContent.text = "Object selector";
        _window.Show();
    }


    private void OnGUI()
    {
        GUILayout.Label("A collection of functions to select objects based on various rules");
        EditorGUILayout.Separator();
        GUILayout.Label("Select direct children by world Y position", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _marginHeight = EditorGUILayout.FloatField("Height", _marginHeight);
        if (GUILayout.Button("Select above"))
        {
            SelectByHeight(true);
        }
        if (GUILayout.Button("Select below"))
        {
            SelectByHeight(false);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SelectByHeight(bool above)
    {
        List<GameObject> selectedObjects = new List<GameObject>();
        foreach (GameObject obj in Selection.gameObjects)
        {
            foreach (Transform child in obj.transform)
            {
                if ((above && child.position.y > _marginHeight) || (!above && child.position.y < _marginHeight))
                {
                    selectedObjects.Add(child.gameObject);
                }
            }
        }
        Selection.objects = selectedObjects.ToArray();
    }


}
