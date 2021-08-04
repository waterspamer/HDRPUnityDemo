#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nettle {

public class GameObjectEx  {

    public static string GetGameObjectPath(GameObject gameObject)
    {
        string path = gameObject.transform.name;
        while (gameObject.transform.parent != null)
        {
            gameObject = gameObject.transform.parent.gameObject;
            path = gameObject.transform.name + "/" + path;
        }
        return path;
    }
#if UNITY_EDITOR
    public static bool IsPrefabInAsset(GameObject gameObject) {

        return PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == null && PrefabUtility.GetPrefabObject(gameObject) != null;
    }
#endif
}
}
