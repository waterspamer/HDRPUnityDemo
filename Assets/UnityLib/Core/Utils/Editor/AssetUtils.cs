using UnityEditor;
using UnityEngine;

namespace Nettle {

    public class AssetUtils {


        public static void CreateFolder(string path) {
            if (AssetDatabase.IsValidFolder(path) || !path.StartsWith("Assets/")) return;
            string[] folders = path.Split('/');
            string curPath = "Assets";
            for (int i = 1; i < folders.Length; i++) {
                if (!AssetDatabase.IsValidFolder(curPath + "/" + folders[i])) {
                    AssetDatabase.CreateFolder(curPath, folders[i]);
                }
                curPath += "/" + folders[i];
            }
        }

        public static bool IsObjectAPrefab(GameObject gameObject) {
            return PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == null && PrefabUtility.GetPrefabObject(gameObject) != null;
        }
    }
}
