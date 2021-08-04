using UnityEngine;
using UnityEngine.EventSystems;

namespace Nettle {
    public class ZoomPanGuiDragger : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler {

        public PointerEventData.InputButton Button = PointerEventData.InputButton.Middle;
        [SerializeField]
        private ZoomPanMouseController _mouseController;

        private void Reset() {
            _mouseController = FindObjectOfType<ZoomPanMouseController>();
        }

        private void Start() {
            if (_mouseController == null) {
                _mouseController = FindObjectOfType<ZoomPanMouseController>();
            }
            _mouseController.PanEnable = false;
            _mouseController.ZoomEnable = false;
        }

        public void OnBeginDrag(PointerEventData data) {
            if (data.button == Button) {
                _mouseController.PanEnable = true;
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            _mouseController.PanEnable = false;
        }

        public void OnDrag(PointerEventData eventData) {
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _mouseController.ZoomEnable = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            _mouseController.ZoomEnable = false;
        }
    }
}