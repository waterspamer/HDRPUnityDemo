using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nettle {

    public class ObjectPlacer : EditorWindow {
        private string _groundLayer = "Ground";
        private Transform _objectsParent = null;
        private GameObject _sceneEventsListener;
        private List<MeshCollider> _addedColliders = new List<MeshCollider>();

        private ObjectPlacerData _data;
        private bool[] _allowSpawnList = new bool[0];

        private bool _placingEnabled = true;

        private GameObject _newgo;
        private Vector3 _newgoDefaulAngles;
        private Vector3 _normal;

        [MenuItem("Nettle/Object Placer/Main", false, 0)]
        public static void ShowWindow() {
            EditorWindow window = GetWindow(typeof(ObjectPlacer));
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("Object Placer");
        }


        void Awake() {
            GameObject[] gos = FindObjectsOfType<GameObject>();
            foreach (GameObject go in gos) {
                if (go.layer == LayerMask.NameToLayer(_groundLayer)) {
                    if (!go.GetComponent<Collider>()) {
                        _addedColliders.Add(go.AddComponent<MeshCollider>());
                    }
                }
            }
            var listener = FindObjectOfType<ObjectPlacerSceneEventsListener>();
            if (!listener) {
                _sceneEventsListener = new GameObject("SceneEventListener");
                listener = _sceneEventsListener.AddComponent<ObjectPlacerSceneEventsListener>();
            }
            listener.MouseDown += MouseDown;
            listener.MouseUp += MouseUp;
            listener.MouseDrag += MouseDrag;
        }



        void OnDestroy() {
            foreach (MeshCollider collider in _addedColliders) {
                DestroyImmediate(collider);
            }
            DestroyImmediate(_sceneEventsListener);
        }

        void OnGUI() {
            _groundLayer = EditorGUILayout.TextField(_groundLayer);
            _data = (ObjectPlacerData)EditorGUILayout.ObjectField("Data", _data, typeof(ObjectPlacerData));
            _objectsParent = (Transform)EditorGUILayout.ObjectField("Objects parent", _objectsParent, typeof(Transform), true);

            string placingBtnText = _placingEnabled ? "Disable placing" : "Enable placing";
            if (GUILayout.Button(placingBtnText)) {
                _placingEnabled = !_placingEnabled;
            }

            if (_data) {

                if (_allowSpawnList.Length < _data.Objects.Count) {
                    _allowSpawnList = new bool[_data.Objects.Count];
                    for (var i = 0; i < _allowSpawnList.Length; i++) {
                        _allowSpawnList[i] = true;
                    }
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All")) {
                    for (var i = 0; i < _allowSpawnList.Length; i++) {
                        _allowSpawnList[i] = true;
                    }
                }

                if (GUILayout.Button("Deselect All")) {
                    for (var i = 0; i < _allowSpawnList.Length; i++) {
                        _allowSpawnList[i] = false;
                    }
                }
                GUILayout.EndHorizontal();

                for (var i = 0; i < _data.Objects.Count; i++) {
                    GUILayout.BeginHorizontal();
                    _allowSpawnList[i] = GUILayout.Toggle(_allowSpawnList[i], "");
                    EditorGUILayout.ObjectField("", _data.Objects[i].GameObject, typeof(GameObject), true);
                    GUILayout.EndHorizontal();
                }
            }

        }


        private void MouseDown(Vector2 mousePosition) {
            if (!_placingEnabled) {
                return;
            }
            RaycastHit hit;
            Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(
                new Vector3(Event.current.mousePosition.x, SceneView.lastActiveSceneView.camera.pixelHeight - Event.current.mousePosition.y));
            if (Physics.Raycast(ray, out hit)) {

                var allowableForSpawn = new List<ObjectPlacerData.ObjectToPlace>();
                for (var i = 0; i < _data.Objects.Count; i++) {
                    if (_allowSpawnList[i]) {
                        allowableForSpawn.Add(_data.Objects[i]);
                    }
                }
                if (allowableForSpawn.Count == 0) {
                    return;
                }
                _newgo = _data.Generate(allowableForSpawn);


                _newgo.transform.parent = _objectsParent;
                _newgo.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                _normal = hit.normal;
                _newgoDefaulAngles = _newgo.transform.localEulerAngles;

                RotateNewGO(mousePosition);
            }
            Selection.activeGameObject = _newgo;
        }

        private void MouseUp(Vector2 mousePosition) {
            if (!_placingEnabled) {
                return;
            }
            Selection.activeGameObject = _newgo;
            _newgo = null;
        }

        private void MouseDrag(Vector2 mousePosition) {
            if (!_placingEnabled) {
                return;
            }
            RotateNewGO(mousePosition);
            Selection.activeGameObject = _newgo;
        }

        private void RotateNewGO(Vector2 mousePosition) {
            if (_newgo) {
                _newgo.transform.localEulerAngles = _newgoDefaulAngles;
                Vector3 up = _newgo.transform.InverseTransformDirection(Vector3.up);
                Vector3 forward = _newgo.transform.InverseTransformDirection(Vector3.forward);
                Vector3 localNormal = _newgo.transform.InverseTransformDirection(_normal);
                Plane plane = new Plane(_normal, _newgo.transform.position);
                Ray cursorRay = HandleUtility.GUIPointToWorldRay(mousePosition);
                float intersectionDistance;
                if (plane.Raycast(cursorRay, out intersectionDistance)) {
                    Vector3 cursorOnPlane = cursorRay.GetPoint(intersectionDistance);
                    Vector3 rotatedForward = Quaternion.FromToRotation(up, localNormal) * forward;
                    Vector3 rotatedForwardWorld = _newgo.transform.TransformDirection(rotatedForward);
                    float signedAngle = Vector3.SignedAngle(rotatedForwardWorld, cursorOnPlane - _newgo.transform.position, _normal);

                    _newgo.transform.rotation *= Quaternion.FromToRotation(up, localNormal);
                    _newgo.transform.Rotate(_normal, signedAngle, Space.World);
                }
            }
        }

    }

}
