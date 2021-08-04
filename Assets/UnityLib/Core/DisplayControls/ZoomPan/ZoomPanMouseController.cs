using System;
using UnityEngine;

namespace Nettle {
    public class ZoomPanMouseController : MonoBehaviour {
        public ZoomPan ZoomPan;
        public NEvent PanActiveEvent;
        [ConfigField("PanEnable")]
        [SerializeField]
        private bool _panEnable = true;
        [ConfigField("ZoomEnable")]
        [SerializeField]
        private bool _zoomEnable = true;
        [ConfigField("PanSpeed")]
        [SerializeField]
        private float _panSpeed = 1.0f;

        public bool PanEnable { get => _panEnable; set => _panEnable = value; }
        public bool ZoomEnable { get => _zoomEnable; set => _zoomEnable = value; }
        public float PanSpeed { get => _panSpeed; set => _panSpeed = value; }

        void Reset() {
            ZoomPan = SceneUtils.FindObjectIfSingle<ZoomPan>();
        }

        void Update() {
            ProcessZoomPan();
        }

        private void ProcessZoomPan() {
            if (ZoomPan != null && ZoomPan.enabled) {
                if (PanEnable && (PanActiveEvent == null || PanActiveEvent)) {
                    ZoomPan.Move(Input.GetAxis("Mouse X") * PanSpeed, Input.GetAxis("Mouse Y") * PanSpeed);
                }
                if (ZoomEnable) {
                    float scrollValue = Input.GetAxis("Mouse ScrollWheel");
                    if (Math.Abs(scrollValue) > 0.001f) {
                        ZoomPan.DoZoom(scrollValue);
                    }
                }
            }
        }
    }
}
