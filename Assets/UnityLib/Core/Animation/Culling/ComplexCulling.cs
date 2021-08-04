using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Nettle.Core {
    [ExecuteAfter(typeof(ComplexCullingManager))]
    public class ComplexCulling : MonoBehaviour {

        public float DisableDistance = 150f;

        public bool EnableAnimatorCulling = true;
        public List<MonoBehaviour> CullMonoBehaviours = new List<MonoBehaviour>();
        private Animator[] _animators;

        private int _totalRenderers = 0;
        private Transform _distanceAnchor;

        private bool _isQuitting = false;

        private void OnBecameVisible() {
            _totalRenderers++;
        }

        private void OnBecameInvisible() {
            _totalRenderers--;
        }

        void OnEnable() {
            UpdateCullingAnimators();
            ComplexCullingManager.Instance.AddComplexCulling(this);
        }

        void OnApplicationQuit() {
            _isQuitting = true;
        }

        void OnDisable() {
            if (_isQuitting) {
                return;
            }
#if UNITY_EDITOR
            if (ComplexCullingManager.Instance == null) {
                return;
            }
#endif
            ComplexCullingManager.Instance.RemoveComplexCulling(this);
        }

        public void UpdateCullingAnimators() {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            _distanceAnchor = renderers[0].transform;
            foreach (var renderer in renderers) {
                RendererVisibilityDispatcher dispatcher = renderer.gameObject.AddComponent<RendererVisibilityDispatcher>();
                dispatcher.BecameVisible += OnBecameVisible;
                dispatcher.BecameInvisible += OnBecameInvisible;
            }
            _animators = GetComponentsInChildren<Animator>();
        }

        public void UpdateCulling() {
            Profiler.BeginSample("ComplexCulling.CullingUpdate");
            if (IsInsideView()) {
                EnableAll();
            } else {
                DisableAll();
            }
            Profiler.EndSample();
        }

        public void EnableAll() {
            foreach (var animator in _animators) {
                if (!animator.enabled) {
                    animator.enabled = true;
                    var tmpCullingMode = animator.cullingMode; //Unity issue workaround. Animater often don't play after enabling
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.cullingMode = tmpCullingMode;
                }
            }
            foreach (var behaviour in CullMonoBehaviours) {
                behaviour.enabled = true;
            }
        }

        private void DisableAll() {
            foreach (var animator in _animators) {
                if (animator.enabled) {
                    animator.enabled = false;
                }
            }
            foreach (var behaviour in CullMonoBehaviours) {
                behaviour.enabled = false;
            }
        }

        private bool IsInsideView() {
            return _totalRenderers > 0 && Vector3.SqrMagnitude(_distanceAnchor.position - Camera.main.transform.position) <
                   DisableDistance * DisableDistance;
        }

        /*private bool IsInsideView() {
            Vector3 positionOnViewport = Camera.main.WorldToViewportPoint(transform.position);
            return positionOnViewport.x > -0.05 && positionOnViewport.x < 1.05 && positionOnViewport.y > -0.05 && positionOnViewport.y < 1.05;
        }*/


    }
}
