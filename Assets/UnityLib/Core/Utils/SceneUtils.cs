using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace Nettle {

public static class SceneUtils {

    public static T FindObjectIfSingle<T>() where T : MonoBehaviour {
        var objects = GameObject.FindObjectsOfType<T>();
        if (objects == null || objects.Length != 1) {
            return null;
        }
        return objects[0];
    }

    public static void FindObjectIfSingle<T>(ref T obj) where T : MonoBehaviour {
        var objects = Object.FindObjectsOfType<T>();
        if (objects == null || objects.Length != 1) {
            obj = null;
        } else {
            obj = objects[0];
        }
    }

    public static List<Scene> GetAllScenes() {
        List<Scene> result = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; ++i) {
            result.Add(SceneManager.GetSceneAt(i));
        }
        return result;
    }

    public static List<T> FindObjectsOfType<T>(bool inludeInactive, IEnumerable<Scene> searchScenes = null) where T : Object {
        //fallback to current single scene
         if (searchScenes == null) {
                List<Scene> allScenes = new List<Scene>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    allScenes.Add(SceneManager.GetSceneAt(i));
                }
                searchScenes = allScenes;
            }
        List<T> result = new List<T>();
        foreach(Scene scene in searchScenes){
             try{
                    GameObject[] rootGOs =  scene.GetRootGameObjects();
                    foreach(GameObject go in rootGOs){
                        result.AddRange(FindObjectsOfTypeInChildren<T>(go, inludeInactive));
                    }
                 }
                 catch{
                     continue;
                 }
        }
        return result;
    }

    public static IEnumerable<T> FindSceneObjectsOfTypeAll<T>() where T : Object {
        return Resources.FindObjectsOfTypeAll<T>().Where(v => v.hideFlags == HideFlags.None);
    }

    public static List<T> FindObjectsOfTypeInChildren<T>(GameObject obj, bool inludeInactive) where T : Object {
        List<T> result = new List<T>();

        if (obj is T) {
            result.Add(obj as T);
        }

        var components = obj.GetComponents<Component>();
        if (components != null && components.Length != 0) {
            foreach (var c in components) {
                if (c is T) {
                    result.Add(c as T);
                }
            }
        }

        var children = obj.transform.GetChildren();
        if (children != null && children.Length != 0) {
            foreach (var child in children) {
                result.AddRange(FindObjectsOfTypeInChildren<T>(child.gameObject, inludeInactive));
            }
        }

        return result;
    }
}
}
