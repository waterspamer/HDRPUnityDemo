using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Nettle {

public class InstantiateGameObjects : EditorWindow
{
    private GameObject _asset;

    private static InstantiateGameObjects _window;
    private bool _paramTransform = true;

    private Vector3 _rotation;
    private Vector3 _scale = new Vector3(1,1,1);
    private string _name = String.Empty;
    private string _predicate = String.Empty;

    private GameObject _currentAsset;

    private GUIStyle _foldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };

    [MenuItem("Window/Create Instances")]
    static void Init()
    {
        _window = (InstantiateGameObjects) EditorWindow.GetWindow(typeof (InstantiateGameObjects), false, "Create instances");
        _window.Show();
    }

    void OnGUI()
    {
        _asset = (GameObject)EditorGUILayout.ObjectField("Asset:", _asset, typeof (GameObject), true);

        EditorGUILayout.Space();
        _paramTransform = EditorGUILayout.Foldout(_paramTransform, "Transform", _foldoutStyle ?? EditorStyles.foldout);
        if (_paramTransform)
        {
            EditorGUILayout.BeginVertical();

            _rotation = EditorGUILayout.Vector3Field("Rotation", _rotation);
            _scale = EditorGUILayout.Vector3Field("Scale", _scale);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        _name = EditorGUILayout.TextField("Name instance:", _name);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        _predicate = EditorGUILayout.TextField("Select by name:", _predicate);
        if (GUILayout.Button("Select"))
        {
            if (!String.IsNullOrEmpty(_predicate))
                SelectObjectsByName(_predicate);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.Label("Selected objects: " + Selection.objects.Length);
        if (GUILayout.Button("Generate"))
        {
            Generate();
        }

        if (GUI.changed)
        {
            if (String.IsNullOrEmpty(_name) || _currentAsset == null || _currentAsset != _asset)
            {
                if (_asset != null)
                {
                    _name = _asset.name + " (Clone)";

                    _currentAsset = _asset;

                    this.Repaint();
                }
            }
        }
    }

    void OnSelectionChange()
    {
        this.Repaint();
    }

    private void SelectObjectsByName(string value)
    {
        // TODO: тут надо подумать, стоит ли искать во всем имени подстроку, или только в определенном месте?
        var list = FindObjectsOfType<Transform>().Where(s=>s.name.Contains(value));
        Selection.objects = list.ToArray();
    }

    private void Generate()
    {
        if (_asset != null && Selection.objects.Length > 0)
        {
            var objects = Selection.objects;
            for (var i = 0; i < objects.Length; i++)
            {
                var gameObj = FindObjectsOfType<GameObject>().First(s => s.name == objects[i].name);
                var rotQuartenion = Quaternion.identity;
                rotQuartenion.eulerAngles = _rotation;

                var obj = (GameObject)Instantiate(_asset, gameObj.transform.position, rotQuartenion);

                obj.transform.parent = gameObj.transform;
                obj.transform.localScale = _scale;
                obj.name = _name;
            }
        }
    }
}
}
