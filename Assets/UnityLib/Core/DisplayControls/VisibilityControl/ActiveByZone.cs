using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace Nettle {
    [ExecuteAfter(typeof(MotionParallaxLOD))]
    public class ActiveByZone : MonoBehaviour {

        public enum ObjectState {
            Inactive = 0,
            Active = 1
        }

        
        public VisibilityZoneViewer ZoneViewer;
        public ObjectState State = ObjectState.Active;
        public string RegularExpression;
        public VisibilityZone[] Zones;
        public string[] ZoneNames;

        private bool _forceEnable = true;

        private bool _isInitialized = false;

        public bool ForceEnable {
            private get {
                return _forceEnable;
            }
            set {
                _forceEnable = true;
                gameObject.SetActive(true);
                _forceEnable = false; ;
            }
        }

        private void Reset() {
            FindZoneViewers();
        }

        private void FindZoneViewers()
        {        
            if (ZoneViewer == null) {
                ZoneViewer = SceneUtils.FindObjectIfSingle<VisibilityZoneViewer>();
            }
        }

        private void OnEnable() {
            if (ZoneViewer && !ForceEnable) {
                OnZoneActiveEventHandler();
            }
            
        }
        private void Start()
        {
            Initialize();
        }

        public void Initialize() {
            if (_isInitialized || !enabled)
            {
                return;
            }
            FindZoneViewers();

            if (ZoneViewer) {
                ZoneViewer.ActiveZoneChanged += OnZoneActiveEventHandler;
            }

            _isInitialized = true;
        }


        private void OnDestroy() {
            if (ZoneViewer) {
                ZoneViewer.ActiveZoneChanged -= OnZoneActiveEventHandler;
            }
        }

        private void OnZoneActiveEventHandler() {
            var zone = ZoneViewer.ActiveZone;
            var show = zone && (CheckByRegex(zone.name) || CheckByZone(zone) || CheckByName(zone.name));
            gameObject.SetActive(State == ObjectState.Active ? show : !show);
        }

        private void OnRemoteZoneActiveEventHandler(string zoneName) {
            var show = CheckByRegex(zoneName) || CheckByName(zoneName);
            gameObject.SetActive(State == ObjectState.Active ? show : !show);
        }

        private bool CheckByRegex(string zoneName) {
            return !String.IsNullOrEmpty(RegularExpression) && zoneName != null &&
                Regex.Match(zoneName, RegularExpression).Success;
        }

        private bool CheckByZone(VisibilityZone zone) {
            return zone != null && Zones != null &&
                Array.Find(Zones, z => z != null && z.name == zone.name) != null;

        }

        private bool CheckByName(string zoneName) {
            return zoneName != null && ZoneNames != null &&
                Array.Find(ZoneNames, z => z == zoneName) != null;
        }
    }
}
