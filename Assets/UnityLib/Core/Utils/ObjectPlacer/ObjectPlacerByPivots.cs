using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EasyButtons;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nettle {
    public class ObjectPlacerByPivots : MonoBehaviour {

#if UNITY_EDITOR
        public enum ObjectSelectionMethod {
            ByPivotName,
            Random
        }

        [Header ("Remove config")]
        public float CloseDistance = 3f;
        [Tooltip ("Remove instances with scale less than value")]
        public float LittleScale = 0.2f;
        public int RandomSeed = 0;
        [Range (0, 1)]
        public float SpawnChance = 1;

        [Space(10)]
        public bool RemoveExisting = true;
        public string SourceNamesPrefix = "";
        public string SourceNamesPostfix = "";
        public string DummyNamesPrefix = "";
        public string DummyNamesPostfix = "";
        public Transform PivotsRoot;
        public GameObjectList GameObjectList;
        public ObjectSelectionMethod SelectionMethod;
        [Space (10)]
        [Header ("Obsolete")]
        [System.Obsolete ("SourceGameObjects is an obsolete. Use GameObjectList")]
        [Tooltip ("SourceGameObjects is an obsolete. Use GameObjectList")]
        public List<GameObject> SourceGameObjects;

        void Awake () {
            EditorInit ();
        }

        void OnValidate () {
            EditorInit ();
        }

        void EditorInit () {
            if (!PivotsRoot) {
                PivotsRoot = transform;
            }
        }

        [Button]
        public void Place () {
            UnityEngine.Random.InitState (RandomSeed);
            IObjectPlacerPostprocessor[] postprocessors = GetComponents<IObjectPlacerPostprocessor> ();
            for (int pivotIndex = 0; pivotIndex < PivotsRoot.childCount; pivotIndex++) {
                if (UnityEngine.Random.Range (0, 1.0f) > SpawnChance) {
                    continue;
                }

                Transform pivot = PivotsRoot.GetChild (pivotIndex);

                if (RemoveExisting)
                {
                //Clear old objects
                    for (int i = 0; i < pivot.childCount; i++)
                    {
                        DestroyImmediate(pivot.GetChild(i).gameObject);
                    }
                }
                else
                {
                    if (pivot.childCount > 0)
                    {
                        continue;
                    }
                }
                GameObject goForSpawn = null;

                if (SelectionMethod == ObjectSelectionMethod.ByPivotName) {
                    Regex regex = new Regex (@"^(.*?)(_(?:lod)\d*)?_?([_|\s]\d*)?$");
                    Match match = regex.Match (pivot.name.ToLower ().Replace ("multiscatterobject_", ""));
                    if (match.Success && match.Groups.Count >= 2) {
                        goForSpawn = GameObjectList.Elements.Find (v => DummyNamesPrefix +v.name.ToLower () + DummyNamesPostfix == (SourceNamesPrefix + match.Groups[1].Value + SourceNamesPostfix).ToLower());
                    }
                } else {
                    goForSpawn = GameObjectList.GetRandomObject ();
                }

                if (goForSpawn) {
                    GameObject newGO = (GameObject) PrefabUtility.InstantiatePrefab (goForSpawn);
                    newGO.transform.SetParent (pivot, false);
                    foreach (IObjectPlacerPostprocessor postrprocessor in postprocessors) {
                        if (postrprocessor.IsEnabled ()) {
                            postrprocessor.PostprocessObject (newGO);
                        }
                    }
                }

            }
        }

        [Button]
        public void RemoveAllSpawnedObjects () {
            foreach (Transform t in PivotsRoot) {
                while (t.childCount > 0) {
                    Undo.DestroyObjectImmediate (t.GetChild (0).gameObject);
                }
            }
        }

        [Button]
        public void RemoveClosest () {
            bool again = true;
            int deletedCount = 0;
            while (again && deletedCount < 10000) {
                again = false;
                for (int pivotIndex = 0; pivotIndex < PivotsRoot.childCount; pivotIndex++) {
                    Transform pivot = PivotsRoot.GetChild (pivotIndex);
                    if (pivot.childCount == 0) {
                        continue;
                    }
                    for (int pivotIndex2 = 0; pivotIndex2 < PivotsRoot.childCount; pivotIndex2++) {
                        Transform pivot2 = PivotsRoot.GetChild (pivotIndex2);
                        if (pivotIndex == pivotIndex2 || pivot2.childCount == 0 ||
                            Vector3.Distance (pivot.position, PivotsRoot.GetChild (pivotIndex2).position) > CloseDistance) {
                            continue;
                        }

                        Bounds bounds1 = pivot.GetComponentInChildren<Renderer> ().bounds;
                        Bounds bounds2 = pivot2.GetComponentInChildren<Renderer> ().bounds;
                        if (!bounds1.Intersects (bounds2)) {
                            continue;
                        }

                        Transform removePivot;
                        if (bounds1.size.x * bounds1.size.y * bounds1.size.z < bounds1.size.x * bounds1.size.y * bounds1.size.z) {
                            removePivot = pivot;
                        } else {
                            removePivot = pivot2;
                        }

                        again = true;
                        RemoveFromPivoit (removePivot);
                        deletedCount++;
                        break;

                    }
                    if (again) {
                        break;
                    }
                }
            }
            Debug.Log (deletedCount + " deleted");
        }

        [Button]
        public void RemoveLittleScale () {
            int amount = 0;
            for (int pivotIndex = 0; pivotIndex < PivotsRoot.childCount; pivotIndex++) {
                Transform pivot = PivotsRoot.GetChild (pivotIndex);
                if (pivot.childCount == 0) {
                    continue;
                }
                if (pivot.localScale.x <= LittleScale) {
                    amount++;
                    RemoveFromPivoit (pivot);
                }
            }
            Debug.Log (amount + " deleted");
        }

        [Button]
        public void ShowStat () {
            int used = 0;
            for (int i = 0; i < PivotsRoot.childCount; i++) {
                Transform pivot = PivotsRoot.GetChild (i);
                if (pivot.childCount > 0) {
                    used++;
                }
            }
            Debug.Log ("Total pivots = " + PivotsRoot.childCount + "::used = " + used + "::free = " + (PivotsRoot.childCount - used));
        }
        private void RemoveFromPivoit (Transform pivot) {
            for (int i = 0; i < pivot.childCount; i++) {
                Undo.DestroyObjectImmediate (pivot.GetChild (i).gameObject);
            }
        }

#endif
    }
}