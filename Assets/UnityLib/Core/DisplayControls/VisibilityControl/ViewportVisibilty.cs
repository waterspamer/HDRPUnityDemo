using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace Nettle {

    public class ViewportVisibilty : MonoBehaviour {

        public delegate void VisibiltyCallback();

        private const float TICK = 0.001f;
        private const float DEFAULT_INTERVAL = 0.2f;
        private const float INTERVAL_SPREAD = 0.15f;

        public bool IsVisible = false;
        public Camera Camera;
        public Transform TargetTransform;

        private Action _becameVisibleEvent;
        private Action _becameInvisibleEvent;

        public float Interval = DEFAULT_INTERVAL;

        private WaitForSeconds _waitForSeconds;

        public static ViewportVisibilty Constructor(GameObject parentGameObject,
            Action becameVisibileCallback = null, Action becameInvisibileCallback = null) {
            return Constructor(parentGameObject, DEFAULT_INTERVAL, becameVisibileCallback, becameInvisibileCallback);
        }

        public static ViewportVisibilty Constructor(GameObject parentGameObject, float interval = DEFAULT_INTERVAL,
            Action becameVisibileCallback = null, Action becameInvisibileCallback = null) {
            // Debug.Log("ViewportVisibilty::Constructor::Start");
            ViewportVisibilty viewportVisibilty = parentGameObject.AddComponent<ViewportVisibilty>();
            //Debug.Log("ViewportVisibilty::Constructor::AfterInstance");
            viewportVisibilty.Interval = interval;
            if (becameVisibileCallback != null) {
                viewportVisibilty._becameVisibleEvent += becameVisibileCallback;
            }
            if (becameInvisibileCallback != null) {
                viewportVisibilty._becameInvisibleEvent += becameInvisibileCallback;
            }

            //Debug.Log("ViewportVisibilty::Constructor::BeforeEnable");
            viewportVisibilty.enabled = true;
            // Debug.Log("ViewportVisibilty::Constructor::End");
            return viewportVisibilty;
        }

        void Reset() {
            EditorInit();
        }

        void OnValidate() {
            EditorInit();
        }

        void EditorInit() {
            if (!TargetTransform) {
                TargetTransform = transform;
            }
        }

        void Start() {
            // Debug.Log("ViewportVisibilty::Start");
            IsVisible = Visible();
            if (IsVisible) {
                _becameVisibleEvent.Invoke();
            } else {
                _becameInvisibleEvent.Invoke();
            }
        }

        private static float _currentShift = 0f;

        void OnEnable() {
            //Debug.Log("ViewportVisibilty::OnEnable");
            _waitForSeconds = new WaitForSeconds(Interval + INTERVAL_SPREAD * Random.Range(-1f, 1f));
            StartCoroutine(CheckVisibilityCoroutine());
        }


        void OnDisable() {
            //  Debug.Log("ViewportVisibilty::OnDisable");
            if (IsVisible) {
                IsVisible = false;
                _becameInvisibleEvent.Invoke();
            }
        }

        IEnumerator CheckVisibilityCoroutine() {
            // Debug.Log("ViewportVisibilty::CheckVisibilityCoroutine");
            yield return null;
            while (true) {
                Profiler.BeginSample("WorldToTest::Screen");
                if (Visible()) {
                    if (!IsVisible) {
                        IsVisible = true;
                        _becameVisibleEvent.Invoke();
                    }
                } else if (IsVisible) {
                    IsVisible = false;
                    _becameInvisibleEvent.Invoke();
                }
                Profiler.EndSample();
                yield return _waitForSeconds;
            }
        }

        private bool Visible() {
            if (Camera.main) {
                Vector3 positionOnViewport = Camera.main.WorldToViewportPoint(transform.position);
                return positionOnViewport.x > -0.05 && positionOnViewport.x < 1.05 && positionOnViewport.y > -0.05 && positionOnViewport.y < 1.05;
            } else {
                return false;
            }
        }
    }
}
