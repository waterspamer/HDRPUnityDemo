using UnityEngine;
using UnityEditor;

namespace Nettle {

[CanEditMultipleObjects]
[CustomEditor(typeof(SetTextureToObjectMaterials))]
public class SetTextureToObjectMaterialsEditor : Editor {
    public override void OnInspectorGUI() {
        //var textureToObjectMaterials = (SetTextureToObjectMaterials)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("Find textures")) {
            var selectedGM = Selection.transforms;
            foreach (var tr in selectedGM) {
                var textureToObjectMaterials = tr.GetComponent<SetTextureToObjectMaterials>();
                if (textureToObjectMaterials) {
                    textureToObjectMaterials.FindTexturesByName();
                }
            }
        }

        if (GUILayout.Button("Reset texture slots")) {
            var selectedGM = Selection.transforms;
            foreach (var tr in selectedGM) {
                var textureToObjectMaterials = tr.GetComponent<SetTextureToObjectMaterials>();
                if (textureToObjectMaterials) {
                    textureToObjectMaterials.ResetTextureSlots();
                }
            }
        }

      
    }
}
}
