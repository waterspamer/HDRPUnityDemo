using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

    [System.Serializable]
    public class ClickableObjectEvent : UnityEvent<ClickableObject> {

    }
    public class ClickableObject : MonoBehaviour {
        /// <summary>
        /// Called when this object is clicked
        /// </summary>
        public ClickableObjectEvent OnClick {
            get {
                if (_onMouseOver == null) {
                    _onClick = new ClickableObjectEvent();
                }
                return _onClick;
            }
        }
        private ClickableObjectEvent _onClick;
        /// <summary>
        /// Called when mouse moves over this object
        /// </summary>
        public ClickableObjectEvent OnMouseOver {
            get {
                if (_onMouseOver == null) {
                    _onMouseOver = new ClickableObjectEvent();
                }
                return _onMouseOver;
            }
        }
        [SerializeField]
        private ClickableObjectEvent _onMouseOver;
        /// <summary>
        /// Called when mouse leaves this object
        /// </summary>
        public ClickableObjectEvent OnMouseOut {
            get {
                if (_onMouseOut == null) {
                    _onMouseOut = new ClickableObjectEvent();
                }
                return _onMouseOut;
            }
        }
        [SerializeField]
        private ClickableObjectEvent _onMouseOut;
        /// <summary>
        /// Called when user clicks the object and begins to drag it
        /// </summary>
        public ClickableObjectEvent OnDragStart {
            get {
                if (_onDragStart == null) {
                    _onDragStart = new ClickableObjectEvent();
                }
                return _onDragStart;
            }
        }
        [SerializeField]
        private ClickableObjectEvent _onDragStart;
        /// <summary>
        /// Called when user releases the dragged object
        /// </summary>
        public ClickableObjectEvent OnDragEnd {
            get {
                if (_onDragEnd == null) {
                    _onDragEnd = new ClickableObjectEvent();
                }
                return _onDragEnd;
            }
        }
        [SerializeField]
        private ClickableObjectEvent _onDragEnd;
        /// <summary>
        /// Called when user is dragging something over this object
        /// </summary>
        public ClickableObjectEvent OnDragOver {
            get{
                if (_onDragOver == null) {
                    _onDragOver = new ClickableObjectEvent();
                }
                return _onDragOver;
            }
        }
        [SerializeField]
        private ClickableObjectEvent _onDragOver;
        /// <summary>
        /// Called when user  is dragging something away from this object
        /// </summary>
        public ClickableObjectEvent OnDragOut {
            get {
                if (_onDragOut == null) {
                    _onDragOut = new ClickableObjectEvent();
                }
                return _onDragOut;
            }
        }
        [SerializeField]
        private ClickableObjectEvent _onDragOut;

    private Collider[] _colliders;

        private void Awake() {
            if (ClickableObjectsRaycaster.Instance != null) {
                _colliders = GetComponentsInChildren<Collider>(true);
                ClickableObjectsRaycaster.Instance.RegisterObject(this);
            }
        }

        private void OnDestroy() {
            if (ClickableObjectsRaycaster.Instance != null) {
                ClickableObjectsRaycaster.Instance.UnregisterObject(this);
            }
        }

        private void OnEnable() {
            foreach (Collider coll in _colliders) {
                coll.enabled = true;
            }
        }

        private void OnDisable() {
            foreach (Collider coll in _colliders) {
                coll.enabled = false;
            }
        }

        public bool CheckCollider(Collider hitCollider) {
            foreach (Collider coll in _colliders) {
                if (coll == hitCollider) {
                    return true;
                }
            }
            return false;
        }
    }
}
