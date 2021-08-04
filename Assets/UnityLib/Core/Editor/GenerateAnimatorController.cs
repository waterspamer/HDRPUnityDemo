using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nettle {
    public class GenerateAnimatorController : MonoBehaviour {
        private static readonly string _nametoskip = "__preview__";
        [MenuItem("Assets/Create/Nettle/CompleteAnimatorControllers")]
        static void CreateController() {
            var objs = Selection.gameObjects;
            if(objs == null) {
                Debug.Log("Select FBX's");
                return;
            }

            foreach (var obj in objs) {
                string path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                var elements = AssetDatabase.LoadAllAssetsAtPath(path);

                Motion clip = null;
                foreach (var element in elements) {
                    var elementAsClip = element as AnimationClip;
                    if (elementAsClip != null) {
                        if (!elementAsClip.name.Contains(_nametoskip)) {
                            clip = elementAsClip;
                        } else {
                            Debug.Log(elementAsClip.name + " skipped");
                        }
                    }
                }

                int dogPosition = path.LastIndexOf("@");
                if (dogPosition != -1) {
                    path = path.Remove(dogPosition);
                } else if (path.LastIndexOf(".") != -1) {
                    path = path.Remove(path.LastIndexOf("."));
                }

                path += ".controller";
                var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
                if (clip != null) {
                    controller.AddMotion(clip);
                } else {
                    Debug.LogError("No animation clip in asset");
                }
            }
        }
    }
}
