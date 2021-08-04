using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Nettle {

    public class ObjectOutlineController : MonoBehaviour {
        private struct OutlinedObjectInfo {
            public GameObject GameObject;
            public Renderer Renderer;
            public int DefaultLayer;
        }
        public bool OutlineRoot = true;
        public bool _outlineChildren = true;
        [SerializeField]
        private Transform[] _excludeChildren;
        [SerializeField]
        private Transform[] _includeChildren;
        [SerializeField]
        //The main outline image effect shader is not ready for HDR yet. Uncomment this when it's ready
        //[ColorUsageAttribute(true,true)]
        private Color _uniqueColor = Color.white;
        public bool _initialState = false;
        private OutlinedObjectInfo[] _objects;
        private bool _isOn = false;
        private bool _init = false;
        private int _uniqueOutlineColorId;

        private void Reset() {
            bool outlineLayerExsist = false;
            for (int i = 0; i < 32; ++i) {
                if (LayerMask.LayerToName(i) == OutlineImageEffect.OutlineLayerName) {
                    outlineLayerExsist = true;
                    break;
                }
            }
            if (!outlineLayerExsist) {
                Debug.LogWarning("Consider creating layer with name '" + OutlineImageEffect.OutlineLayerName + "' for outlined objects");
            }
        }

        private void Awake() {
            if (!_init) {
                _uniqueOutlineColorId = Shader.PropertyToID("_UniqueOutlineColor");
                Initialize();
                ToggleOutline(_initialState);
            }
        }

        private void Initialize() {
            _init = true;
            List<Transform> transforms;
            if (_outlineChildren) {
                try {
                    System.Func<Transform, bool> predicate = x => !_excludeChildren.Contains(x)
                    && (OutlineRoot || x != transform)
                    && x.GetComponent<Renderer>() != null
                    && x.GetComponent<ParticleSystem>() == null;
                    transforms = GetComponentsInChildren<Transform>(true).Where(predicate).ToList();
                } catch {
                    transforms = new List<Transform>();
                }
            } else {
                transforms = new List<Transform> { transform };
                if (_includeChildren != null && _includeChildren.Length > 0) {
                    System.Func<Transform, bool> predicate = x => _includeChildren.Contains(x) && (OutlineRoot || x != transform);
                    transforms.AddRange(GetComponentsInChildren<Transform>(true).Where(predicate));
                }
            }
            _objects = new OutlinedObjectInfo[transforms.Count];
            for (int i = 0; i < _objects.Length; i++) {
                _objects[i] = new OutlinedObjectInfo() { GameObject = transforms[i].gameObject,Renderer = transforms[i].GetComponent<Renderer>(), DefaultLayer = transforms[i].gameObject.layer };
            }
        }

        public void ToggleOutline(bool on)
        {
            if (!_init)
            {
                Initialize();
            }
            _isOn = on;
            if (_objects == null)
            {
                return;
            }
            foreach (OutlinedObjectInfo obj in _objects)
            {
                if (OutlineImageEffect.SeparateLayerForOutlinedObjects)
                {
                    obj.GameObject.layer = on ? LayerMask.NameToLayer(OutlineImageEffect.OutlineLayerName) : obj.DefaultLayer;
                }
                if (obj.Renderer != null)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    obj.Renderer.GetPropertyBlock(block);
                    block.SetColor(_uniqueOutlineColorId, on ? _uniqueColor : Color.black);
                    obj.Renderer.SetPropertyBlock(block);
                }
            }
        }

        public void ToggleOutline() {
            ToggleOutline(!_isOn);
        }

    }
}
