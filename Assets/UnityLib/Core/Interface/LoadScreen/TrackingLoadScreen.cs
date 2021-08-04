using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class TrackingLoadScreen : MonoBehaviour {

        [SerializeField]
        private GameObject _loadScreenObject;
        [SerializeField]
        private float _initialDelay = 0.25f;
        [SerializeField]
        private float _maxDelay = 3;
        private float _timeCountdown = 0;
        private NettleBoxTracking _tracking;
        // Use this for initialization
        void Start() {
#if !UNITY_EDITOR            
            _loadScreenObject.SetActive(true);    
#endif    
            _tracking = StereoEyes.Instance.NettleBoxTracking;
        }

        private void  Update() {
            if (_loadScreenObject.activeSelf) {
                _timeCountdown+=Time.deltaTime;
                if (_timeCountdown>_initialDelay && (_timeCountdown > _maxDelay||_tracking.Active)) {
                    Debug.Log(_timeCountdown);
                    _loadScreenObject.SetActive(false);
                }
            }
        }        
    }

}