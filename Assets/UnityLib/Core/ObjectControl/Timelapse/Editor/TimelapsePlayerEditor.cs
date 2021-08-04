using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Nettle {

[CustomEditor(typeof(TimelapsePlayer))]
public class TimelapsePlayerEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var player = target as TimelapsePlayer;

        if (Application.isPlaying && player != null) {
         
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Play")) {
                player.Play();
            }

            if (GUILayout.Button("Pause")) {
                player.Pause();
            }

            if (GUILayout.Button("Stop")) {
                player.Stop();
            }
            GUILayout.EndHorizontal();
           
        }
       
    }
}
}
