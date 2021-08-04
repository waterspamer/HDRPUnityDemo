using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class ActiveByNEventToggleState : MonoBehaviour {
        [SerializeField]
        private NEventToggle _toggle;
        [SerializeField]
        private bool _invertState = false;

        private void Awake() {
            _toggle.SetEvent.AddListener(SetOn);
            _toggle.ResetEvent.AddListener(SetOff);
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (enabled)
            {
                gameObject.SetActive(_toggle.IsChecked != _invertState);
            }
        }

        private void OnDestroy() {
            _toggle.SetEvent.RemoveListener(SetOn);
            _toggle.ResetEvent.RemoveListener(SetOff);
        }

        private void SetOn() {
            if (enabled)
            {
                gameObject.SetActive(!_invertState);
            }
        }

        private void SetOff()
        {
            if (enabled)
            {
                gameObject.SetActive(_invertState);
            }
        }
    }
}
