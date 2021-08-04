using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nettle.Core {

    public class ActiveByDeviceType : MonoBehaviour {
        [System.Serializable]
        public class ActiveOptions {
            public bool Active;
            public NettleDeviceType Type;
        }

        [SerializeField]
        private ActiveOptions[] _options;

        private void Awake() {
            ActiveOptions options = _options.Where(x => x.Type == VisibilityZoneViewer.DeviceType).FirstOrDefault();
            if (options != null) {
                gameObject.SetActive(options.Active);
            }
        }
    }


   

}