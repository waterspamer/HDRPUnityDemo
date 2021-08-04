using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace Nettle.Core {

#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    public class StaticLODManager : MonoBehaviour {

        public enum LoddingType { DisplayScale, Distance }

        public MotionParallaxDisplay Display;
        public float TileSize = 200f;        
        public int StartBakeFromLODLevel = 1;

        [HideInInspector] public List<GameObject> LODs = new List<GameObject>();

        [HideInInspector] public Transform SourceRoot;
        [HideInInspector] public List<Transform> SourceObjects;
        [HideInInspector] public GameObject SourcePrefab;

        [FormerlySerializedAs("PixelsPerUnit")]
        [HideInInspector] public float[] DisplayScales = new float[20];

        public void OnValidate() {
            EditorInit();
        }

        public void Reset() {
            EditorInit();
            if (!GetComponent<StaticLODManagerForRemoteDisplay>()) {
                gameObject.AddComponent<StaticLODManagerForRemoteDisplay>();
            }
        }


        public void EditorInit() {
            if (!Display) {
                Display = FindObjectOfType<MotionParallaxDisplay>();
            }
            GenerateHierarchy();
        }


        public void Update() {
            int desireLod = DisplayScales.Length;
            for (int i = 0; i < LODs.Count; i++) {
                if (DisplayScales[i] >= Display.transform.localScale.x + Display.transform.position.y)  {
                    desireLod = i;
                    break;
                }
            }
            SetLodLevel(desireLod);
        }

        public void GenerateHierarchy() {
            if (!SourceRoot && !SourcePrefab) {
                SourceRoot = transform.Find("SourceRoot");
                if (SourceRoot == null) {
                    SourceRoot = new GameObject("SourceRoot").transform;
                    SourceRoot.parent = transform;
                    SourceRoot.Reset();
                }
            }
        }


        [HideInInspector] public int CurrentLOD = 0;

        public void NextLod() {
            SetLodLevel(CurrentLOD + 1);
        }

        public void PreviousLod() {
            SetLodLevel(CurrentLOD - 1);
        }

#if UNITY_EDITOR
        public void GenerateLODs() {
            UpdateSourceObjectsList();
            Dictionary<Vector2Int, List<List<GameObject>>> allSourceObjectLODsByTiles = new Dictionary<Vector2Int, List<List<GameObject>>>();
            LODs.ForEach(v => DestroyImmediate(v));
            LODs = new List<GameObject>();

            float maxLodLevel = -1;
            for (int i = 0; i < SourceObjects.Count; i++) {
                Transform sourceObject = SourceObjects[i];
                List<GameObject> sourceObjectLODs = ParseSourceObject(sourceObject);

                if (sourceObjectLODs.Count > 0) {
                    Vector2Int tile = GetTile(sourceObject.position);
                    List<List<GameObject>> allSourceObjectLODs;
                    if (!allSourceObjectLODsByTiles.TryGetValue(tile, out allSourceObjectLODs)) {
                        allSourceObjectLODs = new List<List<GameObject>>();
                        allSourceObjectLODsByTiles.Add(tile, allSourceObjectLODs);
                    }
                    allSourceObjectLODs.Add(sourceObjectLODs);
                    if (maxLodLevel < sourceObjectLODs.Count - 1) {
                        maxLodLevel = sourceObjectLODs.Count - 1;
                    }
                } else {
                    Debug.LogError(sourceObject.name + " has no _LOD0 and will be skipped");
                }
            }

            foreach (var allSourceObjectLODsByTile in allSourceObjectLODsByTiles) {
                
                Tree tree;
                try
                {
                    tree = allSourceObjectLODsByTile.Value[0].First(v => v.GetComponent<Tree>()).GetComponent<Tree>();
                }
                catch
                {
                    tree = null;
                }
                for (int lodLevel = 0; lodLevel <= maxLodLevel; lodLevel++) {
                    GameObject lodTileGO = CreateTile(lodLevel, allSourceObjectLODsByTile.Key);
                    if (lodLevel < StartBakeFromLODLevel) {
                        foreach (var lods in allSourceObjectLODsByTile.Value) {
                            GameObject lodInstance = Instantiate(lods[lodLevel]);
                            lodInstance.transform.parent = lodTileGO.transform;
                            lodInstance.transform.position = lods[0].transform.position;
                            lodInstance.transform.rotation = lods[0].transform.rotation;
                            lodInstance.transform.localScale = lods[0].transform.lossyScale;
                        }
                    } else {
                        ArrayList materials;
                        List<List<CombineInstance>> combineInstanceArrays;
                        CreateCombineInstanses(allSourceObjectLODsByTile.Value, lodLevel, allSourceObjectLODsByTile.Key, out combineInstanceArrays, out materials);

                        MeshFilter meshFilterCombine = lodTileGO.AddComponent<MeshFilter>();
                        MeshRenderer meshRendererCombine = lodTileGO.AddComponent<MeshRenderer>();
                        CombineLodLevel(meshFilterCombine, meshRendererCombine, materials, combineInstanceArrays);
                        if (tree!=null) {
#if UNITY_EDITOR
                            UnityEditorInternal.ComponentUtility.CopyComponent(tree);
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(lodTileGO);
#endif

                        }
                        SaveLODToAsset(meshFilterCombine);
                    }



                }
            }

            SetLodLevel(LODs.Count - 1);
        }

        private Vector2Int GetTile(Vector3 position) {
            Vector2 pos = new Vector2(position.x, position.z);
            Vector2Int tile = new Vector2Int(Mathf.FloorToInt(pos.x / TileSize), Mathf.FloorToInt(pos.y / TileSize));
            return tile;
        }

        private Vector3 GetTileCenter(Vector3 position) {
            Vector2Int tile = GetTile(position);
            return GetTileCenter(tile);
        }

        private Vector3 GetTileCenter(Vector2Int tile) {
            return new Vector3(tile.x * TileSize + TileSize / 2f, 0, tile.y * TileSize + TileSize / 2f);
        }

        private List<GameObject> ParseSourceObject(Transform sourceObjectTransform) {
            List<GameObject> sourceObjectLODs = new List<GameObject>();
            Transform[] children = sourceObjectTransform.GetChildren();
            for (int lodLevel = 0; lodLevel < sourceObjectTransform.childCount; lodLevel++)
            {
                try
                {
                    Transform lod = children.Where(x => x.name.Contains("LOD" + lodLevel)).First();
                    if (lod!=null)
                    {
                        sourceObjectLODs.Add(lod.gameObject);
                    }
                }
                catch
                {

                }
            }
            return sourceObjectLODs;
        }

        private void CreateCombineInstanses(List<List<GameObject>> allSourceObjectLODs, int lodLevel, Vector2Int tile, out List<List<CombineInstance>> combineInstanceArrays, out ArrayList materials) {
            materials = new ArrayList();
            combineInstanceArrays = new List<List<CombineInstance>>();
            foreach (List<GameObject> sourceObjectLODs in allSourceObjectLODs) {
                GameObject lodObject;
                if (sourceObjectLODs.Count > lodLevel) {
                    lodObject = sourceObjectLODs[lodLevel];
                } else {
                    lodObject = sourceObjectLODs.Last();
                }

                Renderer renderer = lodObject.GetComponentInChildren<Renderer>();
                if (!renderer || renderer.gameObject == gameObject) {
                    continue;
                }

                Mesh mesh = GetSharedMesh(renderer);
                if (!mesh || renderer.sharedMaterials.Length != mesh.subMeshCount) {
                    continue;
                }

                Matrix4x4 localToTileLocal = lodObject.transform.localToWorldMatrix;

                Vector3 pos = localToTileLocal.GetPosition();
                //pos = new Vector3(pos.x % TileSize - TileSize / 2 + (pos.x < 0 ? TileSize : 0), pos.y, pos.z % TileSize - TileSize / 2 + (pos.z < 0 ? TileSize : 0));
                pos = pos - GetTileCenter(tile);
                localToTileLocal.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));

                Tree tree = renderer.GetComponent<Tree>();
                if (tree) {
                    mesh = CreateTreeFixedMesh(mesh, localToTileLocal);
                }

                for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++) {
                    int materialIndex = FindMaterialIndex(materials, renderer.sharedMaterials[subMeshIndex].name);
                    if (materialIndex == -1) {
                        materials.Add(renderer.sharedMaterials[subMeshIndex]);
                        materialIndex = materials.Count - 1;
                        combineInstanceArrays.Add(new List<CombineInstance>());
                    }
                    CombineInstance combineInstance = CreateCombineInstance(localToTileLocal, subMeshIndex, mesh);
                    combineInstanceArrays[materialIndex].Add(combineInstance);
                }
            }
        }

        private Mesh CreateTreeFixedMesh(Mesh sourceMesh, Matrix4x4 localToTileLocal) {
            Mesh mesh = Instantiate(sourceMesh);

            List<Vector4> uv2 = new List<Vector4>();
            List<Vector4> uv3 = new List<Vector4>();
            List<Vector4> uv4 = new List<Vector4>();
            List<Vector2> uv5 = new List<Vector2>();
            mesh.GetUVs(1, uv2);
            mesh.GetUVs(2, uv3);
            mesh.GetUVs(3, uv4);
            for (var i = 0; i < uv2.Count; i++) {
                if (uv4[i].w + 0.25 > 1) {
                    Vector3 point = new Vector3(uv2[i].z, uv2[i].w, uv3[i].w);
                    transform.TransformPoint(point);
                    Vector3 translatedPoint = localToTileLocal.MultiplyPoint3x4(point);
                    uv2[i] = new Vector4(uv2[i].x, uv2[i].y, translatedPoint.x, translatedPoint.y);
                    uv3[i] = new Vector4(uv3[i].x, uv3[i].y, uv3[i].z, translatedPoint.z);
                }
                Vector3 worldPos = localToTileLocal.GetPosition();
                uv5.Add(new Vector4(worldPos.x, worldPos.y, worldPos.z));
            }
            mesh.SetUVs(1, uv2);
            mesh.SetUVs(2, uv3);
            mesh.SetUVs(4, uv5);

            return mesh;
        }

        private GameObject CreateTile(int lodLevel, Vector2Int tile) {
            string lodName = "LOD" + lodLevel;
            Transform lodLevelRoot = transform.Find(lodName);
            if (lodLevelRoot == null) {
                lodLevelRoot = new GameObject(lodName).transform;
                lodLevelRoot.SetParent(transform);
                LODs.Add(lodLevelRoot.gameObject);
            }
            GameObject lodLevelTile = new GameObject("Tile_" + tile.x + "_" + tile.y);
            lodLevelTile.transform.SetParent(lodLevelRoot);
            lodLevelTile.transform.position = GetTileCenter(tile);
            return lodLevelTile;
        }



        private void SaveLODToAsset(MeshFilter meshFilter) {
            string path = "Assets/StaticLODs/" + meshFilter.gameObject.scene.name + "/"
                + meshFilter.transform.parent.parent.name + "/" + meshFilter.transform.parent.name + "/";
            Directory.CreateDirectory(path);
            AssetDatabase.CreateAsset(meshFilter.sharedMesh, path + meshFilter.gameObject.name + ".mesh");
            AssetDatabase.SaveAssets();
        }

        private CombineInstance CreateCombineInstance(Matrix4x4 localToTileLocal, int subMeshIndex, Mesh sharedMesh) {
            CombineInstance combineInstance = new CombineInstance();
            combineInstance.transform = localToTileLocal;
            combineInstance.subMeshIndex = subMeshIndex;
            combineInstance.mesh = sharedMesh;
            return combineInstance;
        }

        public void SaveSources() {
            if (SourcePrefab && !EditorUtility.DisplayDialog("Rewrite prefab?", "Prefab already exist. Rewrite it?", "Yes", "Cancel")) {
                return;
            }
            Stopwatch watch1 = Stopwatch.StartNew();
            UpdateSourceObjectsList();
            string path = "Assets/StaticLODs/" + gameObject.scene.name + "/" + gameObject.name + "/";
            Directory.CreateDirectory(path);


            for (int i = 0; i < SourceObjects.Count; i++) {
                Transform goTransform = SourceObjects[i];
                List<Renderer> renderers = goTransform.GetComponentsInChildren<Renderer>().ToList();
                foreach (var renderer in renderers) {
                    Mesh sharedMesh = GetSharedMesh(renderer);
                    LoadDefaultMesh(renderer);
                    Mesh defaultSharedMesh = GetSharedMesh(renderer);
                    if (sharedMesh == defaultSharedMesh) {
                        Color[] sharedMeshColors = sharedMesh.colors;
                        if (sharedMeshColors.Length > 0) {

                            bool differentMeshesColors = false;
                            bool differentVertexColors = false;
                            Color[] defayltSharedMeshColors = defaultSharedMesh.colors;

                            for (var vertexIndex = 0; vertexIndex < sharedMeshColors.Length; vertexIndex++) {
                                if (sharedMeshColors.Length != defayltSharedMeshColors.Length ||
                                    sharedMeshColors[vertexIndex] != defayltSharedMeshColors[vertexIndex]) {
                                    differentMeshesColors = true;
                                }
                                if (sharedMeshColors[0] != sharedMeshColors[vertexIndex]) {
                                    differentVertexColors = true;
                                }
                            }

                            if (differentMeshesColors) {
                                VertexColor vertexColor = renderer.GetComponent<VertexColor>();
                                if (!vertexColor) {
                                    vertexColor = renderer.gameObject.AddComponent<VertexColor>();
                                    if (differentVertexColors) {
                                        vertexColor.Colors = sharedMeshColors;
                                    } else {
                                        vertexColor.Colors = new[] { sharedMeshColors[0] };
                                    }
                                }
                            }
                        }
                    }
                }
            }

            SourcePrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(SourceRoot.gameObject, path + SourceRoot.name + ".prefab", InteractionMode.AutomatedAction);
            LoadSourceColors();
            watch1.Stop();
            Debug.Log(gameObject.name + "::Saved by " + watch1.ElapsedMilliseconds / 1000 + "." + (watch1.ElapsedMilliseconds % 1000).ToString("0000") + " sec");
        }

        private void LoadDefaultMesh(Renderer renderer) {
            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer) {
                SkinnedMeshRenderer original = PrefabUtility.GetCorrespondingObjectFromOriginalSource(skinnedMeshRenderer);
                skinnedMeshRenderer.sharedMesh = original.sharedMesh;
            } else {
                MeshFilter meshFilter = renderer.GetComponentInChildren<MeshFilter>();
                MeshFilter original = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshFilter);
                meshFilter.sharedMesh = original.sharedMesh;
            }
        }

        private Mesh GetSharedMesh(Renderer renderer) {
            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer) {
                return skinnedMeshRenderer.sharedMesh;
            } else {
                return renderer.GetComponentInChildren<MeshFilter>().sharedMesh;
            }
        }

        private void SetSharedMesh(Renderer renderer, Mesh mesh) {
            SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer) {
                skinnedMeshRenderer.sharedMesh = mesh;
            } else {
                renderer.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
            }
        }

        public void LoadSources() {
            if (SourceRoot) {
                List<Transform> SavedSourceObjects =
                    GenerateSourceObjectsList(PrefabUtility.GetCorrespondingObjectFromSource(SourceRoot.gameObject).transform);
                if (SavedSourceObjects.Count != SourceObjects.Count //Check for changes. Not good but better then nothing
                    && !EditorUtility.DisplayDialog("Ignore changes?", "Prefab instance has new unsaved objects. Loading prefab lead to losing this objects. Continue?", "Yes", "Cancel")) {
                    return;
                }
            }
            if (SourceRoot) {
                UnloadSources(true);
            }
            SourceRoot = ((GameObject)PrefabUtility.InstantiatePrefab(SourcePrefab, transform)).transform;
            //SourceRoot.parent = transform;            
            UpdateSourceObjectsList();
            LoadSourceColors();
        }


        public void UnloadSources(bool forceUnload = false) {
            List<Transform> SavedSourceObjects = GenerateSourceObjectsList(PrefabUtility.GetCorrespondingObjectFromSource(SourceRoot.gameObject).transform);
            if (!forceUnload && SavedSourceObjects.Count != SourceObjects.Count //Check for changes. Not good but better then nothing
                && !EditorUtility.DisplayDialog("Ignore changes?", "Prefab instance has new unsaved objects. Unload instance without saving?", "Yes", "Cancel")) {
                return;
            }
            DestroyImmediate(SourceRoot.gameObject);
            SourceRoot = null;
        }

        private List<Transform> GenerateSourceObjectsList(Transform root) {
            return root.GetComponentsInChildren<Transform>().Where(v => v.name.ToLower().Contains("_lod0") && v.GetComponent<Renderer>()).Select(v => v.parent).ToList();
        }

        private void UpdateSourceObjectsList() {
            SourceObjects = GenerateSourceObjectsList(SourceRoot);
        }

        public void LoadSourceColors() {
            for (int i = 0; i < SourceObjects.Count; i++) {
                Transform goTransform = SourceObjects[i];

                List<Renderer> renderers = goTransform.GetComponentsInChildren<Renderer>().ToList();
                foreach (var renderer in renderers) {
                    VertexColor vertexColor = renderer.GetComponent<VertexColor>();
                    if (vertexColor) {
                        Mesh mesh = Instantiate(GetSharedMesh(renderer));
                        mesh.SetVerticesColor(vertexColor.Colors);
                        SetSharedMesh(renderer, mesh);
                    }
                }
            }
        }

#endif


        public void SetLodLevel(int level) {
            level = Mathf.Clamp(level, 0, LODs.Count - 1);
            for (int i = 0; i < LODs.Count; i++) {
                if (level == i) {
                    CurrentLOD = i;
                    LODs[i].SetActive(true);
                } else {
                    LODs[i].SetActive(false);
                }
            }
        }

        public void DisableLODs() {
            CurrentLOD = -1;
            LODs.ForEach(v => v.SetActive(false));
        }

        private void CombineLodLevel(MeshFilter meshFilter, MeshRenderer meshRenderer, ArrayList materials, List<List<CombineInstance>> combineInstanceArrays) {
            // Combine by material index into per-material meshes
            // also, Create CombineInstance array for next step
            Mesh[] meshes = new Mesh[materials.Count];
            CombineInstance[] combineInstances = new CombineInstance[materials.Count];

            for (int m = 0; m < materials.Count; m++) {
                CombineInstance[] combineInstanceArray = combineInstanceArrays[m].ToArray();
                meshes[m] = new Mesh();
                meshes[m].indexFormat = IndexFormat.UInt32;
                meshes[m].CombineMeshes(combineInstanceArray, true, true);

                combineInstances[m] = new CombineInstance();
                combineInstances[m].mesh = meshes[m];
                combineInstances[m].subMeshIndex = 0;
            }

            // Combine into one
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
            meshFilter.sharedMesh.CombineMeshes(combineInstances, false, false);

            // Assign materials
            Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
            meshRenderer.materials = materialsArray;
        }

        private int FindMaterialIndex(ArrayList searchList, string searchName) {
            for (int i = 0; i < searchList.Count; i++) {
                if (((Material)searchList[i]).name == searchName) {
                    return i;
                }
            }
            return -1;
        }
    }




#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StaticLODManager))]
    public class StaticLODManagerEditor : Editor {
        public override void OnInspectorGUI() {

            StaticLODManager staticLODManager = (StaticLODManager)target;


            EditorGUI.BeginDisabledGroup(!staticLODManager.SourceRoot);
            if (GUILayout.Button("Generate LODs")) {
                foreach (Object obj in targets) {
                    StaticLODManager manager = (StaticLODManager)obj;
                    manager.GenerateLODs();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!staticLODManager.SourceRoot);
            if (GUILayout.Button("Save Sources")) {
                foreach (Object obj in targets) {
                    StaticLODManager manager = (StaticLODManager)obj;
                    manager.SaveSources();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!staticLODManager.SourcePrefab);
            if (GUILayout.Button("Load Sources")) {
                foreach (Object obj in targets) {
                    StaticLODManager manager = (StaticLODManager)obj;
                    manager.LoadSources();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!staticLODManager.SourceRoot || !staticLODManager.SourcePrefab);
            if (GUILayout.Button("Unload Sources")) {
                foreach (Object obj in targets) {
                    StaticLODManager manager = (StaticLODManager)obj;
                    manager.UnloadSources();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(staticLODManager.LODs.Count == 0);
            if (GUILayout.Button("Previous LOD")) {
                foreach (Object obj in targets) {
                    StaticLODManager manager = (StaticLODManager)obj;
                    manager.PreviousLod();
                }
            }
            if (GUILayout.Button("Next LOD")) {
                foreach (Object obj in targets) {
                    StaticLODManager manager = (StaticLODManager)obj;
                    manager.NextLod();
                }
            }
            if (GUILayout.Button("Disable LODs")) {
                foreach (Object obj in targets) {
                    StaticLODManager manager = (StaticLODManager)obj;
                    manager.DisableLODs();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            DrawDefaultInspector();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("LOD Transition");
            GUILayout.Label("Display scale");
            EditorGUILayout.EndHorizontal();
            for (var i = 0; i < staticLODManager.LODs.Count - 1; i++) {
                EditorGUI.BeginChangeCheck();
                staticLODManager.DisplayScales[i] = EditorGUILayout.FloatField(i + " - " + (i + 1), staticLODManager.DisplayScales[i]);
                if (EditorGUI.EndChangeCheck()) {
                    foreach (Object obj in targets) {
                        StaticLODManager manager = (StaticLODManager)obj;
                        if (manager.DisplayScales.Length > i) {
                            manager.DisplayScales[i] = staticLODManager.DisplayScales[i];
                        }
                    }
                }
            }


        }
    }

#endif
}