using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nettle {
    public class ActiveOnCloseView : MonoBehaviour {
        [SerializeField]
        private MotionParallaxDisplay _display;
        public float MinPixelsPerUnit = 1;
        public float ZoneRadius = 5;

        public GameObject[] Objects;

        public UnityEvent OnShow;
        public UnityEvent OnHide;
        [SerializeField]
        private bool _state = true;
        [SerializeField]
        private bool _invertState = false;

        // Use this for initialization
        private void Reset () {
            _display = FindObjectOfType<MotionParallaxDisplay> ();
        }

        // Update is called once per frame
        void Update () {
            if (_display.PixelsPerUnit () >= MinPixelsPerUnit && Vector2.Distance (new Vector2 (transform.position.x, transform.position.z), new Vector2 (_display.transform.position.x, _display.transform.position.z)) < ZoneRadius + _display.transform.localScale.x * _display.Width / 2) {
                if (!_state) {
                    foreach (GameObject go in Objects) {
                        go.SetActive (!_invertState);
                    }
                    _state = true;
                    if (OnShow != null) {
                        OnShow.Invoke ();
                    }
                }
            } else {
                if (_state) {
                    foreach (GameObject go in Objects) {
                        go.SetActive (_invertState);
                    }
                    _state = false;
                    if (OnShow != null) {
                        OnHide.Invoke ();
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos () {
            Handles.color = Color.cyan;
            Handles.DrawWireDisc (transform.position, Vector3.up, ZoneRadius);
            Handles.color = Color.white;
        }
#endif
    }
}