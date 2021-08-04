using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class DynamicInfoLabelsCreator : DynamicInfoCreator {


        [SerializeField]
        private GameObject _flatZonesRoot;

        private VisibilityZone[] _zones;
        private VisibilityZone _currentZone;

        protected override void Start() {
            base.Start();
            _zones = _flatZonesRoot.GetComponentsInChildren<VisibilityZone>();
            VisibilityZoneViewer.Instance.OnShowZone.AddListener(RecreateInfo);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            VisibilityZoneViewer.Instance.OnShowZone.RemoveListener(RecreateInfo);
        }

        protected virtual string FlatNumberFromName(string zoneName) {
            int id = zoneName.LastIndexOf("@");
            if (id < 0) {
                return "";
            } else {
                return zoneName.Substring(id + 1).TrimStart('0');
            }
        }

        public override void RecreateInfo()
        {
            _currentZone = VisibilityZoneViewer.Instance.ActiveZone;
            base.RecreateInfo();
        }

        public void RecreateInfo(VisibilityZone zone) {
            _currentZone = zone;
            base.RecreateInfo();
        }

        public override void CreateInfo()
        {
            foreach (VisibilityZone flatZone in _zones) {
                if (flatZone.VisibilityTag == _currentZone.VisibilityTag) {
                    GuiDatabaseItem item = _info.FindFirst(x => x["id"] == FlatNumberFromName(flatZone.name));
                    if (item != null) {
                        DynamicInfoLabel label = CreateLabel(item, flatZone.transform.TransformPoint(flatZone.VisualCenter), flatZone.transform.rotation);
                        label.Zone = flatZone;
                    }
                }
            }
        }

    }
}