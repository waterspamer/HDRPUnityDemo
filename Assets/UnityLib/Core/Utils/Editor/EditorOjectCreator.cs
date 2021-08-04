using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nettle{
    public class EditorOjectCreator : EditorWindow {
        private delegate void ObjectAction(GameObject obj);

        private GameObject _instantiateSource;

        [MenuItem("Nettle/Object Creator")]
        private static void ShowWindow() {
            EditorOjectCreator window = GetWindow(typeof(EditorOjectCreator)) as EditorOjectCreator;
            window.titleContent = new GUIContent("ObjectCreator");
            window.maxSize = new Vector2(400, 100);
            window.minSize = new Vector2(400, 100);
        }

        private void OnGUI() {
            if (GUILayout.Button("Create Parents")) {
                PerformActionOnSelection(CreateParents);
            }
            if (GUILayout.Button("Create child")) {
                PerformActionOnSelection(CreateChild);
            }
            _instantiateSource = EditorGUILayout.ObjectField("Object to instantiate", _instantiateSource, typeof(GameObject),true) as GameObject;
            if (GUILayout.Button("Instantiate object")) {
                if (_instantiateSource != null) {
                    PerformActionOnSelection(InstantiateObject);
                }
            }
        }

        private void PerformActionOnSelection(ObjectAction action) {
            GameObject[] objs = UnityEditor.Selection.gameObjects;
            foreach (GameObject obj in objs) {
                action(obj);
            }
        }

        private void CreateParents(GameObject obj) {
            GameObject parent = new GameObject();
            parent.name = obj.name + "_root";
            parent.transform.parent = obj.transform.parent;
            parent.transform.position = obj.transform.position;
            parent.transform.rotation = Quaternion.identity;
            parent.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(parent, "Create Parents");
            Undo.SetTransformParent(obj.transform, parent.transform, "Create Parents");

        }

        private void CreateChild(GameObject obj) {
            var newChild = new GameObject();
            newChild.transform.SetParent(obj.transform, false);
            newChild.transform.Reset();
            Undo.RegisterCreatedObjectUndo(newChild, "Create Child");
        }

        private void InstantiateObject(GameObject obj) {
            GameObject copy = PrefabUtility.InstantiatePrefab(_instantiateSource) as GameObject;
            copy.name = _instantiateSource.name;
            copy.transform.SetParent(obj.transform, false);
            Undo.RegisterCreatedObjectUndo(copy,"Instantiate Object");
        }
    }
}
