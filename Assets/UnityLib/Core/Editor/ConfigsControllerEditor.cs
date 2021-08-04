using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.SceneManagement;

namespace Nettle {

    [CustomEditor(typeof(ConfigsController))]
    public class ConfigsControllerEditor : Editor {
        private ReorderableList _list;
        private ConfigsController _source;

        private void OnEnable() {
            
            _source = target as ConfigsController;

            _list = new ReorderableList(serializedObject, serializedObject.FindProperty("Configs"), true, true, true, true);
            _list.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = _list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 30f, EditorGUIUtility.singleLineHeight), index.ToString());

                EditorGUI.PropertyField(new Rect(rect.x + 30, rect.y, rect.xMax - (rect.x + 30), EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Config"), GUIContent.none);
            };
            _list.drawHeaderCallback = rect => {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 100f, EditorGUIUtility.singleLineHeight), "Configs");
            };
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            _source.PathToFolder = EditorGUILayout.TextField("Path: ", _source.PathToFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(70f))) {
                _source.PathToFolder = EditorUtility.SaveFolderPanel("Save config to directory", Application.dataPath, "");
                if (_source.PathToFolder.IndexOf(Application.dataPath) == -1) {
                    _source.PathToFolder = "";
                } else {
                    _source.PathToFolder = _source.PathToFolder.Substring(Application.dataPath.Length + 1);
                }
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.Update();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save configs")) {
                _source.Serialize();
            }
            if (GUILayout.Button("Load configs")) {
                _source.Deserialize();
            }

            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Reimport overriding configs")) {
                _source.ReimportOverridingConfigs();
            }
            if (EditorGUI.EndChangeCheck()) {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
}
