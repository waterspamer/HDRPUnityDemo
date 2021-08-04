using System;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

namespace Nettle.Core {
    public class ComplexCullingManager : MonoBehaviour {



        [NonSerialized] public List<ComplexCulling> ComplexCullings = new List<ComplexCulling>();

        public static ComplexCullingManager Instance {
            get {
                if (!_instance) {
                    _instance = FindObjectOfType<ComplexCullingManager>();
                    if (!_instance) {
                        GameObject go = new GameObject("ComplexCullingManager");
                        _instance = go.AddComponent<ComplexCullingManager>();
                    }
                }
                return _instance;
            }
        }

        private static ComplexCullingManager _instance;

        private int _currentId;

        public bool CullingEnabled = true;
        public float CullingCheckInterval = 0.5f;
        public bool DontDestroy = true;

        void Awake() {
            if (DontDestroy) {
                DontDestroyOnLoad(gameObject);
            }
        }

        void Update() {
            if (!CullingEnabled || ComplexCullings.Count == 0) {
                return;
            }

            int idsForCheck = Mathf.CeilToInt(Mathf.Clamp((ComplexCullings.Count / CullingCheckInterval) * Time.smoothDeltaTime, 0, ComplexCullings.Count));
            while (idsForCheck > 0) {
                idsForCheck--;
                _currentId = (_currentId + 1) % ComplexCullings.Count;
                ComplexCulling animatorCulling = ComplexCullings[_currentId];
                if (animatorCulling != null && animatorCulling.gameObject.activeInHierarchy) {
                    animatorCulling.UpdateCulling();
                }
            }
        }


        [Button]
        public void EnableCullingBtn() {
            EnableCulling(true);
        }

        [Button]
        public void DisableCullingBtn() {
            EnableCulling(false);
        }


        public void EnableCulling(bool enable) {
            CullingEnabled = enable;
            if (CullingEnabled) {
                foreach (var complexCulling in ComplexCullings) {
                    complexCulling.EnableAll();
                }
            }
        }

        public void AddComplexCulling(ComplexCulling animatorCulling) {
            ComplexCullings.Add(animatorCulling);
        }

        public void RemoveComplexCulling(ComplexCulling animatorCulling) {
            ComplexCullings.Remove(animatorCulling);
        }
    }
}