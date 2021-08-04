using System;
using System.IO;
using UnityEngine;

namespace Nettle {

[RequireComponent(typeof(TexturesStreaming))]
public class StreamingTexturePathUtils : MonoBehaviour {
    public VisibilityManager VisibilityManager;
    public DayNightController DayNightController;

    public string StaticTagName = "No_Slice";
    public const string ObjectNamePlaceholder = @"{obj_name}";
    public const string VisibilityTagPlaceholder = @"{vis_tag}";
    public const string DayNightTagPlaceholder = @"{dn_tag}";


    private string _streamingTag = "";

    private void Awake() {
        if (VisibilityManager == null) {
            VisibilityManager = SceneUtils.FindObjectIfSingle<VisibilityManager>();
        }
    }

    public int GetDefaultTextureId() {
        if (DayNightController != null) {
            if (DayNightController.TimeOfDay == TimeOfDay.Day) {
                return 0;
            } else {
                return 1;
            }
        }
        return 0;
    }

    public string GetTexturePath(GameObject target, string name, TexturesStreaming.StreamTextureType streamType, bool preview) {
        /*VisibilityControlAlways visibilityControl = target.GetComponent<VisibilityControlAlways>();
        if (visibilityControl) {
            if (preview && visibilityControl.PreviewLoaded) {
                preview = false;
            } else {
                visibilityControl.PreviewLoaded = true;
            }
        }*/
        var result = GetAbsoluteFolderPath(streamType.Folder, preview) + name;
        if (!File.Exists(result)) {
            if (VisibilityManager.ShowList.Contains(target.GetComponent<VisibilityControl>())) {
                return "default";
            }
        }
        return result;

    }

    public string GetAbsoluteFolderPath(string folder, bool preview) {
        return Application.streamingAssetsPath + "/" + folder + (preview ? "128/" : "");
    }

    public string GetFileName(GameObject target, string filenameFormat) {
        var result = filenameFormat.Replace(ObjectNamePlaceholder, GetNameObject(target.name));

        if (DayNightController != null) {
            if (DayNightController.TimeOfDay == TimeOfDay.Day)
                result = result.Replace(DayNightTagPlaceholder, "Day");
            if (DayNightController.TimeOfDay == TimeOfDay.Night)
                result = result.Replace(DayNightTagPlaceholder, "Night");
        }

        // var vtag = target.GetComponent<VisibilityControl>() == null ? "" : VisibilityManager.NewTag;

        if (target.GetComponent<VisibilityControlAlways>()) {
            _streamingTag = StaticTagName;
        } else {
            _streamingTag = string.IsNullOrEmpty(VisibilityManager.NewTag) ? VisibilityManager.CurrentTag : VisibilityManager.NewTag;
        }

        var vtag = target.GetComponent<VisibilityControl>() == null ? "" : _streamingTag;

        return result.Replace(VisibilityTagPlaceholder, vtag);
    }


    /*public void ReloadTextures(bool preview) {
        _streamingTag = VisibilityManager.CurrentTag;
        GetComponent<TexturesStreaming>().ReloadTextures(preview);
    }*/

    public string GetNameObject(string oldName) {
        var result = String.Empty;

        var array = oldName.Split('_');
        if (array[array.Length - 1].Contains("MeshPart")) {
            for (var i = 0; i < array.Length - 1; i++) {
                var k = "_";

                if ((i + 1) >= (array.Length - 1))
                    k = "";
                result += array[i] + k;
            }
        } else {
            result = oldName;
        }
        return result;
    }
}
}
