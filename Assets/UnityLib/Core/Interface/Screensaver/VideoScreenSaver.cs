using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace Nettle {

public class VideoScreenSaver : DeprecatedScreenSaver {
#if UNITY_STANDALONE
    public RawImage RawImageComponent;
    public GameObject CanvasGameObject;
    //public MovieTexture Movie; todo upgrade to videoplayer
    [ConfigField]
    public int ScreenWidth = 1280;
    [ConfigField]
    public int ScreenHight = 720;
    public AudioSource AudioSourceComponent;
    [ConfigField]
    public bool PlayAudio = false;

    public override void OnStart() {
        if (RawImageComponent == null ) { return; }
        //Movie.loop = true;
        //RawImageComponent.texture = Movie;

        CanvasGameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(ScreenWidth, ScreenHight);
    }

    public override void OnShowScreenSaver() {
        if (CanvasGameObject == null ) { return; }

        var canvas = CanvasGameObject.GetComponent<Canvas>();
        canvas.worldCamera = _screensaverCam;

        CanvasGameObject.SetActive(true);

        if (PlayAudio && AudioSourceComponent != null) {
            AudioSourceComponent.Play();
        }
    }

    public override void OnHideScreenSaver() {
        if (CanvasGameObject == null ) { return; }

        CanvasGameObject.SetActive(false);

        if (PlayAudio && AudioSourceComponent != null) {
            AudioSourceComponent.Stop();
        }
    }
#endif
}
}
