using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Nettle {

    [ExecuteBefore(typeof(VisibilityZoneViewer))]
    [ExecuteAfter(typeof(Config))]
    public class TransformByDeviceType : MonoBehaviour {
        [System.Serializable]
        public class TransformOptions {            
            public bool Remember; //TODO: replace with button. Bulat say that hacks is bad and TODO is bad too.
            public bool Apply; //TODO: replace with button.  Bulat say that hacks is bad and TODO is bad too.
            public Vector3 Position = new Vector3(0, 0, 0);
            public Vector3 Rotation;
            public Vector3 Scale = new Vector3(1, 1, 1);
            public NettleDeviceType Type;
        }

        [SerializeField]
        private TransformOptions[] _options;
        [SerializeField]
        private Transform[] _transforms;

        private void OnValidate() {
            foreach (TransformOptions options in _options) {
                if (options.Remember) {
                    options.Remember = false;
                    options.Position = transform.localPosition;
                    options.Rotation = transform.localEulerAngles;
                    options.Scale = transform.localScale;
                }
                if (options.Apply) {
                    options.Apply = false;
                    Apply(transform, options);
                }
            }
        }

        private void Awake() {
            TransformOptions options = _options.Where(x => x.Type == VisibilityZoneViewer.DeviceType).FirstOrDefault();
            if (options != null) {
                Apply(transform, options);
                foreach (Transform tr in _transforms) {
                    Apply(tr, options);

                }
            }
        }

        private void Apply(Transform tr, TransformOptions options) {
            tr.localPosition = options.Position;
            tr.rotation = Quaternion.Euler(options.Rotation);
            tr.localScale = options.Scale;
        }
    }
}
