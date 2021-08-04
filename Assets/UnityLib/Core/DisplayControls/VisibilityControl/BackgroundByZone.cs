using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

    [RequireComponent(typeof(Camera))]
    public class BackgroundByZone : MonoBehaviour {
        public VisibilityManager Manager;
        public List<BackgroundParam> BackgroundParams = new List<BackgroundParam>();

        private Camera _camera;
        private BackgroundParam _defaultBackgroundParam = new BackgroundParam();

        [Serializable]
        public class BackgroundParam {
            public List<VisibilityZone> Zones = new List<VisibilityZone>();
            public Color color;
            public CameraClearFlags clearFlags = CameraClearFlags.Color;
        }

        private void Reset() {
            if (Manager == null) {
                Manager = SceneUtils.FindObjectIfSingle<VisibilityManager>();
            }
        }

        private void Awake() {
            _camera = GetComponent<Camera>();
        }




        void Start() {
            _defaultBackgroundParam.color = _camera.backgroundColor;
            _defaultBackgroundParam.clearFlags = _camera.clearFlags;
            foreach (var backgroundParam in BackgroundParams) {
                var param = backgroundParam;
                UnityAction onShowed = () => { SetBackgroundParam(param); };
                UnityAction onHided = () => { SetBackgroundParam(_defaultBackgroundParam); };
                foreach (var zone in backgroundParam.Zones) {
                    zone.OnShowed.AddListener(onShowed);
                    zone.OnHided.AddListener(onHided);
                }
            }
        }

        private void SetBackgroundParam(BackgroundParam backgroundParam) {
            _camera.backgroundColor = backgroundParam.color;
            _camera.clearFlags = backgroundParam.clearFlags;
        }

    }
}
