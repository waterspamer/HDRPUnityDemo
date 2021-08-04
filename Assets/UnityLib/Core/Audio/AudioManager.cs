using UnityEngine;
using System.Linq;
using UnityEngine.Events;

namespace Nettle {

[System.Serializable]
public class AudioInfo {
    public string Key;
    public LocalizedAudio[] Audio;
}

[System.Serializable]
public class LocalizedAudio {
    public Language Language;
    public AudioClip Audio;
    public float Volume = 1;
}
[System.Serializable]
public class VolumeControl {
    public Language Language;
    [Range(0, 1.0f)]
    public float Volume = 1;
}
public class AudioManager : MonoBehaviour {
    public AudioSource Source;
    public bool IsActive = true;
    public AudioInfo[] Audio;
    [Tooltip("If true, the audio clip currently playing will always be stopped when PlayAudio is called, even if the new clip can't be found")]
    public bool AlwaysStopPrevious = true;
    [Tooltip("If true, the same audio clip can be played many times in a row")]
    public bool CanBeRepated = true;
    public bool ReplayOnChangeLanguage = true;
    public bool OnClipCompleteEnabled = true;
    public UnityEvent OnClipComplete;


    [SerializeField]
    private Language _language;
    [SerializeField]
    private VolumeControl[] _volume;
    private string currentAudioKey = "";
    private bool _isPlaying = false;


    void Update() {
        if (_isPlaying && !Source.isPlaying && Source.time == 0) {
            Stop();
            if (OnClipCompleteEnabled) {
                OnClipComplete.Invoke();
            }
        }
    }

    public void SetLanguage(Language language) {
        _language = language;
        if (Source.isPlaying && ReplayOnChangeLanguage) {
            PlayAudio(currentAudioKey);
        }
    }

    public void SetActive(bool active) {
        IsActive = active;
        if (!IsActive && Source.isPlaying) {
            Stop();
        }
    }

    public void PlayAudio(VisibilityZone zone) {
        PlayAudio(zone.gameObject.name);
    }

    public void PlayAudio(string key) {
        PlayAudio(key, false);
    }

    public void ToogleAutoPlayList() {
        OnClipCompleteEnabled = !OnClipCompleteEnabled;
        if (OnClipCompleteEnabled) {
            OnClipComplete.Invoke();
        }
    }

    public void PlayAudio(string key, bool playWhenInactive) {
        if (!IsActive && !playWhenInactive) {
            return;
        }
        if (currentAudioKey != key || CanBeRepated) {
            currentAudioKey = key;
            if (Source != null) {
                AudioInfo audio = Audio.Where(x => x != null && x.Key == key).FirstOrDefault();
                LocalizedAudio localizedAudio = null;
                if (audio != null) {
                    localizedAudio = audio.Audio.Where(x => x != null && x.Language == _language).FirstOrDefault();
                }
                if (audio != null && localizedAudio != null) {
                    if (Source.clip != localizedAudio.Audio || !Source.isPlaying) {
                        VolumeControl volumeControl = _volume.Where(x => x.Language == _language).FirstOrDefault();
                        float globalVolume = 1;
                        if (volumeControl != null) {
                            globalVolume = volumeControl.Volume;
                        }
                        Source.clip = localizedAudio.Audio;
                        Source.volume = localizedAudio.Volume * globalVolume;
                        Source.Play();
                        _isPlaying = true;
                    }
                } else if (AlwaysStopPrevious) {
                    if (Source.isPlaying) {
                        Stop();
                    }
                }
            }
        }
    }

    public void Stop() {
        Source.Stop();
        _isPlaying = false;
    }
}
}
