using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class ScreenSaverData : ScriptableObject {
        public NettleBoxTracking NettleBoxTracking;
        [SerializeField]
        [ConfigField]
        private float _idleTime;
        [SerializeField]
        private bool _isInScreenSaver;
        [SerializeField]
        private bool _canWakeUpByTracking;

        public float ElapsedTime {get; set;}

        public float IdleTime {
            get {
                return _idleTime;
            }

            set {
                _idleTime = value;
            }
        }

        public bool IsInScreenSaver {
            get {
                return _isInScreenSaver;
            }

            set {
                _isInScreenSaver = value;
            }
        }

        public bool CanWakeUpByTracking {
            get {
                return _canWakeUpByTracking;
            }

            set {
                _canWakeUpByTracking = value;
            }
        }

        private void Awake() {
            IsInScreenSaver = false;
        }
    }
}
