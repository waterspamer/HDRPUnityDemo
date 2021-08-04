using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Nettle.Core.SceneChecker {

    public class WaypiontData {
        public GameObject GameObject;
        public GameObject NavMeshManagerGO;
    }

    public class MaterialData {
        public List<GameObject> GameObjects;
        public Material Material;
    }

    public class SceneChecker : EditorWindow {
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

        private readonly HashSet<string> _incorrectShaderNames = new HashSet<string> { "Standard", "Standard (Roughness setup)", "Standard (Specular setup)" };

        private HashSet<object> _checkedObjects;
        private List<GameObject> _problemEvents = new List<GameObject>();
        private int _problemEventID;


        public int MaxDepth = 7;
        private int _currentDepth;

        private Texture2D _whiteBackground;
        private Vector2 _problemEventsScrollPosition;


        private Vector2 _problemZonesScrollPosition;
        private List<GameObject> _problemZones = new List<GameObject>();
        private int _problemZoneID;

        private Vector2 _problemWaypointsScrollPosition;
        private List<WaypiontData> _problemWaypoints = new List<WaypiontData>();
        private int _problemWaypointID;
        public float WaypointTolerance = 0.2f;
        public float WaypointMaxFixDistance = 2f;

        private Vector2 _missingScriptsScrollPosition;
        private List<GameObject> _missingScripts = new List<GameObject>();
        private int _missingScriptID;

        private Vector2 _problemRenderersScrollPosition;
        private List<Renderer> _problemRenderers = new List<Renderer>();
        private int _problemRenderersID;

        private Vector2 _problemMaterialsScrollPosition;
        private List<MaterialData> _problemMaterials = new List<MaterialData>();
        private int _problemMaterialsID;

        [MenuItem("Nettle/Scene Checker")]
        public static void ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(SceneChecker));
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("Scene Checker");
        }

        void Awake() {
            _whiteBackground = EditorGUIUtility.Load("Assets/UnityLib/Core/Utils/Editor/Textures/WhiteTrBackground.png") as Texture2D;
        }


        void OnGUI() {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Check all")) {
                CheckEvents();
                CheckZonesScale();
                CheckWaypoints();
                CheckMissingScripts();
                CheckRenderers();
                CheckMaterials();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Check events")) {
                CheckEvents();
            }
            MaxDepth = EditorGUILayout.IntField("MaxDepth", MaxDepth);

            if (_problemEvents.Count > 0) {
                if (_problemEvents.Exists(v => v == null)) {
                    _problemEvents = _problemEvents.Where(v => v != null).ToList();
                }
                CheckEvents(_problemEvents.ToArray());
                _problemEventsScrollPosition = EditorGUILayout.BeginScrollView(_problemEventsScrollPosition);
                _problemEventID = _problemEvents.FindIndex(v => v == Selection.activeGameObject);
                _problemEventID = GUILayout.SelectionGrid(_problemEventID, _problemEvents.ConvertAll(v => v.name).ToArray(), 1, SelectionListStyle());
                if (_problemEventID >= 0 && _problemEventID < _problemEvents.Count) {
                    Selection.activeGameObject = _problemEvents[_problemEventID];
                }
                EditorGUILayout.EndScrollView();

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Check zones scale")) {
                CheckZonesScale();
            }
            _problemZonesScrollPosition = EditorGUILayout.BeginScrollView(_problemZonesScrollPosition);
            if (_problemZones.Count > 0) {
                if (_problemZones.Exists(v => v == null)) {
                    _problemZones = _problemZones.Where(v => v != null).ToList();
                }
                CheckZonesScale(_problemZones.ToList());
                _problemZoneID = _problemZones.FindIndex(v => v == Selection.activeGameObject);
                _problemZoneID = GUILayout.SelectionGrid(_problemZoneID, _problemZones.ConvertAll(v => v.name).ToArray(), 1, SelectionListStyle());
                if (_problemZoneID >= 0 && _problemZoneID < _problemZones.Count) {
                    Selection.activeGameObject = _problemZones[_problemZoneID];
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Check waypoints")) {
                CheckWaypoints();
            }
            WaypointTolerance = EditorGUILayout.FloatField("Tolerance", WaypointTolerance);
            if (_problemWaypoints.Count > 0) {
                if (_problemWaypoints.Exists(v => v == null)) {
                    _problemWaypoints = _problemWaypoints.Where(v => v != null).ToList();
                }
                CheckWaypoints(_problemWaypoints.ToList());
                if (GUILayout.Button("Try fix")) {
                    TryFixWaypoints();
                }
                WaypointMaxFixDistance = EditorGUILayout.FloatField("Max fix distance", WaypointMaxFixDistance);
                _problemWaypointsScrollPosition = EditorGUILayout.BeginScrollView(_problemWaypointsScrollPosition);
                _problemWaypointID = _problemWaypoints.FindIndex(v => v.GameObject == Selection.activeGameObject);
                _problemWaypointID = GUILayout.SelectionGrid(_problemWaypointID, _problemWaypoints.ConvertAll(v => v.GameObject.name).ToArray(), 1, SelectionListStyle());
                if (_problemWaypointID >= 0 && _problemWaypointID < _problemWaypoints.Count) {
                    Selection.activeGameObject = _problemWaypoints[_problemWaypointID].GameObject;
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Check missing scripts")) {
                CheckMissingScripts();
            }
            _missingScriptsScrollPosition = EditorGUILayout.BeginScrollView(_missingScriptsScrollPosition);
            if (_missingScripts.Count > 0) {
                if (_missingScripts.Exists(v => v == null)) {
                    _missingScripts = _missingScripts.Where(v => v != null).ToList();
                }
                CheckMissingScripts(_missingScripts.ToList());
                _missingScriptID = _missingScripts.FindIndex(v => v == Selection.activeGameObject);
                _missingScriptID = GUILayout.SelectionGrid(_missingScriptID, _missingScripts.ConvertAll(v => v.name).ToArray(), 1, SelectionListStyle());
                if (_missingScriptID >= 0 && _missingScriptID < _missingScripts.Count) {
                    Selection.activeGameObject = _missingScripts[_missingScriptID];
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Check renderers")) {
                CheckRenderers();
            }
            _problemRenderersScrollPosition = EditorGUILayout.BeginScrollView(_problemRenderersScrollPosition);
            if (_problemRenderers.Count > 0) {
                if (_problemRenderers.Exists(v => v == null)) {
                    _problemRenderers = _problemRenderers.Where(v => v != null).ToList();
                }
                CheckRenderers(_problemRenderers.ToList());
                _problemRenderersID = _problemRenderers.FindIndex(v => v.gameObject == Selection.activeGameObject);
                _problemRenderersID = GUILayout.SelectionGrid(_problemRenderersID, _problemRenderers.ConvertAll(v => v.name).ToArray(), 1, SelectionListStyle());
                if (_problemRenderersID >= 0 && _problemRenderersID < _problemRenderers.Count) {
                    Selection.activeGameObject = _problemRenderers[_problemRenderersID].gameObject;
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Check materials")) {
                CheckMaterials();
            }
            _problemMaterialsScrollPosition = EditorGUILayout.BeginScrollView(_problemMaterialsScrollPosition);
            if (_problemMaterials.Count > 0) {
                if (_problemMaterials.Exists(v => v == null)) {
                    _problemMaterials = _problemMaterials.Where(v => v != null).ToList();
                }
                CheckMaterials(_problemMaterials.ToList());
                _problemMaterialsID = _problemMaterials.FindIndex(v => v.Material == Selection.activeObject);
                _problemMaterialsID = GUILayout.SelectionGrid(
                    _problemMaterialsID, _problemMaterials.ConvertAll(v => v.Material.name + "::" + v.GameObjects[0].name).ToArray(), 1, SelectionListStyle());
                if (_problemMaterialsID >= 0 && _problemMaterialsID < _problemMaterials.Count) {
                    Selection.activeObject = _problemMaterials[_problemMaterialsID].Material;
                    MaterialFinder.Focus(_problemMaterials[_problemMaterialsID].GameObjects[0].GetComponent<Renderer>(), _problemMaterials[_problemMaterialsID].Material);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

        }

        void OnFocus() {
            /*CheckEvents(_problemEvents.ToArray());
            CheckZonesScale(_problemZones.ToList());
            CheckWaypoints(_problemWaypoints.ToList());
            CheckMissingScripts(_missingScripts.ToList());
            CheckRenderers(_problemRenderers.ToList());
            CheckMaterials(_problemMaterials.ToList());*/
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
            return result;
        }

        public void CheckMaterials() {
            /*CheckMaterials(SceneUtils.FindSceneObjectsOfTypeAll<Renderer>()
                .SelectMany(v => v.sharedMaterials.Where(g => g != null), (renderer, material) => new MaterialData { GameObject = renderer.gameObject, Material = material })
                .GroupBy(v => v.Material).Select(v => v.First()).Where(v => v != null).ToList());*/
            CheckMaterials(SceneUtils.FindSceneObjectsOfTypeAll<Renderer>()
                .SelectMany(v => v.sharedMaterials.Where(g => g != null),
                    (renderer, material) => new KeyValuePair<GameObject, Material>(renderer.gameObject, material))
                .GroupBy(v => v.Value)
                .Select(v => new MaterialData { Material = v.Key, GameObjects = v.Select(g => g.Key).ToList() }).ToList());
        }

        public void CheckMaterials(List<MaterialData> problemMaterials) {
            _problemMaterials.Clear();
            _problemMaterials = problemMaterials.Where(
                v => {
                    v.GameObjects.RemoveAll(g => g == null || g.GetComponent<Renderer>() == null || !g.GetComponent<Renderer>().sharedMaterials.Contains(v.Material));
                    return v.GameObjects.Count > 0 && _incorrectShaderNames.Contains(v.Material.shader.name);
                }).ToList();
        }

        public void CheckRenderers() {
            CheckRenderers(SceneUtils.FindSceneObjectsOfTypeAll<Renderer>().ToList());
        }

        public void CheckRenderers(List<Renderer> problemRenderers) {
            _problemRenderers.Clear();
            _problemRenderers = problemRenderers.Where(
                v => v.sharedMaterials.Count(c => c == null || c.Equals(null)) > (v is ParticleSystemRenderer ? 1 : 0)
                ).ToList();
        }


        public void CheckMissingScripts() {
            CheckMissingScripts(SceneUtils.FindSceneObjectsOfTypeAll<GameObject>().ToList());
        }

        public void CheckMissingScripts(List<GameObject> missingScriptGameObjects) {
            _missingScripts.Clear();
            _missingScripts = missingScriptGameObjects.Where(v => v.GetComponents<Component>().Count(c => c == null || c.Equals(null)) > 0).ToList();
        }

        public void CheckWaypoints() {
            List<WaypiontData> waypointsData = new List<WaypiontData>();
            IEnumerable<Component> navMeshManagerComponents = SceneUtils.FindSceneObjectsOfTypeAll<Component>().Where(v => v.GetType().FullName == "Nettle.NavAgent.NavMeshManager");
            foreach (var navMeshManagerComponent in navMeshManagerComponents) {
                FieldInfo waypointSourceField = navMeshManagerComponent.GetType().GetField("WaypointsSource");
                Transform waypointsSource = waypointSourceField.GetValue(navMeshManagerComponent) as Transform;
                if (waypointsSource == null) {
                    continue;
                }
                for (int i = 0; i < waypointsSource.childCount; i++) {
                    waypointsData.Add(
                        new WaypiontData {
                            GameObject = waypointsSource.GetChild(i).gameObject,
                            NavMeshManagerGO = navMeshManagerComponent.gameObject
                        });
                }
            }
            CheckWaypoints(waypointsData);
        }

        public void CheckWaypoints(List<WaypiontData> waypointsData) {
            _problemWaypoints.Clear();
            foreach (var data in waypointsData) {
                Vector3 pointPosition = data.GameObject.transform.position;

                NavMeshHit hit = new NavMeshHit();
                if (!NavMesh.SamplePosition(pointPosition, out hit, WaypointTolerance, NavMesh.AllAreas)
                    || hit.distance > WaypointTolerance) {
                    _problemWaypoints.Add(data);
                    continue;
                }


                MeshCollider collider = data.NavMeshManagerGO.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = data.NavMeshManagerGO.gameObject.GetComponent<MeshFilter>().sharedMesh;
                Vector3 closestPoint = collider.ClosestPoint(pointPosition);
                float distance = Vector3.Distance(closestPoint, pointPosition);
                DestroyImmediate(collider);

                if (distance > WaypointTolerance) {
                    _problemWaypoints.Add(data);
                }
            }
        }

        public void TryFixWaypoints() {
            foreach (var waypointData in _problemWaypoints) {
                NavMeshHit hit = new NavMeshHit();
                if (NavMesh.SamplePosition(waypointData.GameObject.transform.position, out hit, WaypointMaxFixDistance, NavMesh.AllAreas)
                    || hit.distance < WaypointMaxFixDistance) {
                    waypointData.GameObject.transform.position = hit.position;
                }
            }
            CheckWaypoints(_problemWaypoints.ToList());
        }

        public void CheckZonesScale() {
            CheckZonesScale(SceneUtils.FindSceneObjectsOfTypeAll<VisibilityZone>().Select(v => v.gameObject).ToList());
        }

        public void CheckZonesScale(List<GameObject> zoneGameObjects) {
            _problemZones.Clear();
            foreach (var go in zoneGameObjects) {
                Vector3 scale = go.transform.localScale;
                if (Math.Abs(scale.x - scale.y) > 0.0001f || Math.Abs(scale.x - scale.z) > 0.0001f) {
                    _problemZones.Add(go);
                }
            }
        }

        public void CheckEvents() {
            CheckEvents(SceneUtils.FindSceneObjectsOfTypeAll<GameObject>().ToArray());
        }

        private void CheckEvents(params GameObject[] gameObjects) {
            _currentDepth = -1;
            _problemEvents.Clear();
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
                _currentDepth--;
                return;
            }

            Type type = obj.GetType();

            if (IgnoredType(type) || !_checkedObjects.Add(obj)) {
                _currentDepth--;
                return;
            }


            UnityEvent unityEvent = obj as UnityEvent;
            if (unityEvent != null && !_problemEvents.Contains(parentComponent.gameObject)) {
                for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++) {
                    if (!CheckEventMethod(unityEvent, i)) {
                        _problemEvents.Add(parentComponent.gameObject);
                        break;
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

        private bool IsObsolete(MemberInfo value) {
            var attributes = (ObsoleteAttribute[])value.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return (attributes != null && attributes.Length > 0);
        }

        private bool CheckEventMethod(UnityEvent unityEvent, int persistentEventIndex) {
            UnityEngine.Object target = unityEvent.GetPersistentTarget(persistentEventIndex);
            string methodName = unityEvent.GetPersistentMethodName(persistentEventIndex);
            if (!target || string.IsNullOrEmpty(methodName)) {
                return false;
            }

            BindingFlags flags = BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.Static |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;

            FieldInfo m_PersistentCalls = typeof(UnityEventBase).GetField("m_PersistentCalls", flags);
            object m_PersistentCallsValue = m_PersistentCalls.GetValue(unityEvent);
            FieldInfo m_Calls = m_PersistentCalls.FieldType.GetField("m_Calls", flags);
            IList calls = m_Calls.GetValue(m_PersistentCallsValue) as IList;
            PropertyInfo modeProperty = calls[persistentEventIndex].GetType().GetProperty("mode", flags);
            PersistentListenerMode mode = (PersistentListenerMode)modeProperty.GetValue(calls[persistentEventIndex], null);

            Type[] argumentTypes = new Type[0];

            switch (mode) {
                case PersistentListenerMode.EventDefined:
                    Debug.LogError("Not implemented!");
                    break;
                case PersistentListenerMode.Object:
                    argumentTypes = new[] { typeof(object), typeof(UnityEngine.Object) };
                    break;
                case PersistentListenerMode.Int:
                    argumentTypes = new[] { typeof(int) };
                    break;
                case PersistentListenerMode.Float:
                    argumentTypes = new[] { typeof(float) };
                    break;
                case PersistentListenerMode.String:
                    argumentTypes = new[] { typeof(string) };
                    break;
                case PersistentListenerMode.Bool:
                    argumentTypes = new[] { typeof(bool) };
                    break;
            }

            if (mode == PersistentListenerMode.Object) {
                object arguments = calls[persistentEventIndex].GetType().GetProperty("arguments", flags).GetValue(calls[persistentEventIndex], null);
                object value = arguments.GetType().GetProperty("unityObjectArgument", flags).GetValue(arguments, null);
                if (value == null || value.Equals(null)) {
                    return false;
                }
                int methodsFind = target.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(v => v.Name == methodName && v.GetParameters().Length == 1)
                    .Select(v => v.GetParameters()[0])
                    .Count(v => v.ParameterType == value.GetType());
                if (methodsFind == 1) {
                    return true;
                }
                if (methodsFind == 0) {
                    return false;
                }
                Debug.LogError("Found more than one methods!");
                return false;
            } else {
                return UnityEventBase.GetValidMethodInfo(target, methodName, argumentTypes) != null;

            }

            /**/

        }
    }

}