using UnityEngine;

namespace Nettle {

    public class ObjectRotationMouseController : MonoBehaviour {
        public ObjectRotationBase Target;
        public KeyCode Key = KeyCode.A;
        public NEventToggle ZoomPanDisable;
        private Vector3 _lastMouseCoord;
        private bool _rotating;


        private void OnDisable() {
            _rotating = false;
        }

        void Update() {
            if (Input.GetKeyDown(Key)) {
                _rotating = true;
                if (ZoomPanDisable != null) {
                    ZoomPanDisable.SetToggle(true);
                }
                _lastMouseCoord = Input.mousePosition;
            }
            if (Input.GetKeyUp(Key)) {
                if (ZoomPanDisable != null) {
                    ZoomPanDisable.SetToggle(false);
                }
                _rotating = false;
            }

            if (_rotating) {
                var mouseDelta = Input.mousePosition - _lastMouseCoord;
                _lastMouseCoord = Input.mousePosition;
                if (Target != null) {
                    Target.Rotate(mouseDelta);
                }
            }
        }
    }
}
