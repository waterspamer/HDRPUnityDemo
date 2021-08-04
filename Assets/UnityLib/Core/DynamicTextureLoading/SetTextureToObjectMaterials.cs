using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EasyButtons;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nettle {


    [Serializable]
    public class TextureToObjectMaterialsSettings {

        public Texture2D Texture;
        public string StreamingPath;
        public string MaterialPropName;
        public string TextureNameFormat;
        public string SearchPath;
        public string ExcludeShaderRegex;
        public bool LoadAtRuntime = false;

        public string FullStreamingPath {
            get {
                if (!string.IsNullOrEmpty(StreamingPath)) {
                    return Path.Combine(Application.streamingAssetsPath, StreamingPath);
                }
                else {
                    return "";
                }
            }
        }

        public bool TextureFound {
            get {
                if (LoadAtRuntime) {
                    return File.Exists(FullStreamingPath);
                }
                else {
                    return Texture != null;
                }
            }
        }

        public TextureToObjectMaterialsSettings() {
            Texture = null;
            MaterialPropName = "_MainTex";
            TextureNameFormat = "{obj_name}_Texture";
            SearchPath = "";
            ExcludeShaderRegex = "";
        }

        //public TextureToObjectMaterialsSettings(Texture2D texture, string materialPropName, string textureNameFormat) {
        //    Texture = texture;
        //    MaterialPropName = materialPropName;
        //    TextureNameFormat = textureNameFormat;
        //}


    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    public class SetTextureToObjectMaterials : MonoBehaviour {

        public TextureToObjectMaterialsSettings[] TexSettings;
        public TextureAtlasInfo AtlasInfo;

        private const string _objectNamePlaceholder = @"{obj_name}";

        public void FindTexturesByName() {
#if UNITY_EDITOR
            foreach (var texSetting in TexSettings) {
                if (!texSetting.TextureFound) {
                    string objName = gameObject.name;
                    if (objName.Contains("_MeshPart")) {
                        int id = objName.IndexOf("_MeshPart", StringComparison.InvariantCulture);
                        objName = objName.Remove(id);
                    }
                    string texName = texSetting.TextureNameFormat.Replace(_objectNamePlaceholder, objName);

                    Debug.Log(texName);
                    if (AtlasInfo == null) {
                        if (!texSetting.LoadAtRuntime) {
                            var searchPathes = new string[] { "Assets/" + texSetting.SearchPath };
                            var matchingGuids = AssetDatabase.FindAssets(texName, searchPathes);
                            if (matchingGuids.Length > 0) {
                                foreach (var matchingGuid in matchingGuids) {
                                    string texPath = AssetDatabase.GUIDToAssetPath(matchingGuid);
                                    var fileName = Path.GetFileNameWithoutExtension(texPath);
                                    if (fileName == texName) {
                                        texSetting.Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                                        break;
                                    }
                                }
                            }
                        }
                        //end if !LoadAtRuntime
                        else {
                            texSetting.Texture = null;
                            //TODO: Add support for other formats
                            texSetting.StreamingPath = Path.Combine(texSetting.SearchPath,texName + ".tga");
                        }
                    }//end if AtlasInfo == null
                    else {
                        foreach (TextureAtlasInfo.Atlas atlas in AtlasInfo.Atlases) {
                            TextureAtlasInfo.AtlasedObject obj = null;
                            foreach (TextureAtlasInfo.AtlasedObject ao in atlas.Objects) {
                                if (ao.ObjectName == objName) {
                                    obj = ao;
                                    break;
                                }
                            }
                            if (obj != null) {
                                texSetting.Texture = atlas.Texture;
                                break;
                            }
                        }
                    }
                }
            }
#endif
        }

        public void ResetTextureSlots() {
            if (TexSettings == null) { return; }

            foreach (var texSetting in TexSettings) {
                if (texSetting != null) {
                    texSetting.Texture = null;
                }
                texSetting.StreamingPath = "";
            }
        }

        private void ApplyTextures() {
            if (TexSettings == null || TexSettings.Length == 0) {
                Debug.LogWarningFormat("SetTextureToObjectMaterials: no textures on {0}", gameObject.name);
                return;
            }

            var rend = GetComponent<Renderer>();
            if (rend == null) {
                Debug.LogWarningFormat("SetTextureToObjectMaterials: no renderer on {0}", gameObject.name);
                return;
            }

            Material[] materials;

            if (AtlasInfo == null) {
                materials = rend.materials;
            } else {
                materials = new Material[rend.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++) {
                    materials[i] = AtlasedMaterialInstancer.Instance.GetMaterial(rend.sharedMaterials[i], TexSettings[0].Texture);
                }
                rend.sharedMaterials = materials;
            }

            foreach (var texSetting in TexSettings) {
                if (!texSetting.TextureFound || texSetting.MaterialPropName == "") {
                    Debug.LogWarningFormat("SetTextureToObjectMaterials: invalid texture settings on {0}", gameObject.name);
                    continue;
                }
                Texture2D tex;
                if (texSetting.LoadAtRuntime) {
                    //TODO: Add support for other formats
                    iRuntimeTextureLoader loader = new Alpha8TGALoader();
                    tex = loader.Load(texSetting.FullStreamingPath);
                }
                else {
                    tex = texSetting.Texture;
                }

                foreach (var material in materials) {
                    if (material != null && material.HasProperty(texSetting.MaterialPropName)
                        && (String.IsNullOrEmpty(texSetting.ExcludeShaderRegex) || !Regex.IsMatch(material.shader.name, texSetting.ExcludeShaderRegex))) {
                        //Debug.Log(texSetting.MaterialPropName + " loaded successfully for " + gameObject.name);
                        material.SetTexture(texSetting.MaterialPropName, tex);
                    }
                }
            }
        }

        private void Start() {
            ApplyTextures();
        }


    }
}
