using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Nettle {
    public class MaterialFinder: EditorWindow {

        private static Material _materialToFind;
        private static List<Renderer> _foundRenderers = new List<Renderer>();

        private Vector2 _scrollViewPosition;

        [MenuItem("CONTEXT/Material/FindMaterial", false, 10002)]
        private static void FindMaterial(MenuCommand command) {
            _materialToFind = (Material)command.context;
            ShowWindow();
        }

        [MenuItem("Assets/Material Finder")]
        private static void FindMaterial() {
            _materialToFind = Selection.activeObject as Material;
            ShowWindow();
        }

        private static void ShowWindow() {
            FindRenderers();
            Material materialToFind = Selection.activeObject as Material;
            MaterialFinder window = GetWindow(typeof(MaterialFinder)) as MaterialFinder;
            window.titleContent = new GUIContent("MaterialFinder");   
        }

        private void OnGUI() {
            EditorGUI.BeginChangeCheck();
            _materialToFind = EditorGUILayout.ObjectField("Material to find", _materialToFind, typeof(Material), true) as Material;
            if (EditorGUI.EndChangeCheck()) {
                FindRenderers();
            }

            if (_foundRenderers.Count > 0) {
                if (GUILayout.Button("Select All")) {
                    SelectAll();
                }
            }

            _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 50));
            foreach (Renderer r in _foundRenderers) {
                if (r == null) {
                    continue;
                }
                if (!r.enabled || !r.gameObject.activeInHierarchy) {
                    GUI.color = Color.gray;
                }
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(r, typeof(Renderer), true);
                if (GUILayout.Button("Focus", GUILayout.Width(100))) {
                    Focus(r, _materialToFind);
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private static void FindRenderers() {
            if (_materialToFind == null) {
                return;
            }
           _foundRenderers.Clear();
            List<Renderer> renderers = SceneUtils.FindObjectsOfType<Renderer>(true);
            foreach (Renderer r in renderers) {
                for (int i = 0; i < r.sharedMaterials.Length; i++) {
                    if (r.sharedMaterials[i] == _materialToFind) {
                        _foundRenderers.Add(r);
                    }
                }
            }
        }

        public void SelectAll() {
            Selection.objects = _foundRenderers.Select(x => x.gameObject).ToArray();
        }

        public static void Focus(Renderer renderer, Material material) {
            if (renderer == null || material == null) {
                return;
            }

            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) {
                Debug.LogError("Cant find the scene view, WTF?7");
                return;
            }

            Vector3 focusPivot = renderer.transform.position;
            Mesh mesh = null;
            if (renderer is MeshRenderer) {

                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                if (filter != null) {
                    mesh = filter.sharedMesh;
                }
            }
            Vector3 normal = new Vector3(0, 1, -1);
            if (mesh != null) {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++) {
                    if (renderer.sharedMaterials[i] == material) {
                        int id = mesh.GetIndices(i)[0];
                        focusPivot = renderer.transform.TransformPoint(mesh.vertices[id]);
                        normal = renderer.transform.TransformDirection(mesh.normals[id]);
                        break;
                    }
                }
            }

            view.LookAt(focusPivot, Quaternion.LookRotation(-normal, Vector3.up), 3, false);
        }        
    }
}
