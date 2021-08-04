using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Nettle {

public class GlobalAudio : MonoBehaviour {
    private const float _volumeBaseLevel = 80;
    [SerializeField]
    private AudioMixer _mixer;
    
    public void SetVolume(float volume) {
        if (_mixer != null) {
            _mixer.SetFloat("Volume", -_volumeBaseLevel * Mathf.Pow(Mathf.Clamp01(1-volume),3));
        }
    }
}
}
