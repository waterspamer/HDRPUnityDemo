using System;
using UnityEngine;

namespace Nettle {

public enum LogoResizeMode {
    None, Fill, Fit
}
/// <summary>
/// This is the legacy loading screen script. For Unity 2017+ use LoadingScreenCanvas
/// </summary>
public class LoadScreen: MonoBehaviour {
    public StereoEyes Eyes;
    public Camera Cam;
    public LoadScreenManager LoadScrManager;

    public Texture2D SceneLogo;
    [ConfigField]
    public LogoResizeMode LogoAutoResize = LogoResizeMode.Fill;
    [ConfigField]
    public Vector2 LogoSize = new Vector2(1280, 720f);
    public Color ProgressbarColor;
    public Color ProgressbarBackColor;
    [ConfigField]
    public Vector2 ProgressbarSize = new Vector2(512f, 20f);
    [ConfigField]
    public Vector2 ProgressbarOffset = new Vector2(0f, 0f);

    private float _smoothProgress = 0f;

    public Texture2D Progressbar;
    public Texture2D ProgressbarBack;

    /*private void Awake() {
        #if !UNITY_EDITOR
		//SetMaxResolution();
        #endif
    }*/

    private Texture2D CreateTexture1x1(Color color) {
        Texture2D tex = new Texture2D(1, 1);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    /*private void SetMaxResolution() {
        Resolution[] resolutions = Screen.resolutions;
        int id = resolutions.Length - 1;
        Screen.SetResolution(resolutions[id].width, resolutions[id].height, true);
    }*/

    private void DrawProgressbar(UnityEngine.Rect rect, float progress) {
        if (ProgressbarBack != null) {
            Graphics.DrawTexture(rect, ProgressbarBack);
        }
        rect.width *= Mathf.Clamp01(progress);
        if (Progressbar != null) {
            Graphics.DrawTexture(rect, Progressbar);
        }
    }

    private void Start() {
        Eyes.BeforeRenderEvent += Render;

        if (!Cam) { return; }

        Color backgroundColor;
        try {
            backgroundColor = SceneLogo.GetPixel(0, 0);
        } catch (Exception ex) {
            Debug.Log("FAILED to read scene logo! Change import settings to enable texture reading!\n" + ex.Message);
            backgroundColor = Color.black;
        }

        Cam.backgroundColor = backgroundColor;
        Progressbar = CreateTexture1x1(ProgressbarColor);
        ProgressbarBack = CreateTexture1x1(ProgressbarBackColor);
    }

    private void Render() {


        Vector2 resolution = new Vector2(Screen.width, Screen.height);

        var eyeNormalizedRect = Eyes.GetCameraViewportRect(!Eyes.LeftEyeActive);
        var eyePixelRect = new Rect(eyeNormalizedRect.x * Screen.width, eyeNormalizedRect.y * Screen.height,
            eyeNormalizedRect.width * Screen.width, eyeNormalizedRect.height * Screen.height);


        //bool stereoResolution = resolution.x == 1280f && resolution.y == 1470f;
        //if (stereoResolution) {
        //    resolution.y = (resolution.y - VSyncLineSize) * 0.5f;
        //}

        Vector2 logoSize = LogoSize;
        if (LogoAutoResize != LogoResizeMode.None) {
            logoSize = new Vector2(SceneLogo.width, SceneLogo.height);
            float scaleValue = 1;
            float xDifference = eyePixelRect.width / logoSize.x;
            float yDifference = eyePixelRect.height / logoSize.y;
            if (LogoAutoResize == LogoResizeMode.Fit) {
                scaleValue = Mathf.Min(xDifference, yDifference);
            } else if (LogoAutoResize == LogoResizeMode.Fill) {
                scaleValue = Mathf.Max(xDifference, yDifference);
            }
            logoSize *= scaleValue;
        }
        Rect logoRect = new UnityEngine.Rect() {
            position = (eyePixelRect.size - logoSize) * 0.5f,
            size = logoSize
        };
        logoRect.y /= eyeNormalizedRect.height;
        logoRect.height /= eyeNormalizedRect.height;
        //  GetComponent<Camera>().rect = eyeNormalizedRect;


        GL.PushMatrix();
        GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);

        var viewportRect = eyePixelRect;
        GL.Viewport(viewportRect);

        if (SceneLogo != null) {
            Graphics.DrawTexture(logoRect, SceneLogo);
        }

        if (LoadScrManager) {
            _smoothProgress = Mathf.Lerp(_smoothProgress, LoadScrManager.LoadProgress, Time.deltaTime * 2f);
        } else {
            _smoothProgress = 0.5f;
        }

        Rect progressbarRect = new UnityEngine.Rect() {
            position = (eyePixelRect.size - ProgressbarSize) * 0.5f + ProgressbarOffset,
            size = ProgressbarSize
        };
        progressbarRect.y = eyePixelRect.height - ProgressbarOffset.y - progressbarRect.height;

        progressbarRect.y /= eyeNormalizedRect.height;
        progressbarRect.height /= eyeNormalizedRect.height;


        DrawProgressbar(progressbarRect, _smoothProgress);

        GL.PopMatrix();

        //if (stereoResolution) {
        //    float height = resolution.y + VSyncLineSize;
        //    logoRect.y += height;
        //    progressbarRect.y += height;

        //    GUI.DrawTexture(logoRect, sceneLogo);
        //    DrawProgressbar(progressbarRect, smoothProgress);
        //}
    }
}
}
