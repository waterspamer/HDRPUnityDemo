using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public class ObstructionHiderBroadcast : MonoBehaviour
    {
        private List<ObstructionHider> _hiders;

        private void GetHiders()
        {
            if (_hiders == null)
            {
                _hiders = SceneUtils.FindObjectsOfType<ObstructionHider>(true);
            }
        }

        public bool EnableHiding{
            get
            {
                return _enableHiding;
            }
            set
            {
                _enableHiding = value;
                GetHiders();
                foreach (ObstructionHider hider in _hiders)
                {
                    hider.EnableHiding = _enableHiding;
                }
            }
        }
        [SerializeField]
        private bool _enableHiding = true;

        private void Start()
        {
            EnableHiding = _enableHiding;
        }
    }
}