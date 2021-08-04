using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Nettle {

    public class VisibilityZone : MonoBehaviour {
        public VisibilityZoneGroup Group;
        public VisibilityZoneTransitionRules[] TransitionRules;
        [SerializeField]
        [FormerlySerializedAs ("VisibilityTag")]
        private string _visibilityTag;
        [SerializeField]
        [FormerlySerializedAs ("FastSwitchingTo")]
        public bool _fastSwitchingTo = true;
        [SerializeField]
        [FormerlySerializedAs ("FastSwitchingFrom")]
        public bool _fastSwitchingFrom = true;
        [Space (20)]
        [SerializeField]
        [FormerlySerializedAs ("MinZoom")]
        private float _minZoom = 0.1f;
        [Space (20)]
        [SerializeField]
        [FormerlySerializedAs ("MaxZoom")]
        private float _maxZoom = 1.2f;
        [FormerlySerializedAs ("PanInside")]
        [SerializeField]
        private bool _zoomPanInside = false;
        [Space (20)]
        [SerializeField]
        private bool _showGizmo = true;
        [SerializeField]
        private bool _showGizmoIfNotSelected;
        [SerializeField]
        private bool _showZoomGizmo = true;
        [SerializeField]
        private bool _showFrustumGizmo = false;
        public enum Aspect { Aspect_Free, Aspect_16x9, Aspect_16x10, Aspect_4x3, Aspect_1x1 }

        [SerializeField]
        private Aspect _screenAspect = Aspect.Aspect_16x9;
        [SerializeField]
        [FormerlySerializedAs ("HorizontalLines")]
        private float[] _horizontalLines;
        [SerializeField]
        [FormerlySerializedAs ("VerticalLines")]
        private float[] _verticalLines;
        [SerializeField]
        [Tooltip ("If set to true, showing this zone will unload currently loaded additive scene")]
        private bool _isMainScene = false;
        [SerializeField]
        private bool _isStatic = false;
        [SerializeField]
        private Vector3 _staticLeftEye;
        [SerializeField]
        private Vector3 _staticRightEye;
        [SerializeField]
        private bool _overrideMaxSceneDepth;

        [SerializeField]
        private bool isDrawZoneMesh = false;
        [SerializeField]
        private bool isDrawZoneMeshOnUpdate = false;

        [SerializeField]
        private MotionParallaxDisplay motionParallaxDisplay;

        [SerializeField]
        private Material zoneMat;
        [SerializeField]
        private MeshFilter zoneMeshFilter;

        //show scene from last position on which it was hided
        public bool isResumingLastPosition = false;

        [SerializeField] private Transform DisplayTransform;
        [SerializeField] private GameObject DisplayDummyPrefab;
        [SerializeField] private GameObject lastDisplayPos;
       public Transform LastTransform;


        [SerializeField]
        private float zoneMeshOffset_Y = 0.001f;
        [SerializeField]
        private MeshRenderer zoneMeshRenderer;

        //features for desk and dynamic menu

        public Vector3 menuOffset = new Vector3(1,0,0);

        [SerializeField]
        public String VisibleName;
        
        public bool OverrideMaxSceneDepth {
            get {
                return _overrideMaxSceneDepth;
            }
        }

        [SerializeField]
        [ConditionalHide("_overrideMaxSceneDepth",true)]
        private float _maxSceneDepth = -50;
        public float MaxSceneDepth {
            get {
                return _maxSceneDepth;
            }
        }
        public Vector3 VisualCenter;

        public bool IsStaticMP3D {
            get { return _isStatic; }
            set {
                _isStatic = value;
                if (OnChangeStaticState != null)
                    OnChangeStaticState.Invoke (value);
            }
        }

        public Renderer AttachedRenderer{
            get
            {
                if (_attachedRenderer == null)
                {
                    _attachedRenderer = GetComponent<Renderer>();
                }
                return _attachedRenderer;
            }
        }
        private Renderer _attachedRenderer;

        public string VisibilityTag { get => _visibilityTag; set => _visibilityTag = value; }
        protected bool FastSwitchingTo { get => _fastSwitchingTo; private set => _fastSwitchingTo = value; }
        protected bool FastSwitchingFrom { get => _fastSwitchingFrom; private set => _fastSwitchingFrom = value; }
        public float MinZoom { get => _minZoom; set => _minZoom = value; }
        public float MaxZoom { get => _maxZoom; set => _maxZoom = value; }
        public bool ZoomPanInside { get => _zoomPanInside; set => _zoomPanInside = value; }
        public Vector3 StaticLeftEye { get => _staticLeftEye; set => _staticLeftEye = value; }
        public Vector3 StaticRightEye { get => _staticRightEye; set => _staticRightEye = value; }

        public event Action<bool> OnChangeStaticState;
        public UnityEvent OnShowed = new UnityEvent ();
        public UnityEvent OnTransitionComplete = new UnityEvent ();
        public UnityEvent OnHided = new UnityEvent ();

        private MaterialPropertyBlock _rendererProperties;
        private MaterialPropertyBlock RendererProperies{
            get
            {
                if(_rendererProperties == null){
                    _rendererProperties = new MaterialPropertyBlock();
                }
                return _rendererProperties;
            }            
        }



        public bool FastSwitchFrom (VisibilityZone target) {
            if (target == null) {
                return true;
            }

            var settings = target.GetTransitionSettings (this);
            if (settings != null) {
                return settings.FastSwitch;
            } else {
                return target.FastSwitchingFrom || FastSwitchingTo;
            }
        }

        public bool FastSwitchFromPrevious () {
            return FastSwitchFrom (VisibilityZoneViewer.Instance.PreviousZone);
        }

        public VisibilityZoneTransition GetTransitionSettings (VisibilityZone target) {
            if (TransitionRules != null) {
                return (from rule in TransitionRules where rule.Match (target) select rule.TransitionSettings).FirstOrDefault ();
            }
            return null;
        }

        public void SetBorderColor(Color color)
        {
            if (AttachedRenderer != null)
            {
                AttachedRenderer.GetPropertyBlock(RendererProperies);
                _rendererProperties.SetColor("_Color", color);
                AttachedRenderer.SetPropertyBlock(_rendererProperties);
            }
        }
        public void ResetBorderColor()
        {
            if (AttachedRenderer != null)
            {
                AttachedRenderer.SetPropertyBlock(null);
            }
        }

        [EasyButtons.Button ()]
        public void ShowMe () {
            VisibilityZoneViewer.Instance.ShowZone (name);
        }

        public void Show () {
            OnShowed?.Invoke ();
            if(isDrawZoneMesh){
                zoneMeshRenderer.enabled = true;
            }
            if (_isMainScene) {
                AdditiveSceneLoader.UnloadCurrent ();
            }
        }

        public void TransitionComplete () {
            OnTransitionComplete?.Invoke ();
        }

        public void Hide () {
            if(isDrawZoneMesh){
                zoneMeshRenderer.enabled = false;
            }
            if(isResumingLastPosition){
                if(lastDisplayPos!=null){
                    Destroy(lastDisplayPos);
                }
                Debug.Log(DisplayTransform);
                lastDisplayPos = Instantiate(DisplayDummyPrefab,DisplayTransform.position,DisplayTransform.rotation);
                LastTransform = lastDisplayPos.transform;
                LastTransform.localScale = DisplayTransform.localScale;
            }
            OnHided?.Invoke ();
        }

        void OnValidate () {
            EditorInit ();
        }

        void Reset () {
            EditorInit ();
            _horizontalLines = new float[0];
            _verticalLines = new float[0];
        }

        void EditorInit () {

            MaxZoom = MaxZoom < 1f ? 1f : MaxZoom;
            MinZoom = Mathf.Clamp01 (MinZoom);
        }

        public Transform GetTransform () {
            return transform;
        }

        public float GetAspect () {
            if (_screenAspect == Aspect.Aspect_Free) {
                return Screen.width / (float) Screen.height;
            } else if (_screenAspect == Aspect.Aspect_16x9) {
                return 16.0f / 9.0f;
            } else if (_screenAspect == Aspect.Aspect_16x10) {
                return 16.0f / 10.0f;
            } else if (_screenAspect == Aspect.Aspect_4x3) {
                return 4.0f / 3.0f;
            }
            return 1.0f;
        }

        void DrawRect (Vector3[] points, Color color) {
            Gizmos.color = color;
            for (int i = 0; i < 4; ++i) {
                Gizmos.DrawLine (points[i], points[(i + 1) % 4]);
            }
        }

        public static Vector3[] GetLocalSpaceRect (float aspect, float zoom = 1.0f) {
            return new [] {
            new Vector3 (-zoom, 0f, zoom / aspect),
            new Vector3 (zoom, 0f, zoom / aspect),
            new Vector3 (zoom, 0f, -zoom / aspect),
            new Vector3 (-zoom, 0f, -zoom / aspect)
            };
        }

        public Vector3[] LocalToWorldSpacePoints (Vector3[] points) {
            return points.Select (v => GetTransform ().localToWorldMatrix.MultiplyPoint3x4 (v)).ToArray ();
        }

        public Vector3[] GetMaxViewRectWorldSpace () {
            return LocalToWorldSpacePoints (GetLocalSpaceRect (GetAspect (), MaxZoom));
        }

        public NTransform GetNTransform () {
            return new NTransform (GetTransform ());
        }

        void DrawGizmos () {
            if (!_showGizmo) return;
            var oldMatrix = Gizmos.matrix;
            Gizmos.matrix = GetTransform ().localToWorldMatrix;

            DrawRect (GetLocalSpaceRect (GetAspect (), 1.0f), Color.blue);

            if (_showZoomGizmo) {
                if (MinZoom != 0f) {
                    DrawRect (GetLocalSpaceRect (GetAspect (), MinZoom), Color.red);
                }

                if (MaxZoom != 0f) {
                    DrawRect (GetLocalSpaceRect (GetAspect (), MaxZoom), Color.yellow);
                }
            }
            if (_horizontalLines != null && _horizontalLines.Length > 0) {
                foreach (var horizontalLine in _horizontalLines) {
                    Gizmos.DrawLine (new Vector3 (-1.0f, 0.0f, horizontalLine), new Vector3 (1.0f, 0.0f, horizontalLine));
                }
            }
            if (_verticalLines != null && _verticalLines.Length > 0) {
                foreach (var verticalLine in _verticalLines) {
                    Gizmos.DrawLine (new Vector3 (verticalLine, 0.0f, -1.0f / GetAspect ()),
                        new Vector3 (verticalLine, 0.0f, 1.0f / GetAspect ()));
                }
            }

#if UNITY_EDITOR
            if (_showFrustumGizmo && ((Selection.gameObjects.Length == 1 && Selection.activeGameObject == gameObject) || _showGizmoIfNotSelected)) {
                float scale = 2 / 1.42f;
                Vector3 eyesPos = new Vector3 (0, (1.65f - 0.54f) * scale, (-0.85f) * scale);
                Vector3[] localSpaceRect = GetLocalSpaceRect (GetAspect (), 1.0f);
                Gizmos.color = Color.red;
                foreach (Vector3 corner in localSpaceRect) {
                    Gizmos.DrawRay (eyesPos, corner - eyesPos);
                }
                for (var i = 0; i < localSpaceRect.Length; i++) {
                    Gizmos.color = new Color (0, 1, 0, 0.75f);
                    Mesh mesh = new Mesh ();
                    mesh.vertices = new [] { eyesPos, localSpaceRect[i], localSpaceRect[(i + 1) % 4] };
                    mesh.triangles = new [] { 0, 1, 2, 2, 1, 0 };
                    mesh.RecalculateNormals ();
                    Gizmos.DrawMesh (mesh);
                }
            }
#endif
            Gizmos.matrix = oldMatrix;
        }

        void OnDrawGizmos () {
            if (_showGizmoIfNotSelected) {
                DrawGizmos ();
            }
        }

        void OnDrawGizmosSelected () {
            if (!_showGizmoIfNotSelected) {
                DrawGizmos ();
            }
            if (transform.localScale.x != transform.localScale.y) {
                if (transform.localScale.x == transform.localScale.z) {
                    transform.localScale = new Vector3 (transform.localScale.y, transform.localScale.y, transform.localScale.y);
                } else {
                    transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.x, transform.localScale.x);
                }
            } else if (transform.localScale.y != transform.localScale.z) {
                if (transform.localScale.x == transform.localScale.y) {
                    transform.localScale = new Vector3 (transform.localScale.z, transform.localScale.z, transform.localScale.z);
                } else {
                    transform.localScale = new Vector3 (transform.localScale.y, transform.localScale.y, transform.localScale.y);
                }
            } else if (transform.localScale.z != transform.localScale.x) {
                if (transform.localScale.z == transform.localScale.y) {
                    transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.x, transform.localScale.x);
                } else {
                    transform.localScale = new Vector3 (transform.localScale.z, transform.localScale.z, transform.localScale.z);
                }
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere (transform.TransformPoint (VisualCenter), 0.01f * transform.localScale.x);

        }

        internal void GetTransitionRules (VisibilityZone activeZone) {
            throw new NotImplementedException ();
        }

        void Start(){
            if(!isDrawZoneMesh)return;
            var zoneMesh = new Mesh();
            if(zoneMeshFilter==null){
                zoneMeshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if(zoneMeshRenderer==null){
                zoneMeshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            zoneMeshRenderer.sharedMaterial = zoneMat;
            System.Collections.Generic.List<Vector3> list = new System.Collections.Generic.List<Vector3>();
            foreach (var item in GetLocalSpaceRect (GetAspect (), 1.0f))
            {
                list.Add(new Vector3(item.x,item.y+zoneMeshOffset_Y,item.z));
            }
            zoneMesh.SetVertices(list);
            int[] tris = new int[6]
            {
                // lower left triangle
                0, 1, 2,
                // upper right triangle
                2, 3, 0
            };
            zoneMesh.triangles = tris;
            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            };
            zoneMesh.uv = uv;
            Vector3[] normals = new Vector3[4]
            {
                new Vector3(0,1,0),
                new Vector3(0,1,0),
                new Vector3(0,1,0),
                new Vector3(0,1,0)
            };
            zoneMesh.normals = normals;
            zoneMeshFilter.mesh = zoneMesh; 
            zoneMeshRenderer.enabled = false;
        }

        void Update(){
            if(!isDrawZoneMeshOnUpdate) return;
            System.Collections.Generic.List<Vector3> list = new System.Collections.Generic.List<Vector3>();
            var zoneMesh = new Mesh();
            Vector3[] corners = new Vector3[4];

            motionParallaxDisplay.GetWorldScreenCorners(out corners);

            foreach (var item in corners)
            {//works only without zoom change
                list.Add(new Vector3(-item.x/DisplayTransform.localScale.x,
                (item.y+zoneMeshOffset_Y)/DisplayTransform.localScale.y,
                (-item.z)/DisplayTransform.localScale.z));
            }
            zoneMesh.SetVertices(list);
            int[] tris = new int[6]
            {
                // lower left triangle
                0, 1, 2,
                // upper right triangle
                2, 3, 0
            };
            zoneMesh.triangles = tris;
            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            };
            zoneMesh.uv = uv;
            zoneMeshFilter.mesh = zoneMesh; 

        }
    }
}