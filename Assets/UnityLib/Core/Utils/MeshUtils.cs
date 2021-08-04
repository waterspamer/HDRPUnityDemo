using UnityEngine;

public static class MeshUtils {

    public static void SetVerticesColor(this Mesh mesh, params Color[] colors) {
        Color[] meshColors = new Color[mesh.vertices.Length];
        for (var i = 0; i < meshColors.Length; i++) {
            meshColors[i] = i < colors.Length  ? colors[i] : colors[colors.Length -1];
        }
        mesh.colors = meshColors;
    } 

}