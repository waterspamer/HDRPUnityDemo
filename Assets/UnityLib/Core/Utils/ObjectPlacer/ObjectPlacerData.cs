using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nettle {

    [CreateAssetMenu(fileName = "ObjectPlacerData", menuName = "ObjectPlacer/Data")]
    public class ObjectPlacerData : ScriptableObject {
#if UNITY_EDITOR

        [Serializable]
        public class ObjectToPlace {
            public float Weight = 1;
            public GameObject GameObject;
        }

        [HideInInspector] public List<ObjectToPlace> Objects = new List<ObjectToPlace>();

        public virtual GameObject Generate() {
            return Generate(Objects);
        }

        public virtual GameObject Generate(List<ObjectToPlace> objects) {
            EditorGUI.BeginChangeCheck();
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(objects[RandomEx.Weight(objects.Select(v => v.Weight))].GameObject);
            Undo.RegisterCreatedObjectUndo(go, "ObjectPlacer::AddObject");
            return go;
        }
    #endif
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(ObjectPlacerData))]
    public class ObjectPlacerDataEditor : Editor {

        private bool _showObjects = true;

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            ObjectPlacerData data = (ObjectPlacerData)target;

            Rect objectsForPlacingRect = EditorGUILayout.BeginHorizontal();
            _showObjects = EditorGUILayout.Foldout(_showObjects, "Objects for placing", true);
            EditorGUILayout.EndHorizontal();
            List<ObjectPlacerData.ObjectToPlace> removeObjectToPlaces = new List<ObjectPlacerData.ObjectToPlace>();
            if (_showObjects) {
                foreach (var objectToPlace in data.Objects) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 50f;
                    objectToPlace.Weight = EditorGUILayout.FloatField("Weight", objectToPlace.Weight, GUILayout.ExpandWidth(false));
                    objectToPlace.GameObject = (GameObject)EditorGUILayout.ObjectField("", objectToPlace.GameObject, typeof(GameObject), false);
                    if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) {
                        removeObjectToPlaces.Add(objectToPlace);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add object")) {
                    data.Objects.Add(new ObjectPlacerData.ObjectToPlace());
                }
            }
            foreach (var objectToPlace in removeObjectToPlaces) {
                data.Objects.Remove(objectToPlace);
            }




            Event evt = Event.current;
            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!objectsForPlacingRect.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences) {
                            var objectToPlace = new ObjectPlacerData.ObjectToPlace();
                            objectToPlace.Weight = 1f;
                            objectToPlace.GameObject = (GameObject)draggedObject;
                            data.Objects.Add(objectToPlace);
                        }
                    }
                    break;
            }

        }
    }
    #endif
}