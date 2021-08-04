using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Nettle {


    public class ConfigureMaterials {

        private const string EXTERNAL_TEXTURES_VARIABLE = "UnityExternalTextures";
        private const string LOCAL_TEXTURES_PATH = @"Assets\Textures";
        private const string PROTOTYPE_SHADER = "Hidden/ConfigureMaterialPrototype";

        [MenuItem("CONTEXT/Material/ConfigureMaterial", false, 10001)]
        private static void ConfigureMaterial(MenuCommand command) {
            Material material = (Material)command.context;
            AssignTexturesByName(material);
        }

        [MenuItem("Assets/Configure Materials")]
        private static void AssignTexturesByName() {
            AssignTexturesByName(GetSelectedMaterials());
        }

        private static void AssignTexturesByName(params Material[] selectedMaterials) {

            var textures = AssetDatabase.FindAssets("t:Texture2D").
                Select(v => AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(v)));
            var texture2Ds = textures as Texture2D[] ?? textures.ToArray();

            /*string externalPath = Environment.GetEnvironmentVariable(EXTERNAL_TEXTURES_VARIABLE);
            if (string.IsNullOrEmpty(externalPath) || !Directory.Exists(externalPath)) {
                externalPath = SelectTexturesFolder();
            }
            FileInfo[] externalFiles = new FileInfo[0];
            if (Directory.Exists(externalPath)) {
                externalFiles = Directory.GetFiles(externalPath, "*.*", SearchOption.AllDirectories).Select(v => new FileInfo(v)).ToArray();
            }*/

            Directory.CreateDirectory(LOCAL_TEXTURES_PATH);

            foreach (var mat in selectedMaterials) {
                string materialGenericName = GetGenericNameForMaterial(mat.name);
                /*var filteredExternalFiles = externalFiles.Where(v => v.Name.ToLower().StartsWith(materialGenericName)).ToList();
                foreach (FileInfo filteredExternalFile in filteredExternalFiles) {
                    string localFile = LOCAL_TEXTURES_PATH + "\\" + filteredExternalFile.Name;
                    if (!File.Exists(localFile) || CalculateMD5(filteredExternalFile.FullName) != CalculateMD5(localFile)) {
                        if (!File.Exists(localFile)) {
                            Debug.Log("Copy::" + filteredExternalFile.Name + "::Cause !Exist");
                        } else {
                            Debug.Log("Copy::" + filteredExternalFile.Name + "::Cause !MD5");
                        }
                        File.Copy(filteredExternalFile.FullName, localFile, true);
                    }
                }*/

                //Check for null is strange, why texture2Ds can contain null elements?..
                var texsByMat = texture2Ds.Where(v => v != null && v.name.ToLower().StartsWith(materialGenericName)).ToArray();
                string DiffuseMapName = null;
                string NormalMapName = null;
                Shader defaultShader = mat.shader;
                mat.shader = Shader.Find(PROTOTYPE_SHADER);
                foreach (var tex in texsByMat) {
                    string texNameLower = tex.name.ToLower();
                    if (CheckAlbedoName(texNameLower, mat.name) ||
                    texNameLower == materialGenericName + "_color"//for SpeedTrees 
                    ) {
                        //Debug.Log(string.Format("Material: {0}, diffuse map: {1}", mat.name, tex.name));
                        SetAlbedoMap(mat, tex);
                        if (DiffuseMapName != null) {
                            Debug.LogError("Duplicate diffuse texture::" + tex.name + "::" + DiffuseMapName);
                        }
                        DiffuseMapName = tex.name;
                    } else if( CheckSpecularSmoothnessName(texNameLower, materialGenericName)){
                        //Debug.Log(string.Format("Material: {0}, specular map: {1}", mat.name, tex.name));
                        SetSpecMap(mat, tex);
                    } else if (texNameLower == materialGenericName + "_em") {
                        SetEmissionMap(mat, tex);
                    } else if (texNameLower == materialGenericName + "_ao") {
                        SetOcclusionMap(mat, tex);
                    } else if (CheckNormalName(texNameLower,materialGenericName)) {
                        SetNormalMap(mat, tex);
                        if (NormalMapName != null) {
                            Debug.LogError("Duplicate normal map texture::" + tex.name + "::" + NormalMapName);
                        }
                        NormalMapName = tex.name;
                    } else if (texNameLower == materialGenericName + "_m") {
                        SetMetallicMap(mat, tex);
                    } else {
                        Debug.LogWarning(string.Format("Unknown texture type! Material: {0}, texture: {1}", mat.name, tex.name));
                    }
                }
                mat.shader = defaultShader;
            }
        }

        public static string GetGenericNameForMaterial(string materialName)
        {
            string materialGenericName = materialName.ToLower();
            Match match = Regex.Match(materialGenericName, @"(.+_\d+)_");
            if (match.Success)
            {
                materialGenericName = match.Groups[1].Value;
            }
            else if (materialGenericName.Contains("_lod"))
            {
                materialGenericName = materialGenericName.Substring(0, materialGenericName.LastIndexOf("_lod"));
            }
            return materialGenericName;
        }

        public static bool CheckAlbedoName(string textureName, string materialName)
        {
            materialName = materialName.ToLower();
            textureName = textureName.ToLower();
            return textureName == materialName || textureName == materialName + "_diffuse"
             || textureName == materialName + "_diff" || textureName == materialName + "_dif";
        }

        public static bool CheckSpecularSmoothnessName(string textureName, string materialName)
        {
            materialName = materialName.ToLower();
            textureName = textureName.ToLower();
            return textureName ==  materialName + "_ss" ||
            textureName == materialName + "_spec";
        }

        
        public static bool CheckEmissionName(string textureName, string materialName)
        {
            materialName = materialName.ToLower();
            textureName = textureName.ToLower();
            return textureName == materialName + "_em";
        }

        
        public static bool CheckOcclusionName(string textureName, string materialName)
        {
            materialName = materialName.ToLower();
            textureName = textureName.ToLower();
            return textureName == materialName + "_ao";
        }

        public static bool CheckNormalName(string textureName, string materialName)
        {
            materialName = materialName.ToLower();
            textureName = textureName.ToLower();
            return textureName == materialName + "_normal" || textureName == materialName + "_nm" || textureName == materialName + "_nrm" || textureName == materialName + "_norm";
        }

        private static Material[] GetSelectedMaterials() {
            return (Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets)).Cast<Material>().ToArray();
        }

        static string CalculateMD5(string filename) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(filename)) {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        [MenuItem("Nettle/Configure Materials/Select external textures folder")]
        static void SelectTexturesFolderMenuItem() {
            SelectTexturesFolder();
        }

        private static string SelectTexturesFolder() {
            string path = EditorUtility.OpenFolderPanel("Select external textures folder", "", "");
            Environment.SetEnvironmentVariable(EXTERNAL_TEXTURES_VARIABLE, path);
            return path;
        }

        public static void SetAlbedoMap(Material mat, Texture2D tex) {
            if (mat != null && tex != null) {
                if (mat.mainTexture != null) { return; }
                mat.SetColor("_Color", Color.white);
                mat.SetTexture("_MainTex", tex);
            }
        }

        public static void SetNormalMap(Material mat, Texture2D tex) {
            if (mat != null && tex != null) {
                mat.SetTexture("_BumpMap", tex);
            }
        }

        public static void SetSpecMap(Material mat, Texture2D tex) {
            if (mat != null && tex != null) {
                mat.SetColor("_SpecColor", Color.white);
                mat.SetTexture("_SpecGlossMap", tex);
            }
        }

        public static void SetEmissionMap(Material mat, Texture2D tex) {
            if (mat != null && tex != null) {
                mat.SetColor("_EmissionColor", Color.white);
                mat.SetTexture("_EmissionMap", tex);
            }
        }

        public static void SetOcclusionMap(Material mat, Texture2D tex) {
            if (mat != null && tex != null) {
                mat.SetTexture("_OcclusionMap", tex);
            }
        }

        public static void SetMetallicMap(Material mat, Texture2D tex) {
            if (mat != null && tex != null) {
                mat.SetTexture("_MetallicGlossMap", tex);
            }
        }
    }
}
