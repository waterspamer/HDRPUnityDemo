using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {
    [ExecuteAfter (typeof (UiStereoCameraController))]
    [RequireComponent (typeof (Camera))]
    [RequireComponent (typeof (SetCameraDepthMode))]
    public class StereoCursor : MonoBehaviour {
        [SerializeField]
        private Shader _depthRenderShader;
        [SerializeField]
        private Shader _passThroughShader;
        [SerializeField]
        private float _cursorScale = 0.25f;
        [SerializeField]
        private bool _depthMultiSample = true;

        private Camera _camera;
        private StereoEyes _eyes;
        private Vector3 _worldCursorPosition;
        private Material _depthMaterial;
        private Material _passThroughMaterial;
        private Vector2 _mainCursorPosition;
        private Vector2 _secondaryCursorPosition;
        private bool _cursorOnFarPlane = false;
        private bool _showCursorInit = false;
        public UnityEvent OnCalculate;

        private static StereoCursor _instance;
        public static StereoCursor Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<StereoCursor> ();
                }
                return _instance;
            }
        }

        public static Vector3 WorldPosition {
            get {
                return Instance._worldCursorPosition;
            }
        }

        public static bool IsMainEye {
            get {
                return !Instance._eyes.LeftEyeActive;
            }
        }

        private void Reset () {
            _depthRenderShader = Shader.Find ("Hidden/DepthShader");
            _passThroughShader = Shader.Find ("Hidden/PassThrough");
        }
        private bool _getKey;

        private void Awake () {
#if UNITY_WEBGL            
            Destroy (this);
            return;
#endif
            _eyes = GetComponentInParent<StereoEyes> ();
            if (_eyes == null) {
                Debug.LogError ("Removing StereoCursor because stereo eyes could not be found");
                Destroy (this);
                return;
            }
            _camera = GetComponent<Camera> ();
            _depthMaterial = new Material (_depthRenderShader);
            _passThroughMaterial = new Material (_passThroughShader);
            //Disable the old cursor
#if !UNITY_EDITOR
            Cursor.visible = false;
#endif
        }

        private void Update () {
            _getKey = Input.GetKeyDown (KeyCode.S);
        }
        void OnRenderImage (RenderTexture source, RenderTexture destination) {
            if (!_showCursorInit) {
                Graphics.Blit (source, destination);
                return;
            }

            float defaultDepth = 1;
            if (_eyes.transform.parent != null) {
                Vector3 displayCenterViewSpace = _camera.WorldToViewportPoint (_eyes.transform.parent.position);
                defaultDepth = displayCenterViewSpace.z / _camera.farClipPlane;
            }
            Vector2 mousePos = _camera.ScreenToViewportPoint (Input.mousePosition);
            mousePos = new Vector2 (Mathf.Clamp01 (mousePos.x), Mathf.Clamp01 (mousePos.y));
            if (_eyes.CameraMode == EyesCameraMode.Stereo) {
                if (IsMainEye) {
                    RenderTexture depthRT = new RenderTexture (source.width, source.height, 24, RenderTextureFormat.ARGB32);
                    depthRT.Create ();
                    Graphics.Blit (source, depthRT, _depthMaterial);
                    if (_getKey) {
                        Debug.Log ("SAVING DEPTH TO " + Application.dataPath + "/../Depth.tga");
                        TextureUtils.SaveRenderTexture (depthRT, Application.dataPath + "/../Depth.tga");
                    }
                    Texture2D dt2d;
                    RenderTexture.active = depthRT;
                    Vector2 mousePosClamped = new Vector2 (Mathf.Clamp (mousePos.x * source.width, 0, source.width - 1), Mathf.Clamp ((1 - mousePos.y) * source.height, 0, source.height - 1));

                    float linearDepth;
                    if (!_depthMultiSample) {
                        dt2d = new Texture2D (1, 1, TextureFormat.ARGB32, false);
                        dt2d.ReadPixels (new Rect (mousePosClamped.x, mousePosClamped.y, 1, 1), 0, 0);
                        dt2d.Apply ();
                        linearDepth = dt2d.GetPixel (0, 0).r;
                    } else {
                        ShowCursor.CustomCursor cursor = ShowCursor.Instance.CurrentCursor;
                        Rect rect = GetRectForCursor (mousePosClamped, false);
                        if (rect.x < 0) {
                            rect.width -= rect.x;
                            rect.x = 0;
                        }
                        if (rect.y < 0) {
                            rect.height -= rect.y;
                            rect.y = 0;
                        }
                        if (rect.x + rect.width > source.width - 1) {
                            rect.width = source.width - rect.x;
                        }
                        if (rect.y + rect.height > source.height - 1) {
                            rect.height = source.height - rect.y;
                        }
                        dt2d = new Texture2D (Mathf.FloorToInt (rect.width), Mathf.FloorToInt (rect.height), TextureFormat.ARGB32, false);
                        dt2d.filterMode = FilterMode.Point;
                        dt2d.ReadPixels (rect, 0, 0);
                        float[] samples = new float[5];
                        samples[0] = dt2d.GetPixel (dt2d.width / 2, 0).r;
                        samples[1] = dt2d.GetPixel (dt2d.width / 2, dt2d.height - 1).r;
                        samples[2] = dt2d.GetPixel (0, dt2d.height / 2).r;
                        samples[3] = dt2d.GetPixel (dt2d.width - 1, dt2d.height / 2).r;
                        samples[4] = dt2d.GetPixel (dt2d.width / 2, dt2d.height / 2).r;
                        linearDepth = 1;
                        foreach (float s in samples) {
                            if (s < linearDepth) {
                                linearDepth = s;
                            }
                        }
                        Destroy (dt2d);
                    }
                    RenderTexture.active = null;
                    if (linearDepth == 1) {
                        //Cursor is not over any object, use default depth, which is the depth of MotionParallaxDisplay object in view space
                        linearDepth = defaultDepth;
                    } else if (QualitySettings.activeColorSpace == ColorSpace.Linear) {
                        //Convert color space
                        linearDepth = Mathf.Pow (linearDepth, 2.2f);
                    }
                    _worldCursorPosition = _camera.ViewportToWorldPoint (new Vector3 (mousePos.x, mousePos.y, linearDepth * (_camera.farClipPlane - _camera.nearClipPlane)));
                    /*_mainCursorPosition = _camera.ViewportToScreenPoint(mousePos);
                    _mainCursorPosition.y = Screen.height - _mainCursorPosition.y;*/
                    _mainCursorPosition = Input.mousePosition;
                    depthRT.Release ();
                    Destroy (depthRT);
                } else {
                    /*
                                        if (!_cursorOnFarPlane) {*/
                    Vector2 secondaryCursorViewPosition = _camera.WorldToViewportPoint (_worldCursorPosition);
                    _secondaryCursorPosition = new Vector2 (secondaryCursorViewPosition.x * Screen.width, (secondaryCursorViewPosition.y * _camera.rect.height + _camera.rect.y) * Screen.height);
                    /*}
                    else {
                        _secondaryCursorPosition = _mainCursorPosition;
                        _secondaryCursorPosition.y += Screen.height * _camera.rect.y;

                    }*/
                }
            } else {
                //mousePos.y = 1 - mousePos.y;
                _mainCursorPosition = _camera.ViewportToScreenPoint (mousePos);
                _secondaryCursorPosition = _mainCursorPosition + Vector2.up * Screen.height * _camera.rect.y;
                _worldCursorPosition = _camera.ViewportToWorldPoint (new Vector3 (mousePos.x, mousePos.y, defaultDepth * (_camera.farClipPlane - _camera.nearClipPlane)));
            }
            if (OnCalculate != null) {
                OnCalculate.Invoke ();
            }
            Graphics.Blit (source, destination, _passThroughMaterial);
        }

        private void OnEnable () {
            if (ShowCursor.Instance != null) {
                StartCoroutine (DrawCoroutine ());
                _showCursorInit = true;
            }
        }

        private IEnumerator DrawCoroutine () {
            while (true) {
                yield return new WaitForEndOfFrame ();
                if (ShowCursor.Instance.Show) {
                    GL.PushMatrix ();
                    GL.LoadPixelMatrix (0, Screen.width, Screen.height, 0);
                    if (_eyes.RenderTwoEyesPerFrame || !IsMainEye) {
                        DrawCursor (_secondaryCursorPosition);
                    }
                    if (_eyes.RenderTwoEyesPerFrame || IsMainEye) {
                        DrawCursor (_mainCursorPosition);
                    }
                    GL.PopMatrix ();
                }
            }
        }

        private void DrawCursor (Vector2 position) {
            Graphics.DrawTexture (GetRectForCursor (position), ShowCursor.Instance.CurrentCursor.Texture);
        }

        private Rect GetRectForCursor (Vector2 position, bool flipY = true) {
            if (flipY) {
                position.y = Screen.height - position.y;
            }
            ShowCursor.CustomCursor cursor = ShowCursor.Instance.CurrentCursor;
            return new Rect (position.x - cursor.Hotspot.x * _cursorScale, position.y - cursor.Hotspot.y * _cursorScale, cursor.Texture.width * _cursorScale, cursor.Texture.height * _cursorScale);;
        }
        /*
                private void OnDrawGizmos () {

                    Gizmos.DrawSphere (_worldCursorPosition, 1);
                }*/
    }
}