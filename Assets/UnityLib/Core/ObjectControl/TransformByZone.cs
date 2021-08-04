using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

    public class TransformByZone : MonoBehaviour {
        [System.Serializable]
        public class TransformOptions {
            public Vector3 Position = new Vector3(0, 0, 0);
            public Vector3 Rotation;
            public Vector3 LocalScale = new Vector3(1, 1, 1);
            public bool PositionIsLocal = false;
            public bool RotationIsLocal = false;
            public VisibilityZone[] Zones;
            public void Apply(Transform transform) {

                if (PositionIsLocal) {
                    transform.localPosition = Position;
                }
                else {
                    transform.position = Position;
                }
                if (RotationIsLocal) {
                    transform.localRotation = Quaternion.Euler(Rotation);
                }
                else {
                    transform.rotation = Quaternion.Euler(Rotation);
                }
                transform.localScale = LocalScale;
            }
        }
        public TransformOptions[] Options;
        private TransformOptions _defaultOptions;
        public VisibilityZoneViewer ZoneViewer;
        private void Reset() {
            ZoneViewer = SceneUtils.FindObjectIfSingle<VisibilityZoneViewer>();
        }

        private void Awake() {
            _defaultOptions = new TransformOptions();

            _defaultOptions.Position = transform.localPosition;
            _defaultOptions.PositionIsLocal = true;
            _defaultOptions.Rotation = transform.localRotation.eulerAngles;
            _defaultOptions.RotationIsLocal = true;
            _defaultOptions.LocalScale = transform.localScale;
            if (ZoneViewer) {
                ZoneViewer.ActiveZoneChanged += OnZoneActiveEventHandler;
            }
        }

        private void OnEnable() {
            if (ZoneViewer) {
                OnZoneActiveEventHandler();
            }


        }
        private void OnDestroy() {
            if (ZoneViewer) {
                ZoneViewer.ActiveZoneChanged -= OnZoneActiveEventHandler;
            }
        }

        private void OnZoneActiveEventHandler() {
            var zone = ZoneViewer.ActiveZone;
            foreach (TransformOptions option in Options) {
                if (CheckByZone(zone,option.Zones)) {
                    option.Apply(transform);
                    return;
                }
            }
            _defaultOptions.Apply(transform);
        }

        private bool CheckByZone(VisibilityZone zone, VisibilityZone[] zonesArray) {
            return zone != null && zonesArray != null &&
               System.Array.Find(zonesArray, z => z != null && z.name == zone.name) != null;
        }

    }
}
