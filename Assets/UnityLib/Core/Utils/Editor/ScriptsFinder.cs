using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nettle {

    /// <summary>
    /// Can find various scripts
    /// </summary>
    public class ScriptsFinder : EditorWindow {

        private static int countGameObjects = 0;
        private static int countComponents = 0;
        private static int countMissing = 0;

        private string _objectTypeToFind = "";
        private bool _strictSearch = false;

        [MenuItem("Nettle/Scripts Finder")]
        public static void ShowWindow() {
            GetWindow(typeof(ScriptsFinder));
        }


        public static Type GetType(string typeName) {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }


        public void OnGUI() {
            EditorGUILayout.Separator();

            if (GUILayout.Button("Find Objects With Scripts not is VisibilityControl and VisibilityZone")) {
                //Debug.Log("Time " + Time.time);
                int startTime = System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond;
                FindNotVisControlandZone();
                int delta = System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond - startTime;
                Debug.Log("Search Time " + (delta));

            }

            if (GUILayout.Button("Find all Visibility Tags in scene")) {
                FindAllVisiblityTagsInScene();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Children with Component", GUILayout.Width(200)))
            {
                /*
                Type typeToFind = GetType(_objectTypeToFind);
                if (typeToFind != null) {
                    if (typeToFind.IsSubclassOf(typeof(Component))) {
                        List<GameObject> newSelection = new List<GameObject>();
                        foreach (GameObject go in Selection.gameObjects) {
                            if (AssetUtils.IsObjectAPrefab(go)) {
                                Debug.LogWarning("Skipped object " + go.name + " because it's a prefab");
                                continue;
                            }
                            Component[] foundComponents = go.GetComponentsInChildren(typeToFind);
                            newSelection.AddRange(foundComponents.Select(x => x.gameObject));
                        }
                        Selection.objects = newSelection.ToArray();
                    }
                    else {
                        Debug.LogError("Type to find is not a component");
                    }
                }
                else {
                    Debug.LogError("Invalid type");
                }*/
                List<GameObject> foundObjects = new List<GameObject>();
                List<Transform> visitedTransforms = new List<Transform>();
                foreach (GameObject go in Selection.gameObjects)
                {
                    FindChildrenWithComponentRecursively(go.transform, foundObjects, visitedTransforms);
                }
                if (foundObjects.Count > 0)
                {
                    Selection.objects = foundObjects.ToArray();
                }else
                {
                    Debug.Log("Nothing found");
                }
            }
            _objectTypeToFind = GUILayout.TextField(_objectTypeToFind);
            GUILayout.EndHorizontal();
            _strictSearch = EditorGUILayout.Toggle("Strict search", _strictSearch);
        }

        private void FindChildrenWithComponentRecursively(Transform parent, List<GameObject> output, List<Transform> visitedTransforms)
        {
            if (visitedTransforms.Contains(parent))
            {
                return;
            }
            Component[] allComponents = parent.GetComponents<Component>();
            foreach (Component component in allComponents)
            {
                Type type = component.GetType();
                if ((!_strictSearch && type.FullName.Contains(_objectTypeToFind))||(_strictSearch && type.Name.Equals(_objectTypeToFind)))
                {
                    output.Add(parent.gameObject);
                    break;
                }
            }
            visitedTransforms.Add(parent);
            foreach(Transform child in parent)
            {
                FindChildrenWithComponentRecursively(child, output, visitedTransforms);
            }            
        }

        private static void FindNotVisControlandZone() {
            GameObject[] gameObjects = Selection.gameObjects;
            countGameObjects = 0;
            countComponents = 0;
            countMissing = 0;
            foreach (GameObject gObject in gameObjects) {
                CheckNotVisControlZone(gObject);
            }
            Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} ", countGameObjects, countComponents, countMissing));
        }

        private static void CheckNotVisControlZone(GameObject g) {
            countGameObjects++;
            string s;
            Transform t;
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                countComponents++;

                if ((components[i] is MonoBehaviour) && !(components[i] is VisibilityControl) && !(components[i] is VisibilityZone)) {

                    countMissing++;
                    s = g.name;
                    t = g.transform;
                    while (t.parent != null) {
                        s = t.parent.name + "/" + s;
                        t = t.parent;
                    }
                    //Debug.Log(s + " has an empty script attached in position: " + i, g);
                    Debug.LogError(s + " has an script not VisibilityControl or VisibilityZone in position: " + i, g);
                }
            }
            // Now recurse through each child GO (if there are any):
            foreach (Transform childT in g.transform) {
                //Debug.Log("Searching " + childT.name  + " " );
                CheckNotVisControlZone(childT.gameObject);
            }
        }

        private static void FindObjectsWithComponent<T>() {
            GameObject[] gameObjects = Selection.gameObjects;
            countGameObjects = 0;
            countComponents = 0;
            countMissing = 0;
            foreach (GameObject gObject in gameObjects) {
                IsObjectsWithComponent<T>(gObject);
            }
            Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} ", countGameObjects, countComponents, countMissing));
        }

        private static void IsObjectsWithComponent<T>(GameObject g) {
            countGameObjects++;
            string s;
            Transform t;
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                countComponents++;

                if (components[i] is T) {

                    countMissing++;
                    s = g.name;
                    t = g.transform;
                    while (t.parent != null) {
                        s = t.parent.name + "/" + s;
                        t = t.parent;
                    }

                    Debug.LogError(s + " has an search component in position: " + i, g);
                }
            }

            // Now recurse through each child GO (if there are any):
            foreach (Transform childT in g.transform) {
                IsObjectsWithComponent<T>(childT.gameObject);
            }
        }

        private static void FindAllVisiblityTagsInScene() {
            VisibilityControl[] controls = FindObjectsOfType<VisibilityControl>();
            List<string> tags = new List<string>();
            foreach (VisibilityControl control in controls) {
                foreach (string tag in control.Tags) {
                    if (!tags.Contains(tag)) {
                        tags.Add(tag);
                    }
                }
            }
            string result = "Found tags:\n";
            foreach (string tag in tags) {
                result += tag + "\n";
            }
            Debug.Log(result);
        }
    }
}
