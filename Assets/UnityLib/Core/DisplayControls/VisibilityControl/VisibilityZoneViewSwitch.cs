using UnityEngine;
using System;
using System.Collections.Generic;

namespace Nettle {
    [Serializable]
    public class VisibilityZoneTrigger {
        public VisibilityZone Zone;
        public KeyCode Key;
        public VisibilityZoneTrigger(VisibilityZone zone) {
            Zone = zone;
        }
    }

    public class VisibilityZoneViewSwitch : MonoBehaviour {
        public VisibilityZoneViewer Viewer;
        [HideInInspector]
        public List<VisibilityZoneTrigger> ZoneTriggers;
        public bool UseTriggers = true;

        [SerializeField] private bool isCircular = true;

        private int _curID;

//void OnValidate(){
    //foreach(var a in ZoneTriggers){
    //    Debug.Log(a.Zone.name);
//    }
//}
        void Reset() {
            SceneUtils.FindObjectIfSingle(ref Viewer);
        }

        private void Awake() {
            if (Viewer == null) {
                SceneUtils.FindObjectIfSingle(ref Viewer);
            }
        }

        void Update() {
            ProcessInput();
        }

        private int GetCurrentZoneID() {
            return ZoneTriggers.FindIndex(v => v.Zone == Viewer.ActiveZone);
        }

        public void ResetCurrentID(){
            _curID = 0;
        }

        public void SwitchToPrevZone() {
            Debug.Log(ZoneTriggers.Count);
            if (ZoneTriggers.Count == 0 || (!isCircular&&_curID==0)) {
                return;
            }
            var currentID = GetCurrentZoneID();
            _curID = (_curID != 0) ? --_curID : ZoneTriggers.Count - 1;
            Viewer.ShowZone(ZoneTriggers[_curID].Zone.name);
        }

        public void SwitchToNextZone() {
            Debug.Log(ZoneTriggers.Count);
            if (ZoneTriggers.Count == 0 || (!isCircular&&_curID==ZoneTriggers.Count-1)) {
                return;
            }
            var currentID = GetCurrentZoneID();
            _curID = (_curID + 1) % ZoneTriggers.Count;
            Viewer.ShowZone(ZoneTriggers[_curID].Zone.name);
        }

        public void RA_OnEvent(int i){
            if(i<0||i>=ZoneTriggers.Count)return;
            _curID = i;
            Viewer.ShowZone(ZoneTriggers[i].Zone.name);
        }

        private void ProcessInput() {
            if (UseTriggers) {
                for (int i = 0; i < ZoneTriggers.Count; ++i) {
                    if (Input.GetKeyDown(ZoneTriggers[i].Key)) {
                        Viewer.ShowZone(ZoneTriggers[i].Zone.name);
                        _curID = i;
                        break;
                    }
                }
            }
        }
    }
}
