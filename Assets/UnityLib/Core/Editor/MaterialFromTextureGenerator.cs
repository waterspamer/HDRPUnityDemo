using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Nettle {


    public class MaterialFromTextureGenerator {

        [MenuItem("Assets/Generate Materials")]
        private static void AssignTexturesByName() {
            var selectedTextures = GetSelectedTextures();

            foreach (var texture2D in selectedTextures) {
                var filePath = AssetDatabase.GetAssetPath(texture2D.GetInstanceID());
                string dir = Path.GetDirectoryName(filePath);
                Debug.Log(dir);
                Material material = new Material(Shader.Find("Standard"));
                material.mainTexture = texture2D;
                material.name = texture2D.name;
                AssetDatabase.CreateAsset(material, dir+ "/"+ material.name+".mat");
            }
        }

        private static Texture2D[] GetSelectedTextures() {
            return (Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets)).Cast<Texture2D>().ToArray();
        }
    }
}
