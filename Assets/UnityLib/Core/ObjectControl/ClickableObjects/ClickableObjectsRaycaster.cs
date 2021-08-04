using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

    [RequireComponent(typeof(Camera))]
    public class ClickableObjectsRaycaster : MonoBehaviour, IZoomPanBlocker {
        public LayerMask RaycastMask = ~0;
        public float MaxClickTime = 0.2f;
        public float MaxPositionDelta = 10;
        public ClickableObjectEvent OnObjectClicked;


        private List<ClickableObject> _objects = new List<ClickableObject>();
        private Camera _camera;
        private float _buttonDownTime;
        private Vector2 _buttonDownPosition;
        private float _maxDelta;
        private ClickableObject _draggedObject = null;
        private ClickableObject _clickedObject = null;
        private ClickableObject _objectUnderMouse = null;

        private static ClickableObjectsRaycaster _instance;
        public static ClickableObjectsRaycaster Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<ClickableObjectsRaycaster>();
                }
                return _instance;
            }
        }

        private void Awake() {
            _camera = GetComponent<Camera>();
        }

        private void Start() {
            ZoomPan zoomPan = FindObjectOfType<ZoomPan>();
            if (zoomPan != null) {
                zoomPan.Blockers.Add(this);
            }
        }

        public void RegisterObject(ClickableObject obj) {
            if (!_objects.Contains(obj)) {
                _objects.Add(obj);
            }
        }

        public void UnregisterObject(ClickableObject obj) {
            _objects.Remove(obj);
        }

        private void Update() {

            ClickableObject newObjectUnderMouse = null;

            Vector2 mousePos = Input.mousePosition;
            //If mouse is in the lower eye rect, move it to upper
            if (mousePos.y < _camera.rect.y * Screen.height) {
                mousePos.y += _camera.rect.y * Screen.height;
            }
            Ray ray = _camera.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, RaycastMask)) {
                foreach (ClickableObject obj in _objects) {
                    if (obj.CheckCollider(hit.collider)) {
                        newObjectUnderMouse = obj;
                        break;
                    }
                }
            }

            //If the pointer has left an object, call its related events
            if (newObjectUnderMouse != _objectUnderMouse) {
                if (_objectUnderMouse != null) {
                    if (_objectUnderMouse.OnMouseOut != null) {
                        _objectUnderMouse.OnMouseOut.Invoke(_objectUnderMouse);
                    }
                    
                    if (Input.GetMouseButton(0)) {
                        if (_objectUnderMouse.OnDragOut != null) {
                            _objectUnderMouse.OnDragOut.Invoke(_objectUnderMouse);
                        }
                    }
                }
                _objectUnderMouse = newObjectUnderMouse;
                if (_objectUnderMouse != null) {
                    if (_objectUnderMouse.OnMouseOver != null) {
                        _objectUnderMouse.OnMouseOver.Invoke(_objectUnderMouse);
                    }
                    if (Input.GetMouseButton(0)) {
                        if (_objectUnderMouse.OnDragOver != null) {
                            _objectUnderMouse.OnDragOver.Invoke(_objectUnderMouse);
                        }
                    }
                }
            }


            if (Input.GetMouseButtonUp(0)) {
                if (_draggedObject != null) {
                    if (_draggedObject.OnDragEnd != null) {
                        _draggedObject.OnDragEnd.Invoke(_draggedObject);
                    }
                    _draggedObject = null;
                }
                _clickedObject = null;
            }

            if (_objectUnderMouse == null) {
                return;
            }
            

            if (Input.GetMouseButtonDown(0)) {
                _buttonDownTime = Time.unscaledTime;
                _buttonDownPosition = Input.mousePosition;
                _maxDelta = 0;
                _clickedObject = _objectUnderMouse;
            }

            if (Input.GetMouseButton(0)) {
                float delta = Vector2.Distance(_buttonDownPosition, Input.mousePosition);
                if (_draggedObject==null && _objectUnderMouse == _clickedObject && delta >= MaxPositionDelta) {
                    _draggedObject = _objectUnderMouse;
                    if (_objectUnderMouse.OnDragStart != null) {
                        _objectUnderMouse.OnDragStart.Invoke(_objectUnderMouse);
                    }
                }
                if (delta > _maxDelta) {
                    _maxDelta = delta;
                }
            }
            if (Input.GetMouseButtonUp(0)) {
                if (MaxClickTime >= Time.unscaledTime - _buttonDownTime || MaxPositionDelta > _maxDelta) {
                    if (_objectUnderMouse.OnClick != null) {
                        _objectUnderMouse.OnClick.Invoke(_objectUnderMouse);
                    }
                    if (OnObjectClicked != null) {
                        OnObjectClicked.Invoke(_objectUnderMouse);
                    }
                }
            }
        }

        public bool IsPanBlocked() {
            return _clickedObject != null ;
        }
        public bool IsZoomBlocked() {
            return false;
        }
    }
}
