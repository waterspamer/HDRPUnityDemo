using System.Collections;
using System.Linq;
using UnityEngine;

namespace Nettle {

public class AudioCollider : MonoBehaviour {

    public float Height = 20f;
    public float AttenuationSpeed = 0.5f;
    private AudioSource[] AudioSources;
    private float[] DefaultVolumes;
    private Coroutine FadeCoroutine;
    private bool _started;


    void Start() {
        AudioSources = GetComponents<AudioSource>();
        DefaultVolumes = AudioSources.Select(v => v.volume).ToArray();
        foreach (var audioSource in AudioSources) {
            audioSource.volume = 0;
        }
    }

    public void OnEnable() {
        FadeCoroutine = null;
    }

    public void Play() {
        if (FadeCoroutine != null) {
            StopCoroutine(FadeCoroutine);
        }
        _started = true;
        FadeCoroutine = StartCoroutine(FadeIn());
        
    }

    public void Stop() {
        if (FadeCoroutine != null) {
            StopCoroutine(FadeCoroutine);
        }
        _started = false;
        FadeCoroutine = StartCoroutine(FadeOut());
        
    }

    public void OnTriggerStay(Collider collider) {
        if (Height > collider.transform.position.y - transform.position.y && !AudioColliderListener.Muted) {
            if (!_started) {
                Play();
            }
        } else {
            if (_started) {
                Stop();
            }
        }
    }

    public void OnTriggerExit(Collider collider) {
        Stop();
    }

    IEnumerator FadeIn() {
        foreach (var audioSource in AudioSources) {
            audioSource.Play();
        }
        bool completed = false;
        while (!completed) {
            for (int i = 0; i < AudioSources.Length; i++) {
                AudioSources[i].volume += Mathf.Lerp(0, DefaultVolumes[i], AttenuationSpeed * Time.deltaTime);
                if (AudioSources[i].volume >= DefaultVolumes[i]) {
                    AudioSources[i].volume = DefaultVolumes[i];
                    completed = true;
                }
            }
            yield return null;
        }
    }


    IEnumerator FadeOut() {
        bool completed = false;
        while (!completed) {
            for (int i = 0; i < AudioSources.Length; i++) {
                AudioSources[i].volume -= Mathf.Lerp(0, DefaultVolumes[i], AttenuationSpeed * Time.deltaTime);
                if (AudioSources[i].volume <= 0) {
                    AudioSources[i].volume = 0;
                    completed = true;
                }
            }
            yield return null;
        }
        foreach (var audioSource in AudioSources) {
            audioSource.Stop();
        }
    }
}
}
