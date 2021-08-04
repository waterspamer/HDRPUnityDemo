using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nettle {
    public class ObjectFadeController : MonoBehaviour {
        private class MaterialReplacement {
            public Material Old;
            public Material New;
        }

        private const string _defaultFadeShader = "Nettle/Generated/MR_Standard_Fade";
        [SerializeField]
        private Shader _transparentShader;
        [SerializeField]
        private bool _fadeChildren = true;
        [SerializeField]
        [ConditionalHide ("_fadeChildren", true)]
        private Renderer[] _excludeChildren;
        [SerializeField]
        [Range (0, 1)]
        private float _initialFadeState = 1;
        [SerializeField]
        private float _fadeSpeed = 1;
        [SerializeField]
        private float _minAlpha = 0;
        [SerializeField]
        private float _maxAlpha = 1;
        [SerializeField]
        private string _colorPropertyName = "_Color";

        public bool IsVisible{
            get
            {
                return _fadeDirection > 0 || _fadeState>=_maxAlpha;
            }            
        }

        public float FadeSpeed { get => _fadeSpeed; set => _fadeSpeed = value; }
        public float MinAlpha {
            get => _minAlpha;
            set
            {
                float oldValue = _minAlpha;
                _minAlpha = value;
                if (_fadeState < value)
                {
                    SetFade(value);
                }
                else if (_fadeState>value&&Mathf.Abs(_fadeState-oldValue)<Mathf.Epsilon)
                {
                    FadeOut();
                }
            }
        }public float MaxAlpha {
            get => _maxAlpha;
            set
            {
                float oldValue = _minAlpha;
                _maxAlpha = value;
                if (_fadeState > _maxAlpha)
                {
                    SetFade(_maxAlpha);
                }
                else if (_fadeState<_maxAlpha&&Mathf.Abs(_fadeState-oldValue)<Mathf.Epsilon)
                {
                    FadeIn();
                }
            }
        }

        private List<MaterialReplacement> _materialReplacements = new List<MaterialReplacement> ();
        private List<Renderer> _fadedObjects = new List<Renderer>();
        private float _fadeState;
        private float _fadeDirection = 0;
        

        //Instantiate a material so that all objects using the same material end up using the same instance 
        private Material GetNewSharedMaterial (Material oldMaterial) {
            MaterialReplacement rep = _materialReplacements.Find (x => x.Old == oldMaterial);
            if (rep == null) {
                rep = new MaterialReplacement ();
                rep.Old = oldMaterial;
                rep.New = new Material (oldMaterial);
                _materialReplacements.Add (rep);
            }
            return rep.New;
        }

        private void Reset () {
            _transparentShader = Shader.Find (_defaultFadeShader);
        }

        private  void Awake()
        {
            _fadeState = _initialFadeState;
        }

        private void Start()
        {
            if (!_fadeChildren)
            {
                Renderer r = GetComponent<Renderer>();
                if (r != null)
                {
                    _fadedObjects.Add(r);
                }
            }
            else
            {
                try
                {
                    _fadedObjects.AddRange(GetComponentsInChildren<Renderer>(true).Where(x => _excludeChildren.Count(y => y == x) == 0).ToArray());
                }
                catch
                {
                }
            }
            foreach (Renderer r in _fadedObjects)
            {
                InstantiateMaterialsForRenderer(r);
            }
            SetFade(_initialFadeState);
        }

        private void InstantiateMaterialsForRenderer(Renderer r)
        {
            Material[] sharedMaterials = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    if (r.sharedMaterials[i] != null)
                    {
                        sharedMaterials[i] = GetNewSharedMaterial(r.sharedMaterials[i]);
                    }
                }
                r.sharedMaterials = sharedMaterials;
        }

        private void SetFade (float state) {
            _fadeState = Mathf.Clamp01 (state);
            if (_fadedObjects == null) {
                return;
            }
            foreach (Renderer r in _fadedObjects) {
                SetFadeForRenderer(r);
            }
        }

        private void SetFadeForRenderer(Renderer r)
        {
            if (r is SpriteRenderer)
            {
                SpriteRenderer sr = r as SpriteRenderer;
                Color color = sr.color;
                color.a = _fadeState;
                sr.color = color;
            }
            else
            {
                foreach (Material mat in r.sharedMaterials)
                {
                    if (mat == null)
                    {
                        continue;
                    }
                    if (_transparentShader != null)
                    {
                        int q = mat.renderQueue;
                        if (_fadeState < 1)
                        {
                            mat.shader = _transparentShader;
                        }
                        else
                        {
                            //Restore material's old shader
                            Material m = _materialReplacements.Find(x => x.New == mat).Old;
                            mat.shader = m.shader;
                        }
                        if (q >= 3000)
                        {
                            mat.renderQueue = q;
                        }
                    }
                    Color color = mat.GetColor(_colorPropertyName);
                    color.a = _fadeState;
                    mat.SetColor(_colorPropertyName, color);
                }
            }
        }

        public void AddRenderers(Renderer[] renderers)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                {
                    _fadedObjects.Add(r);
                    InstantiateMaterialsForRenderer(r);
                    SetFadeForRenderer(r);
                }                
            }
        }

        public void InstantSetFade (float state) {
            _fadeDirection = 0;
            SetFade (state);
        }

        public void FadeIn () {
            _fadeDirection = 1;
        }

        public void FadeOut () {
            _fadeDirection = -1;
        }

        public void StopFading () {
            _fadeDirection = 0;
        }

        public void Update () {
            if (_fadeDirection > 0) {
                if (_fadeState < _maxAlpha) {
                    SetFade (_fadeState + Time.deltaTime * _fadeSpeed);
                } else {
                    _fadeDirection = 0;
                    SetFade (_maxAlpha);
                }
            } else if (_fadeDirection < 0) {
                if (_fadeState > _minAlpha) {
                    SetFade (_fadeState - Time.deltaTime * _fadeSpeed);
                } else {
                    _fadeDirection = 0;
                    SetFade (_minAlpha);
                }
            }
        }

    }
}