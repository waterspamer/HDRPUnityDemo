using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Nettle {

public class TimelapseControllerWindow : EditorWindow {

    public Timelapse Target;

    public static void Init(Timelapse timelapse) {
        // Get existing open window or if none, make a new one:
        var window = (TimelapseControllerWindow)GetWindow(typeof(TimelapseControllerWindow));
        window.Target = timelapse;
        if (timelapse != null) {
            timelapse.Init();
        }
        window.Show();
    }

    private void OnGUI() {
        GUILayout.Label("Timelapse");

        if (Target != null) {
            long newTime = (long)GUILayout.HorizontalSlider(Target.CurrenTime.Ticks, Target.StartDate.ToDateTime().Ticks, Target.EndDate.ToDateTime().Ticks);
            if (Target.CurrenTime.Ticks != newTime) {
                Target.SetTime(new DateTime(newTime));
            }
        } else {
            if (GUILayout.Button("Find timelapse")) {
                Target = GameObject.FindObjectOfType<Timelapse>();
            }

        }
    }
}
}
