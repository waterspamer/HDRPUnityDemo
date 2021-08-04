using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using Nettle.SVN;
using System.Linq;
using static Nettle.SVN.Svn;
using System.IO;
using System.Linq;

namespace Nettle
{
    public static class ContentDependenciesLoader
    {
        private const string _progressBarTitle = "Loading dependencies";
        private const string _modelsLibURL = "http://nettle-server/svn/NettleModelsLib";
        private const string _unityLibRepoPath = "http://nettle-server/svn/UnityLib/trunk";
        private const string _modelsLibLocalPath = "ContentFromModelsLib";
        private static List<Entry> _scenesFolderContents;
        private static List<Entry> _exportFolderContents;

        private static List<Entry> _modelFilesLoaded = new List<Entry>();
        private static List<string> _modelFilesMissing = new List<string>();
        private static List<Material> _loadedMaterials = new List<Material>();
        private static List<Material> _materialsInProject = new List<Material>();

        private enum TextureSlot { None, Albedo, Normal, SpecularSmoothness, Occlusion, Emission }
       // private enum TextureLoadingStatus {NotLoadedYet, Loaded, Error}

        private static readonly List<string> _imageFileExtensions = new List<string>{ "png", "tiff", "tif", "jpg", "jpeg", "tga", "bmp", "exr", "pict", "psd", "iff"};

        private static GameObjectList _currentObjectList;

        [MenuItem("Assets/Nettle/Load dependencies for dummies")]
        public static void LoadDependenciesForSelection()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("Selection is not a game object");
                return;
            }
            _currentObjectList = ScriptableObject.CreateInstance<GameObjectList>();
            EditorUtility.DisplayProgressBar(_progressBarTitle,"Creating textures list",0);
            _scenesFolderContents = Svn.ListVerbose(_modelsLibURL +"/scenes", true);
            EditorUtility.DisplayProgressBar(_progressBarTitle,"Creating models list",0);
            _exportFolderContents = Svn.ListVerbose(_modelsLibURL +"/export", true);
            Transform[] dummies = Selection.activeGameObject.GetComponentsInChildren<Transform>(true);
            Directory.CreateDirectory(Application.dataPath + "/" + _modelsLibLocalPath + "/Models");
            Directory.CreateDirectory(Application.dataPath + "/" + _modelsLibLocalPath + "/Textures");
            /*
            string localFolderPath = Application.dataPath + "/" + _unityLibLocalPath;
            if (!Directory.Exists(localFolderPath))
            {
                List<string> svnProp = Svn.PropGet(Application.dataPath, "svn:external").ToList();
                if (svnProp.Find(x=>x.Contains(_unityLibLocalPath))==null)
                {
                    svnProp.Add(_unityLibLocalPath + " " + _unityLibLocalPath + "/" + _unityLibLocalPath);
                }
            }*/
            _modelFilesLoaded.Clear();
            _modelFilesMissing.Clear();
            _loadedMaterials.Clear();

            string[] materialGuids = AssetDatabase.FindAssets("t:Material");
            _materialsInProject.Clear();
            for (int i = 0; i < materialGuids.Length; i++)
            {
                _materialsInProject.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(materialGuids[i]), typeof(Material)) as Material);
            }

            int counter = 0;
            foreach (Transform dummy in dummies)
            {
                EditorUtility.DisplayProgressBar(_progressBarTitle,"Finding meshes for " + dummy.name,(float)counter/(float)dummies.Length);
                counter++;
                LoadMeshesForDummy(dummy.name);
            }
            counter = 0;
            Shader customShader = Shader.Find("Hidden/ConfigureMaterialPrototype");
            foreach (Entry scenesFolderEntry in  _scenesFolderContents)
            {                
                EditorUtility.DisplayProgressBar(_progressBarTitle,"Applying textures to materials",(float)counter/(float)_scenesFolderContents.Count);    
                counter++;
                if (scenesFolderEntry.Kind == NodeType.Directory)
                {
                    continue;
                }
                Match match = Regex.Match(scenesFolderEntry.Name, @"[\\,\/]([^\\,\/,\s]+)\.(\w+)$");
                if (!match.Success)
                {
                    continue;
                }
                string extension = match.Groups[2].Value;
                if (!_imageFileExtensions.Contains(extension))
                {
                    continue;
                }
                string textureName = match.Groups[1].Value;
                Texture2D texture = null;
                foreach (Material mat in _loadedMaterials)
                {
                    if (mat.shader.name == "Standard")
                    {
                        mat.shader = customShader;
                    }
                    string materialGenericName = ConfigureMaterials.GetGenericNameForMaterial(mat.name);
                    TextureSlot slot = TextureSlot.None;
                    if (ConfigureMaterials.CheckAlbedoName(textureName, mat.name))
                    {
                        slot = TextureSlot.Albedo;
                    }else if (ConfigureMaterials.CheckNormalName(textureName, materialGenericName))
                    {
                        slot = TextureSlot.Normal;
                    }
                    else if (ConfigureMaterials.CheckSpecularSmoothnessName(textureName, materialGenericName))
                    {
                        slot = TextureSlot.SpecularSmoothness;
                    }
                    else if (ConfigureMaterials.CheckOcclusionName(textureName, materialGenericName))
                    {
                        slot = TextureSlot.Occlusion;
                    }
                    else if (ConfigureMaterials.CheckEmissionName(textureName, materialGenericName))
                    {
                        slot = TextureSlot.Emission;
                    }
                    if (slot == TextureSlot.None)
                    {
                        continue;
                    }
                    //Debug.Log("Texture " + textureName + " goes to material " + mat.name + " " + slot, mat);
                    if (texture == null)
                    {
                        Svn.Export(_modelsLibURL + "/scenes/" + scenesFolderEntry.Name, Application.dataPath + "/" + _modelsLibLocalPath + "/Textures");
                        string assetPath = "Assets/" + _modelsLibLocalPath + "/Textures/"+textureName+"."+extension;
                        AssetDatabase.ImportAsset(assetPath);
                        texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
                        if (texture == null)
                        {
                            Debug.LogWarning("Error loading texture " + textureName);
                            break;
                        }
                    }
                    if (slot == TextureSlot.Albedo)
                    {
                        ConfigureMaterials.SetAlbedoMap(mat, texture);
                    }else  if (slot == TextureSlot.Normal)
                    {
                        ConfigureMaterials.SetNormalMap(mat, texture);
                    }else  if (slot == TextureSlot.SpecularSmoothness)
                    {
                        ConfigureMaterials.SetSpecMap(mat, texture);
                    }else  if (slot == TextureSlot.Occlusion)
                    {
                        ConfigureMaterials.SetOcclusionMap(mat, texture);
                    }else  if (slot == TextureSlot.Emission)
                    {
                        ConfigureMaterials.SetEmissionMap(mat, texture);
                    }
                }                
            }
            AssetDatabase.CreateAsset(_currentObjectList, "Assets/" + _modelsLibLocalPath + "/" + Selection.activeGameObject.name + ".asset");
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        private static void LoadMeshesForDummy(string dummyName)
        {
            Match match = Regex.Match(dummyName, @"(.+)_\d*$");
            string targetObjectName;
            if (match.Success)
            {
                targetObjectName = match.Groups[1].Value;
            }
            else
            {
                targetObjectName = dummyName;
            }
            if (_modelFilesMissing.Contains(targetObjectName))
            {
                return;
            }

            GameObject objectForDummy = null;

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab " + targetObjectName);
            if (prefabGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
                objectForDummy = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            }
            else
            {
                System.Predicate<Entry> nameSearch = x => x.Kind == NodeType.File && x.Name.ToLower().EndsWith(targetObjectName.ToLower() + ".fbx");
                Entry modelEntry = _modelFilesLoaded.Find(nameSearch);
                if (modelEntry == null)
                {
                    modelEntry = _exportFolderContents.Find(nameSearch);
                    if (modelEntry != null)
                    {
                        _modelFilesLoaded.Add(modelEntry);
                        //TODO - compare revisions to avoid re-loading the same model, if one is already loaded. Will require storing revision info in UnityLib   
                        string localPath = Application.dataPath + "/" + _modelsLibLocalPath + "/Models";
                        if (File.Exists(localPath))
                        {
                            File.Delete(localPath);
                        }
                        Svn.Export(_modelsLibURL + "/export/" + modelEntry.Name, localPath);
                        string assetPath = "Assets/" + _modelsLibLocalPath + "/Models/" + targetObjectName + ".fbx";
                        AssetDatabase.ImportAsset(assetPath);
                        objectForDummy = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                        if (objectForDummy != null)
                        {
                            Renderer[] renderers = objectForDummy.GetComponentsInChildren<Renderer>();
                            foreach (Renderer r in renderers)
                            {
                                Material[] mats = r.sharedMaterials;
                                for (int i = 0; i < mats.Length; i++)
                                {
                                    if (mats[i] == null)
                                    {
                                        continue;
                                    }
                                    Material existingMat = _materialsInProject.Find(x => x!=null && x.name == mats[i].name);
                                    string existingMatPath = existingMat!=null? AssetDatabase.GetAssetPath(existingMat): "";
                                    if (existingMat != null && !existingMatPath.Contains(_modelsLibLocalPath) && existingMat.GetTexture("_MainTex")!=null)
                                    {
                                        if (existingMat != mats[i])
                                        {
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(mats[i]));
                                            mats[i] = existingMat;
                                        }
                                    }
                                    else
                                    {
                                        if (!_loadedMaterials.Contains(mats[i]))
                                        {
                                            _loadedMaterials.Add(mats[i]);
                                        }
                                    }
                                }
                                r.sharedMaterials = mats;
                            }
                        }
                    }
                    else
                    {
                        _modelFilesMissing.Add(targetObjectName);
                        Debug.LogWarning("Couldn't find model file for " + targetObjectName);
                        return;
                    }
                }            
            }
            if (objectForDummy != null)            
            {
                _currentObjectList.Elements.Add(objectForDummy);
            }
        }

    }
}