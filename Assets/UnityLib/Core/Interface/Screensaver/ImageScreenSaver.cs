using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;

namespace Nettle {

public class ImageScreenSaver : DeprecatedScreenSaver {

    public Texture2D LogoTex;

    [ConfigField("Border")]
    public int Border = 20;
    [ConfigField("Horizontal speed")]
    public int HorizontalSpeed = 120;
    [ConfigField("Vertical speed")]
    public int VerticalSpeed = 60;

    /*private Camera _screensaverCam;
    private Camera[] _sceneCams;*/

    private Color _backgroundColor;

    private float _logoSizeX;
    private float _logoSizeY;

    private float _logoPosX;
    private float _logoPosY;

    private float _logoTargetPosX;
    private float _logoTargetPosY;

    private Coroutine _rendererCoroutine;

    public override void OnStart() {
        Texture2D logoTmp = LoadPNG(Application.dataPath + "/../Screensaver.png");

        if (logoTmp) {
            LogoTex = logoTmp;
        }

        try {
            _backgroundColor = LogoTex.GetPixel(0, 0);
        } catch (Exception ex) {
            Debug.LogWarning(ex.Message);
            return;
        }

        if (LogoTex) {
            _logoSizeX = LogoTex.width;
            _logoSizeY = LogoTex.height;
        }
    }

    public override void OnShowScreenSaver() {
        /*_sceneCams = Camera.allCameras.Where(v => v.enabled).ToArray();
        foreach (var sceneCam in _sceneCams) {
            sceneCam.enabled = false;
        }

        if (!_screensaverCam) {
            _screensaverCam = gameObject.AddComponent<Camera>();
        }

        _screensaverCam.clearFlags = CameraClearFlags.Color;
        _screensaverCam.backgroundColor = _backgroundColor;
        _screensaverCam.cullingMask = 0;
        _screensaverCam.enabled = true;*/

        var eyes = Eyes.GetComponent<StereoEyes>();
        Vector2 eyeRtSize;
        if (eyes != null) {
            eyeRtSize = eyes.GetEyeRTSize();
        } else {
            eyeRtSize = new Vector2(Screen.width, Screen.height);
        }


        _logoTargetPosX = eyeRtSize.x - Border - _logoSizeX / 2.0f;
        _logoTargetPosY = eyeRtSize.y - Border - _logoSizeY / 2.0f;

        _logoPosX = Border + _logoSizeX / 2.0f;
        _logoPosY = Border + _logoSizeY / 2.0f;

        if (_rendererCoroutine != null) {
            StopCoroutine(_rendererCoroutine);
        }
        StartCoroutine(RenderCoroutine());
    }

    public override void OnHideScreenSaver() {
        if (_rendererCoroutine != null) {
            StopCoroutine(_rendererCoroutine);
        }
        /*if (_sceneCams != null) {
            foreach (var sceneCam in _sceneCams) {
                if (sceneCam != null) {
                    sceneCam.enabled = true;
                }
            }
        }

        if (_screensaverCam) {
            Destroy(_screensaverCam);
        }*/
    }

    public static Texture2D LoadPNG(string filePath) {
        Texture2D tex = null;

        if (File.Exists(filePath)) {
            var fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    private void UpdatePosition(Rect rect) {

        float minPosX = Border + _logoSizeX / 2.0f;
        float maxPosX = rect.width - (Border + _logoSizeX / 2.0f);

        float minPosY = Border + _logoSizeY / 2.0f;
        float maxPosY = rect.height - (Border + _logoSizeY / 2.0f);

        float distToTargetX = _logoTargetPosX - _logoPosX;
        float distToTargetY = _logoTargetPosY - _logoPosY;

        float offsetX = distToTargetX > 0 ?
            _logoPosX + (HorizontalSpeed * Time.smoothDeltaTime) :
            _logoPosX - (HorizontalSpeed * Time.smoothDeltaTime);
        float offsetY = distToTargetY > 0 ?
            _logoPosY + (VerticalSpeed * Time.smoothDeltaTime) :
            _logoPosY - (VerticalSpeed * Time.smoothDeltaTime);

        _logoPosX = Mathf.Clamp(offsetX, minPosX, maxPosX);
        _logoPosY = Mathf.Clamp(offsetY, minPosY, maxPosY);


        if (Mathf.Abs(_logoPosX - _logoTargetPosX) < 0.1f) {
            if (distToTargetX > 0) {
                _logoTargetPosX = Border + _logoSizeX / 2;
            } else {
                _logoTargetPosX = rect.width - Border - _logoSizeX / 2;
            }
        }

        if (Mathf.Abs(_logoPosY - _logoTargetPosY) < 0.1f) {
            if (distToTargetY > 0) {
                _logoTargetPosY = Border + _logoSizeY / 2.0f;
            } else {
                _logoTargetPosY = rect.height - Border - _logoSizeY / 2.0f;
            }
        }
    }

    IEnumerator RenderCoroutine() {
        while (true) {
            yield return new WaitForEndOfFrame();
            if (!Active || !LogoTex || !Eyes) {
                continue;
            }
            var eyes = Eyes.GetComponent<StereoEyes>();

            if (eyes != null) {
                UpdatePosition(new Rect(Vector2.zero, eyes.GetEyeRTSize()));

                _screensaverCam.rect = eyes.GetCameraViewportRect(eyes.LeftEyeActive);

                GL.PushMatrix();
                GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
                var yOffset = eyes.GetCameraViewportRect(eyes.LeftEyeActive).y * Screen.height;
                Graphics.DrawTexture(new Rect(_logoPosX - _logoSizeX / 2.0f, _logoPosY - _logoSizeY / 2.0f + yOffset, _logoSizeX, _logoSizeY), LogoTex);

                GL.PopMatrix();
            }

        }
    }
}
}
