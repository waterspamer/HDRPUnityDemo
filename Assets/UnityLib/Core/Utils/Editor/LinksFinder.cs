using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using Nettle;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class LinksFinder : EditorWindow {
    /// Add a context menu named "Do Something" in the inspector
    /// of the attached script.

    private readonly HashSet<Type> _ignoredTypes = new HashSet<Type> {
            typeof(GameObject),
            typeof(Transform),
            typeof(Material),
            typeof(Mesh),
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            typeof(Animator),
            typeof(NavMeshObstacle),
            typeof(Texture2D),
            typeof(String),
            typeof(MethodBase),
            typeof(Evidence)
        };

    private HashSet<object> _checkedObjects;
    private List<GameObject> _linkedGO = new List<GameObject>();
    public int MaxDepth = 10;
    private int _currentDepth;
    private Texture2D _whiteBackground;
    private Vector2 _linkedGOScrollPosition;
    private int _linkedGO_ID;


    private List<GameObject> _linkedEvents = new List<GameObject>();
    private int _linkedEventsID;
    private Vector2 _linkedEventsScrollPosition;


    [MenuItem("GameObject/Find links", false, 49)]
    public static void FindLinks() {
        ShowWindow();
    }

    public static void ShowWindow() {
        LinksFinder window = (LinksFinder)GetWindow(typeof(LinksFinder));
        window.autoRepaintOnSceneChange = true;
        window.titleContent = new GUIContent("Links:" + Selection.activeGameObject.name);
        window.FindAllLinks();
    }

    void OnGUI() {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Linked events");
        EditorGUILayout.Space();
        if (_linkedEvents.Count > 0) {
            _linkedEventsScrollPosition = EditorGUILayout.BeginScrollView(_linkedEventsScrollPosition);
            _linkedEventsID = _linkedEvents.FindIndex(v => v == Selection.activeGameObject);
            _linkedEventsID = GUILayout.SelectionGrid(_linkedEventsID, _linkedEvents.ConvertAll(v => v.name).ToArray(), 1, SelectionListStyle());
            if (_linkedEventsID >= 0 && _linkedEventsID < _linkedEvents.Count) {
                Selection.activeGameObject = _linkedEvents[_linkedEventsID];
            }
            EditorGUILayout.EndScrollView();

        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Linked fields");
        EditorGUILayout.Space();
        if (_linkedGO.Count > 0) {
            _linkedGOScrollPosition = EditorGUILayout.BeginScrollView(_linkedGOScrollPosition);
            _linkedGO_ID = _linkedGO.FindIndex(v => v == Selection.activeGameObject);
            _linkedGO_ID = GUILayout.SelectionGrid(_linkedGO_ID, _linkedGO.ConvertAll(v => v.name).ToArray(), 1, SelectionListStyle());
            if (_linkedGO_ID >= 0 && _linkedGO_ID < _linkedGO.Count) {
                Selection.activeGameObject = _linkedGO[_linkedGO_ID];
            }
            EditorGUILayout.EndScrollView();

        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private GUIStyle SelectionListStyle() {
        GUIStyle result = new GUIStyle(GUI.skin.button);
        result.onFocused.background = null;
        result.onNormal.background = _whiteBackground;
        result.onHover.background = null;
        result.onActive.background = null;
        result.active.background = _whiteBackground;
        result.focused.background = null;
        result.normal.background = null;
        result.hover.background = null;
        result.alignment = TextAnchor.MiddleLeft;
        return result;
    }

    void Awake() {
        _whiteBackground = EditorGUIUtility.Load("Assets/UnityLib/Core/Utils/Editor/Textures/WhiteTrBackground.png") as Texture2D;
    }

    public void FindAllLinks() {
        CheckEvents(SceneUtils.FindSceneObjectsOfTypeAll<GameObject>().Where(v => !IsPrefab(v) && v != Selection.activeGameObject).ToArray());
    }



    private void CheckEvents(params GameObject[] gameObjects) {
        _currentDepth = -1;
        _linkedGO.Clear();
        _linkedEvents.Clear();
        _checkedObjects = new HashSet<object>();
        List<Component> components = new List<Component>();
        foreach (var go in gameObjects) {
            components.AddRange(go.GetComponents<Component>()
                .Where(v => v != null && !_ignoredTypes.Contains(v.GetType()))
            );
        }

        foreach (var component in components) {
            CheckObject(component, component);
        }
    }

    private void CheckObject(object obj, Component parentComponent) {
        _currentDepth++;
        if (_currentDepth >= MaxDepth || obj == null || obj.Equals(null)) {
            _currentDepth--;
            return;
        }

        Component component = obj as Component;
        if (component && component != parentComponent) {
            if (component.gameObject == Selection.activeGameObject && !_linkedGO.Contains(parentComponent.gameObject)) {
                _linkedGO.Add(parentComponent.gameObject);
            }
            _currentDepth--;
            return;
        }

        Type type = obj.GetType();

        if (IgnoredType(type) || !_checkedObjects.Add(obj)) {
            _currentDepth--;
            return;
        }


        UnityEvent unityEvent = obj as UnityEvent;
        if (unityEvent != null) {
            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++) {
                UnityEngine.Object target = unityEvent.GetPersistentTarget(i);
                if (target != null) {
                    GameObject targetGO = target as GameObject;
                    if (targetGO == null) {
                        targetGO = ((Component)target).gameObject;
                    }
                    if (Selection.activeGameObject == targetGO && !_linkedEvents.Contains(targetGO)) {
                        _linkedEvents.Add(targetGO);
                        break;
                    }
                }
            }
        }

        IEnumerable iEnumerable = obj as IEnumerable;
        if (iEnumerable != null) {
            IEnumerator iterator = iEnumerable.GetEnumerator();
            while (iterator.MoveNext()) {
                CheckObject(iterator.Current, parentComponent);
            }
        }

        List<MemberInfo> members = new List<MemberInfo>();
        members.AddRange(type.GetFields());
        members.AddRange(type.GetProperties());

        foreach (var member in members) {
            if (IsObsolete(member)) {
                continue;
            }
            object value = null;
            PropertyInfo property = member as PropertyInfo;
            if (property != null) {
                Type propertyType = property.PropertyType;
                if (!IgnoredType(propertyType)
                    && property.GetIndexParameters().Length == 0) {
                    try {
                        value = property.GetValue(obj, null);
                    } catch (Exception ex) {
                        Debug.LogError("Can't property.GetValue()!::" + propertyType + "::" + obj + "::" + ex);
                    }
                }
            } else {
                FieldInfo field = member as FieldInfo;
                if (!IgnoredType(field.FieldType)) {
                    value = field.GetValue(obj);
                }
            }
            CheckObject(value, parentComponent);

        }

        _currentDepth--;
    }

    private bool IgnoredType(Type type) {
        return type.IsPrimitive
               || type.IsEnum
               || !type.IsClass
               || (type.IsArray && IgnoredType(type.GetElementType()))
               || _ignoredTypes.Contains(type);
    }


    public bool IsPrefab(GameObject go) {
        return PrefabUtility.GetCorrespondingObjectFromSource(go) == null && PrefabUtility.GetPrefabObject(go) != null;
    }

    private bool IsObsolete(MemberInfo value) {
        var attributes = (ObsoleteAttribute[])value.GetCustomAttributes(typeof(ObsoleteAttribute), false);
        return (attributes != null && attributes.Length > 0);
    }



}