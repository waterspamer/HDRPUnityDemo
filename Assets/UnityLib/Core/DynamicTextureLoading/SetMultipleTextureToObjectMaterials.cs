using System.Linq;
using EasyButtons;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nettle {

    public class SetMultipleTextureToObjectMaterials : MonoBehaviour {
        public enum RemoveType { All, NoTextures, NotAllTextures }
        public bool IncludeWithVisibilityControl = false;
        public RemoveType removeType;
        public TextureToObjectMaterialsSettings[] TexSettings;
        public TextureAtlasInfo AtlasInfo;

        [Button]
        public void Set() {
            Transform[] allTransforms = transform.GetComponentsInChildren<Transform>(true);
            if (TexSettings == null) {
                Debug.LogError("TexSettings == null");
                return;
            }
            foreach (var child in allTransforms) {
                if (child.GetComponent<Renderer>() != null && (IncludeWithVisibilityControl || !child.GetComponent<VisibilityControl>()) && child != transform) {
                    SetTextureToObjectMaterials setTextureToObjectMaterials = child.GetComponent<SetTextureToObjectMaterials>();
                    if (!setTextureToObjectMaterials) {
                        setTextureToObjectMaterials = child.gameObject.AddComponent<SetTextureToObjectMaterials>();
                    }

                    if (!EqualsConfig(setTextureToObjectMaterials)) {
                        setTextureToObjectMaterials.AtlasInfo = AtlasInfo;
                        setTextureToObjectMaterials.TexSettings = new TextureToObjectMaterialsSettings[TexSettings.Length];
                        for (int i = 0; i < setTextureToObjectMaterials.TexSettings.Length; i++) {
                            setTextureToObjectMaterials.TexSettings[i] = new TextureToObjectMaterialsSettings();
                            setTextureToObjectMaterials.TexSettings[i].MaterialPropName = TexSettings[i].MaterialPropName;
                            setTextureToObjectMaterials.TexSettings[i].SearchPath = TexSettings[i].SearchPath;
                            setTextureToObjectMaterials.TexSettings[i].TextureNameFormat = TexSettings[i].TextureNameFormat;
                            setTextureToObjectMaterials.TexSettings[i].ExcludeShaderRegex = TexSettings[i].ExcludeShaderRegex;
                            setTextureToObjectMaterials.TexSettings[i].LoadAtRuntime = TexSettings[i].LoadAtRuntime;
                        }
                        setTextureToObjectMaterials.FindTexturesByName();

                        int texturesFound = 0;
                        for (int i = 0; i < setTextureToObjectMaterials.TexSettings.Length; i++) {
                            if (setTextureToObjectMaterials.TexSettings[i].TextureFound) {
                                texturesFound++;
                            }
                        }
                        if (texturesFound != setTextureToObjectMaterials.TexSettings.Length) {
                            Debug.LogError("Textures not found for " + child.name);
                        }
                    }

                }
            }
#if UNITY_EDITOR
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
        }

        [Button]
        public void Remove() {
            SetTextureToObjectMaterials[] allComponents = transform.GetComponentsInChildren<SetTextureToObjectMaterials>(true);
            foreach (var component in allComponents) {
                switch (removeType) {
                    case RemoveType.All:
                        DestroyImmediate(component);
                        break;
                    case RemoveType.NoTextures:
                        if (component.TexSettings == null || component.TexSettings.Length == 0 || component.TexSettings.FirstOrDefault(v => v.TextureFound) == null) {
                            DestroyImmediate(component);
                        }
                        break;
                    case RemoveType.NotAllTextures:
                        if (component.TexSettings == null || component.TexSettings.Length == 0 ||
                            component.TexSettings.Where(v => v.TextureFound).ToArray().Length < component.TexSettings.Length) {
                            DestroyImmediate(component);
                        }
                        break;
                }
            }
#if UNITY_EDITOR
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
        }

        private bool EqualsConfig(SetTextureToObjectMaterials component) {
            if (component.AtlasInfo != AtlasInfo) {
                return false;
            }

            if (component.TexSettings == null || component.TexSettings.Length != TexSettings.Length) {
                return false;
            }

            for (int i = 0; i < component.TexSettings.Length; i++) {
                if (component.TexSettings[i].MaterialPropName != TexSettings[i].MaterialPropName ||
                    component.TexSettings[i].SearchPath != TexSettings[i].SearchPath ||
                    component.TexSettings[i].TextureNameFormat != TexSettings[i].TextureNameFormat||
                    component.TexSettings[i].LoadAtRuntime!= TexSettings[i].LoadAtRuntime
                    ) {
                    return false;
                }
            }
            return true;
        }

        private void Reset() {
            TexSettings = new TextureToObjectMaterialsSettings[2];

            TexSettings[0] = new TextureToObjectMaterialsSettings();
            TexSettings[0].MaterialPropName = "_AOMap";
            TexSettings[0].TextureNameFormat = "{obj_name}_[]_AOMap";
            TexSettings[0].SearchPath = "Maps/AO";

            TexSettings[1] = new TextureToObjectMaterialsSettings();
            TexSettings[1].MaterialPropName = "_ShadowMap";
            TexSettings[1].TextureNameFormat = "{obj_name}_[]_ShadowMap";
            TexSettings[1].SearchPath = "Maps/Shadows";
            TexSettings[1].ExcludeShaderRegex = "_Blend";
        }
    }
}
