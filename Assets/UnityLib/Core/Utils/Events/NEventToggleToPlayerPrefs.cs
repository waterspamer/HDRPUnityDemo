using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nettle
{
    [RequireComponent(typeof(NEventToggle))]
    public class NEventToggleToPlayerPrefs : MonoBehaviour
    {
        [SerializeField]
        private string _prefsKey = "";
        private NEventToggle _toggle;
        // Start is called before the first frame update
        private void Start()
        {
            _toggle = GetComponent<NEventToggle>();
            _toggle.SetEvent.AddListener(OnToggleOn);
            _toggle.ResetEvent.AddListener(OnToggleOff);
            _toggle.SetToggle(PlayerPrefs.GetInt(_prefsKey, _toggle.IsChecked ? 1 : 0) != 0);
        }

        private void OnToggleOn()
        {
            SaveState(true);
        }

        private void OnToggleOff()
        {
            SaveState(false);
        }

        private void SaveState(bool on)
        {
            PlayerPrefs.SetInt(_prefsKey,on?1:0);
        }

        private void OnDestroy()
        {
            if (_toggle != null)
            {
                _toggle.SetEvent.RemoveListener(OnToggleOn);
                _toggle.ResetEvent.RemoveListener(OnToggleOff);
            }
        }
    }
}