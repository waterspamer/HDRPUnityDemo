using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

    public class VisibilityZoneGroup : MonoBehaviour {
        [Tooltip("If true, zone transition will be skipped if switching between zones inside group")]
        public bool SwitchWithoutTransition = false;


        public UnityEvent OnEnterGroup;
        public UnityEvent OnExitGroup;

        private bool _isInside = false;


        private void Start() {
            VisibilityZoneViewer.Instance.ZoneGroupEnter.AddListener(TryEnter);
            VisibilityZoneViewer.Instance.ZoneGroupExit.AddListener(TryExit);
            TryEnter(VisibilityZoneViewer.Instance.ActiveZone);
        }



        private void TryEnter(VisibilityZone zone) {
            if (zone == null) {
                return;
            }
            if (!_isInside && OnEnterGroup != null && zone.Group == this) {
                Debug.Log("Enter group " + gameObject.name, gameObject);
                _isInside = true;
                OnEnterGroup.Invoke();
            }
        }

        private void TryExit(VisibilityZone zone) {
            if (zone == null) {
                return;
            }
            if (_isInside && OnExitGroup != null && zone.Group != this) {
                Debug.Log("Exit group " + gameObject.name, gameObject);
                _isInside = false;
                OnExitGroup.Invoke();
            }
        }

        private void OnDestroy() {
            VisibilityZoneViewer.Instance.ZoneGroupEnter.RemoveListener(TryEnter);
            VisibilityZoneViewer.Instance.ZoneGroupExit.RemoveListener(TryExit);
        }
    }
}
