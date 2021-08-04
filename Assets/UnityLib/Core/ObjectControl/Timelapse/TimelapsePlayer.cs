using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

namespace Nettle {

[Serializable]
public class PlayerStateEvent : UnityEvent<EPlayerStates> { }

public class TimelapsePlayer : MonoBehaviour {


    public enum UpdateMode {
        EveryFrame,
        CustomFps
    }

    public Timelapse timelapse;
    public UpdateMode updateMode;
    public int customFps = 0;
    public float daysPerSecond = 120;
    public bool loop;
    private EPlayerStates _state;

    public UnityEvent EventPlay;
    public UnityEvent EventPause;
    public UnityEvent EventStop;
    public PlayerStateEvent setPlayerStateEvent;

    private void Start() {
        if(EventPlay == null) {
            EventPlay = new UnityEvent();
        }
        if (EventPause == null) {
            EventPause = new UnityEvent();
        }
        if (EventStop == null) {
            EventStop = new UnityEvent();
        }
    }

    public void Play() {
        if(_state != EPlayerStates.Play) {
            _state = EPlayerStates.Play;
            StartCoroutine("PlayingCoroutine");
            if (setPlayerStateEvent != null) {
                setPlayerStateEvent.Invoke(_state);
            }
            if(EventPlay != null) {
                EventPlay.Invoke();
            }
        }
    }

    public void Pause() {
        if (_state != EPlayerStates.Pause) {
            ExitPlayMode();
            _state = EPlayerStates.Pause;
            if (setPlayerStateEvent != null) {
                setPlayerStateEvent.Invoke(_state);
            }
            if (EventPause != null) {
                EventPause.Invoke();
            }
        }
    }

    public void Stop() {
        ExitPlayMode();
        _state = EPlayerStates.Stop;
        if (timelapse != null) {
            timelapse.SetTime(timelapse.StartDate.ToDateTime());
        }
        if (setPlayerStateEvent != null) {
            setPlayerStateEvent.Invoke(_state);
        }
        if (EventStop != null) {
            EventStop.Invoke();
        }
    }

    public void SetLoop(bool on) {
        loop = on;
    }

    private void ExitPlayMode() {
        if (_state == EPlayerStates.Play) {
            StopCoroutine("PlayingCoroutine");
        }
    }

    private IEnumerator PlayingCoroutine() {
        while(true) {
            float deltaTime = 0.0f;
            if ((updateMode == UpdateMode.EveryFrame) || (customFps <= 0)) {
                yield return new WaitForEndOfFrame();
                deltaTime = Time.deltaTime;
            } else {
                float startTime = Time.time;
                yield return new WaitForSeconds(1.0f / (float)customFps);
                deltaTime = Time.time - startTime;
            }

            if(timelapse != null) {
                timelapse.SetTime(timelapse.CurrenTime + TimeSpan.FromDays(daysPerSecond * deltaTime));

                if(timelapse.EndDate.ToDateTime() == timelapse.CurrenTime) {
                    if (loop) {
                        timelapse.SetTime(timelapse.StartDate.ToDateTime());
                    }else {
                        Pause();
                    }
                }
            }
        }
    }
}
}
