using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nettle {

    public class SetColorByToggleState : MonoBehaviour {
        public Color ActiveColor;
        public Color PassiveColor;
        /// <summary>
        /// Toggle that controls the color of target graphic.
        /// </summary>
        [SerializeField]
        private Toggle _toggle;
        /// <summary>
        /// Graphic that must change its color.
        /// </summary>
        [SerializeField]
        private Graphic _targetGraphic;

        public void Reset() {
            EditorInit();
        }

        public void OnValidate() {
            EditorInit();
        }

        private void EditorInit() {
            if (_toggle == null) {
                _toggle = GetComponent<Toggle>();
            }

            if (_toggle != null && _targetGraphic == null) {
                _targetGraphic = _toggle.targetGraphic;
            }
        }

        private void Awake() {
            if (_toggle != null) {
                _toggle.onValueChanged.AddListener(SetColorByState);
                SetColorByState(_toggle.isOn);
            }
        }

        public void RefreshColor()
        {
            SetColorByState(_toggle.isOn);
        }

        public void SetColorByState(bool state) {
            if (_targetGraphic == null) { return; }
            _targetGraphic.color = state ? ActiveColor : PassiveColor;
        }
    }
}
