using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {
    public class ObjectSetSwitcher : MonoBehaviour {
        [System.Serializable]
        public class ObjectSetSwitcherEvent : UnityEvent<int> {
        }

        [SerializeField]
        private int _currentActiveSet = 0;
        [SerializeField]
        private ObjectSet[] _sets;

        public ObjectSetSwitcherEvent OnSwitch;

        public int CurrentActiveSet {
            get {
                return _currentActiveSet;
            }
            set {
                _currentActiveSet = value;
                if (_currentActiveSet < 0) {
                    _currentActiveSet = 0;
                }
                else if (_currentActiveSet >= _sets.Length) {
                    _currentActiveSet = _sets.Length - 1;
                }
                UpdateSets();
            }
        }

        public void NextSet() {
            _currentActiveSet++;
            if (_currentActiveSet >= _sets.Length) {
                _currentActiveSet = 0;
            }
            UpdateSets();
        }

        public void PreviousSet() {
            _currentActiveSet--;
            if (_currentActiveSet < 0) {
                _currentActiveSet = _sets.Length - 1;
            }
            UpdateSets();
        }

        private void UpdateSets() {
            if (_currentActiveSet >= 0 && _currentActiveSet < _sets.Length) {
                OnSwitch.Invoke(_currentActiveSet);
                foreach (ObjectSet set in _sets) {
                    set.gameObject.SetActive(false);
                }
                _sets[_currentActiveSet].gameObject.SetActive(true);
            }
        }
        private void Start() {
            CurrentActiveSet = _currentActiveSet;
        }
    }
}