using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class OutlineSettings : MonoBehaviour {
        [SerializeField]
        [Range(0, 5)]
        private float _outlineSize = 1;
        [Range(0, 20)]
        [SerializeField]
        private float _outlineStrength = 2;
        [SerializeField]
        private Color _outlineColor = Color.red;
        [SerializeField]
        private Shader _outlinedObjectShader;
        private OutlineImageEffect _outline;

        private void Reset() {
            _outlinedObjectShader = Shader.Find("Hidden/OutlinedObject");
        }

        public void Apply() {
            if (_outline == null) {
                _outline = FindObjectOfType<OutlineImageEffect>();
            }
            _outline._outlineColor = _outlineColor;
            _outline._outlineSize = _outlineSize;
            _outline._outlineStrength = _outlineStrength;
            _outline._outlinedObjectShader = _outlinedObjectShader;
        }
    }
}
