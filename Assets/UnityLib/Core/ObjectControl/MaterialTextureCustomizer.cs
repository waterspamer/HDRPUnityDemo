using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Nettle {
    [System.Serializable]
    public class CustomizedTexture {
        public string UniqueName;
        public string Path = "";
        public string MaterialSlot = "_MainTex";
        [SerializeField]
        private Material _material;
        public Material Material {
            get {
                return _material;
            }
        }
        public Vector2 Tiling = new Vector2(1, 1);
        public Vector2 Offset = Vector2.zero;
        public void CopyTo(CustomizedTexture other) {
            other.UniqueName = UniqueName;
            other.Path = Path;
            other.MaterialSlot = MaterialSlot;
            other.Tiling = Tiling;
            other.Offset = Offset;
        }
        [System.NonSerialized]
        public Texture DefaultTexture = null;
    }

    [ExecuteBefore(typeof(DefaultTime))]
    public class MaterialTextureCustomizer : MonoBehaviour {

        public string ConfigFilePath = "TextureCustomizer.xml";
        public CustomizedTexture[] Textures;

        private string FullConfigFilePath {
            get {
                return Application.streamingAssetsPath + "/" + ConfigFilePath;
            }
        }

        [EasyButtons.Button]
        public void SaveXML() {
            if (!Directory.Exists(Application.streamingAssetsPath)) {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(CustomizedTexture[]));
            Stream output = File.Open(FullConfigFilePath, FileMode.OpenOrCreate);
            serializer.Serialize(output, Textures);
            output.Close();
        }

        [EasyButtons.Button]
        public void LoadXML() {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Load Texture Customizer XML");
#endif
            if (!File.Exists(FullConfigFilePath)) {
                return;
            }
            Stream input = File.Open(FullConfigFilePath, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(CustomizedTexture[]));
            CustomizedTexture[] deserializedTextures = null;
            try {
                deserializedTextures = serializer.Deserialize(input) as CustomizedTexture[];
            }
            catch (System.Exception ex) {
                Debug.LogError("Error loading customized textures: " + ex.Message);
            }
            input.Close();
            if (deserializedTextures != null) {
                foreach (CustomizedTexture t in deserializedTextures) {
                    CustomizedTexture oldTex = null;
                    try {
                        oldTex = Textures.Where(x => x.UniqueName == t.UniqueName).FirstOrDefault();
                    }
                    catch {
                    }

                    if (oldTex != null) {
                        t.CopyTo(oldTex);
                    }
                }
            }
        }


        private Dictionary<string, Texture2D> _loadedTextures = new Dictionary<string, Texture2D>();

        private void Awake() {
            LoadXML();
            ApplyTextures();
        }             

        private Texture2D LoadTextureAtPath(string path) {
            Texture2D result;
            if (!_loadedTextures.TryGetValue(path, out result)){
                string strPath = Application.streamingAssetsPath + "/" + path;
                if (!File.Exists(strPath)) {
                    return null;
                }
                result = new Texture2D(2, 2);
                byte[] textureData = File.ReadAllBytes(strPath);
                result.LoadImage(textureData);
                result.Apply();
                _loadedTextures.Add(path, result);
            }
            return result;
        }

        private void ApplyTextures() {
            foreach (CustomizedTexture t in Textures) {
                if (t.Material == null) {
                    return;
                }
                if (t.DefaultTexture == null) {
                    t.DefaultTexture = t.Material.GetTexture(t.MaterialSlot);
                }
                t.Material.SetTextureScale(t.MaterialSlot, t.Tiling);
                t.Material.SetTextureOffset(t.MaterialSlot, t.Offset);
                Debug.Log("Tiling " + t.Tiling+ " " + t.Material.GetTextureScale(t.MaterialSlot));
                try {
                    t.Material.SetTexture(t.MaterialSlot, LoadTextureAtPath(t.Path));
                }
                catch {
                }
            }
        }

        private void OnDestroy() {
            foreach (CustomizedTexture t in Textures) {
                if (t.DefaultTexture != null) {
                    t.Material.SetTexture(t.MaterialSlot, t.DefaultTexture);
                }
            }
        }
    }
}
