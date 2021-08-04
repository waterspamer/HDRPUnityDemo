using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class FlareRenderer : MonoBehaviour {
        private const int _defaultCapacity = 100;
        public float MaxSize = 5;
        public float MinSize = 0.5f;
        public float MaxSizeDistance = 1000;
        public float MinSizeDistance = 10;
        [Tooltip ("How deep can a flare penetrate an opaque object before it gets occluded by it")]
        public float MaxSurfacePenetration = 0.2f;
        [SerializeField]
        private Shader _flareShader;
        [SerializeField]
        private Color _globalColorModifier = Color.white;
        [SerializeField]
        private Texture2D _texture;

        private Material _flareMaterial;
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private Material _material;

        private List<FlareSource> _sources = new List<FlareSource> ();

        private static FlareRenderer _instance;
        private List<Vector3> _vertices;
        private List<Vector2> _uvs;
        private List<int> _indices;
        private List<Color> _colors;

        public static FlareRenderer Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<FlareRenderer> ();
                }
                return _instance;
            }
        }

        public void RegisterFlare (FlareSource source) {
            _sources.Add (source);
        }

        public void UnregisterFlare (FlareSource source) {
            _sources.Remove (source);
        }

        private void Reset () {
            _flareShader = Shader.Find ("Hidden/NettleFlare");
        }

        private void Start () {
            _meshFilter = new GameObject ("Flare mesh").AddComponent<MeshFilter> ();
            Renderer rend = _meshFilter.gameObject.AddComponent<MeshRenderer> ();
            _mesh = new Mesh ();
            _meshFilter.sharedMesh = _mesh;
            _material = new Material (_flareShader);
            rend.sharedMaterial = _material;
            _vertices = new List<Vector3> (_defaultCapacity);
            _colors = new List<Color> (_defaultCapacity);
            _indices = new List<int> (_defaultCapacity);
            _uvs = new List<Vector2> (_defaultCapacity);
        }

        void LateUpdate () {
            _vertices.Clear ();
            _colors.Clear ();
            _uvs.Clear ();
            _indices.Clear ();
            int i = 0;
            foreach (FlareSource source in _sources) {
                if ((source.FlareColor.r == 0 && source.FlareColor.g == 0 && source.FlareColor.b == 0) || source.FlareColor.a == 0) {
                    continue;
                }
                _vertices.Add (source.transform.position);
                _colors.Add (source.FlareColor*_globalColorModifier);
                _indices.Add (i);
                _uvs.Add (new Vector2 (source.Scale, source.Intensity));
                i++;
            }
            _mesh.Clear ();
            _mesh.SetVertices (_vertices);
            _mesh.SetIndices (_indices.ToArray (), MeshTopology.Points, 0);
            _mesh.SetColors (_colors);
            _mesh.SetUVs (0, _uvs);

            _material.SetTexture ("_MainTex", _texture);
            _material.SetFloat ("_MaxSize", MaxSize);
            _material.SetFloat ("_MinSize", MinSize);
            _material.SetFloat ("_MaxSizeDistance", MaxSizeDistance);
            _material.SetFloat ("_MinSizeDistance", MinSizeDistance);
            _material.SetFloat ("_MaxMeshPenetration", MaxSurfacePenetration);
        }

    }
}