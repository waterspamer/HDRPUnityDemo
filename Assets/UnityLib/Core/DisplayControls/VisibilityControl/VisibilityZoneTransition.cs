using System;
using UnityEngine;

namespace Nettle {

    [Serializable]
    public class VisibilityZoneTransition {
        public bool affectTransform = true;
        [ConditionalHide("affectTransform")]
        [SerializeField]
        private bool _fastSwitch = false;


        public bool FastSwitch {
            get => affectTransform && _fastSwitch;
        }
    }
}