using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControl : MonoBehaviour {

    public float TimeScale = 1f;
    public bool Paused = false;

    public void SetTimeScale(float timeScale) {
        TimeScale = timeScale;
        if (!Paused) {
            Time.timeScale = TimeScale;
        }
    }

    public void Pause() {
        Paused = true;
        Time.timeScale = 0;
    }

    public void Play() {
        Paused = false;
        Time.timeScale = TimeScale;

    }
}
