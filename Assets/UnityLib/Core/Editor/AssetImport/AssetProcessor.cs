using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;

namespace Nettle {
    
    public class AssetProcessor : AssetPostprocessor {
        private const string _materialFolderName = "Materials";
        private static List<NettleObjectImporter> Importers = null;
        void OnPreprocessModel() {
            var modelImporter = assetImporter as ModelImporter;
            modelImporter.materialLocation = ModelImporterMaterialLocation.External;
            modelImporter.materialSearch = ModelImporterMaterialSearch.Everywhere;
            if (Importers == null) {
                InitImporters();
            }
            
        }
        
        void OnPreprocessSpeedTree()        
        {
            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetImporter.assetPath);
            string folderName = assetImporter.assetPath.Substring(0, assetImporter.assetPath.LastIndexOf("/"));
            if (!AssetDatabase.IsValidFolder(folderName+"/" + _materialFolderName))
            {
                AssetDatabase.CreateFolder(folderName,_materialFolderName);
            }
            foreach (var subAsset in subAssets)
            {

                if (subAsset is Material)
                {
                    string matPath = folderName + "/"+_materialFolderName+"/" + subAsset.name + ".mat";                    
                    Material mat = AssetDatabase.LoadMainAssetAtPath(matPath) as Material;
                    if (mat == null)
                    {
                        AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(subAsset), matPath);
                        mat = AssetDatabase.LoadMainAssetAtPath(matPath) as Material;
                    }
                    assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(subAsset), mat);
                }
            }

            if (Importers == null)
            {
                InitImporters();
            }            
        }


        private static void InitImporters() {
            Importers = new List<NettleObjectImporter>();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var t in a.GetTypes()) {
                    if (t.IsSubclassOf(typeof(NettleObjectImporter))) {
                        Importers.Add(Activator.CreateInstance(t) as NettleObjectImporter);
                    }
                }
            }
        }

        void OnPreprocessTexture() {
            TextureImporter tex_importer = assetImporter as TextureImporter;
            if (tex_importer.importSettingsMissing) {
                tex_importer.maxTextureSize = 8192;
            }
        }


        private void OnPostprocessGameObjectWithUserProperties(GameObject gameObject, string[] names, object[] values) {
            for (int i = 0; i < names.Length; i++) {
                if (names[i] == "UDP3DSMAX") {
                    ProcessUDP3DSMAX(gameObject, values[i].ToString());
                }
            }
        }

        private void ProcessUDP3DSMAX(GameObject gameObject, string properties) {
            //Debug.Log(gameObject + "::" + properties);
            using (StringReader reader = new StringReader(properties)) {
                string property;
                while ((property = reader.ReadLine()) != null) {
                    //check old format
                    MatchCollection arrayMatches = Regex.Matches(property, @"\s*(\w+)\s*=\s*#\s*\((.*)\)", RegexOptions.IgnoreCase);
                    if (arrayMatches.Count > 0) // array property
                    {
                        property = arrayMatches[0].Groups[1].Value + "=[" + arrayMatches[0].Groups[2].Value + "]";
                        //LogWarning("Old property format!::Use PopertyName = [PropertyValues] instead of PopertyName = #(PropertyValues)");
                    }

                    arrayMatches = Regex.Matches(property, @"\s*(\w+)\s*=\s*(.*)", RegexOptions.IgnoreCase); //@"\s*(\w+)\s*=\s*([\[{].*[\]}])"
                    if (arrayMatches.Count > 0) {
                        string propertyName = arrayMatches[0].Groups[1].Value;
                        string propertyValue = arrayMatches[0].Groups[2].Value;
                        if (propertyValue.Equals("")) {
                            LogWarning("Empty user property value!::GameObject=" + gameObject + "::Property name=" + propertyName);
                        }
                        ProcessObject(gameObject, propertyName, propertyValue);
                    } else {
                        ProcessObject(gameObject, property, "");
                    }
                }
            }
        }

        private void ProcessObject(GameObject gameObject, string propertyName, string propertyValue) {
            foreach (NettleObjectImporter importer in Importers) {
                importer.ProcessObject(gameObject, propertyName, propertyValue, assetPath);
            }
        }

        void OnPostprocessModel(GameObject g) {
            List<string> materialNames = new List<string>();
            foreach (var renderer in g.GetComponentsInChildren<Renderer>()) {
                foreach (var material in renderer.sharedMaterials) {
                    if (material != null && material.shader.name == "Standard" && !materialNames.Contains(material.name)) {
                        materialNames.Add(material.name);
                    }
                }
            }
            foreach (string name in materialNames) {
                Debug.LogWarning("Standard shader on material: " + name);
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            foreach (string importedAsset in importedAssets.Where(v => v.EndsWith(".ini"))) {
                ConfigsController.ReimportOverridingConfig(importedAsset);
            }


        }


    }

}
