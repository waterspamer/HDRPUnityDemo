using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyButtons;
using Nettle;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenshotMaker : MonoBehaviour {

    private enum DrawMode { Free, Ellipse, Arrow }

    private const float CIRCLE_POINT_DENSITY = 6;
    private const float ARROW_LINES_LENGTH = 15;
    private const float WAIT_SCREENSHOT_TIME = 10;

    public MotionParallaxDisplay Display;
    public VisibilityZoneViewer VisibilityZoneViewer;
    public ZoomPanMouseController ZoomPanMouseController;

    public RawImage RawImage;
    public InputField InputField;
    public RawImage TempRawImage;
    public GameObject GUI;
    public GameObject Control;
    private float _timeScaleCache;
    private bool _zoomPanEnableCache;
    private bool _brushDown;
    public GameObject Info;
    public Text ZoneText;
    public Text DisplayText;


    private Vector2 _startDrawFigurePos;
    private Texture2D _emptyTexture;
    private DrawMode _drawMode = DrawMode.Arrow;
    private string _directory;
    private string _currentImagePath;
    private Texture2D _capturedScreenshot;
    private Stack<Texture2D> UndoRecords = new Stack<Texture2D>();

    void Awake() {
        TimeSpan timeSpanElepsed = TimeSpan.FromSeconds(-Time.realtimeSinceStartup);
        DateTime startDateTime = DateTime.Now.Add(timeSpanElepsed);
        _directory = startDateTime.ToString("yyyy-MM-dd_HH.mm");
        SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
    }

    private void SceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode) {
        FindComponents();
    }

    private void FindComponents() {
        if (!ZoomPanMouseController) {
            ZoomPanMouseController = SceneUtils.FindObjectsOfType<ZoomPanMouseController>(true).FirstOrDefault();
        }
        if (!VisibilityZoneViewer) {
            VisibilityZoneViewer = SceneUtils.FindObjectsOfType<VisibilityZoneViewer>(true).FirstOrDefault();
        }
        if (!Display) {
            Display = SceneUtils.FindObjectsOfType<MotionParallaxDisplay>(true).FirstOrDefault();
        }
    }

    public void Make() {
        NEventKey.IsGlobalActive = false;
        UndoRecords.Clear();
        InputField.text = "";
        if (ZoomPanMouseController) {
            _zoomPanEnableCache = ZoomPanMouseController.enabled;
            ZoomPanMouseController.enabled = false;
        }
        StartCoroutine(MakeScreenshotCoroutine());
    }

    public void FocusText() {
        if (!InputField.isFocused) {
            InputField.Select();
        }
    }

    IEnumerator MakeScreenshotCoroutine() {
        yield return new WaitForEndOfFrame();

        _emptyTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        _emptyTexture.wrapMode = TextureWrapMode.Clamp;
        for (int x = 0; x < _emptyTexture.width; x++) {
            for (int y = 0; y < _emptyTexture.height; y++) {
                _emptyTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        _emptyTexture.Apply();
        TempRawImage.texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        TempRawImage.texture.wrapMode = TextureWrapMode.Clamp;
        Graphics.CopyTexture(_emptyTexture, TempRawImage.texture);
        TempRawImage.enabled = false;

        _timeScaleCache = Time.timeScale;
        Time.timeScale = 0f;
        Directory.CreateDirectory(_directory);
        _currentImagePath = _directory + "/" + DateTime.Now.ToString("HH.mm.ss") + ".png";
        Debug.Log("_directory=" + _directory + "::_currentImagePath=" + _currentImagePath);
        yield return StartCoroutine(CaptureScreenshot(_currentImagePath));
        File.Delete(_currentImagePath);
        GUI.SetActive(true);
        Control.SetActive(true);
        InputField.Select();
        Cursor.visible = true;
        RawImage.texture = _capturedScreenshot;

        if (Display) {
            DisplayText.text = "Display: " + Display.transform.position;
        }
        if (VisibilityZoneViewer) {
            ZoneText.text = "Zone: " + VisibilityZoneViewer.ActiveZone.name;
        }

    }

    public void Save() {
        StartCoroutine(SaveCoroutine());
    }

    public void Abort() {
        Close();
    }

    public void Undo() {
        if (UndoRecords.Count > 0) {
            _capturedScreenshot = UndoRecords.Pop();
            RawImage.texture = _capturedScreenshot;
        }
    }

    private void RecordUndo(Texture2D texture) {
        Texture2D recordTex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        recordTex.wrapMode = TextureWrapMode.Clamp;
        recordTex.SetPixels(texture.GetPixels());
        recordTex.Apply();
        UndoRecords.Push(recordTex);
    }

    public IEnumerator CaptureScreenshot(string path) {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        rt.autoGenerateMips = false;
        rt.useMipMap = false;
        rt.isPowerOfTwo = false;
        RenderTexture.active = rt;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        _capturedScreenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        _capturedScreenshot.ReadPixels(new Rect(0, 0, _capturedScreenshot.width, _capturedScreenshot.height), 0, 0);
        _capturedScreenshot.Apply();
    }

    public IEnumerator SaveCoroutine() {
        Info.SetActive(false);
        yield return StartCoroutine(CaptureScreenshot(_currentImagePath));
        Info.SetActive(true);
        //File.WriteAllBytes(_currentImagePath, _capturedScreenshot.EncodeToPNG());

        int skipHeight = 0;
        StereoEyes eyes = SceneUtils.FindObjectsOfType<StereoEyes>(true).First();
        if (eyes && eyes.StereoType == StereoType.FramePacking) {
            skipHeight = (_capturedScreenshot.height - 45) / 2 + 45;
        }
        int height = _capturedScreenshot.height - skipHeight;
        Texture2D textureForSave = new Texture2D(_capturedScreenshot.width, height, TextureFormat.RGBA32, false);
        for (int x = 0; x < _capturedScreenshot.width; x++) {
            for (int y = 0; y < height; y++) {
                textureForSave.SetPixel(x, y, _capturedScreenshot.GetPixel(x, skipHeight + y));
            }
        }
        textureForSave.SetPixels(_capturedScreenshot.GetPixels());
        textureForSave.Apply();
        File.WriteAllBytes(_currentImagePath, textureForSave.EncodeToPNG());
        Close();
    }

    private void Close() {
        Time.timeScale = _timeScaleCache;
        GUI.SetActive(false);
        Control.SetActive(false);
        _capturedScreenshot = null;
        NEventKey.IsGlobalActive = true;
        Cursor.visible = false;
        if (ZoomPanMouseController) {
            ZoomPanMouseController.enabled = true;
        }
    }

    public void DrawingFreeDown() {
        _drawMode = DrawMode.Free;
    }

    public void DrawingEllipseDown() {
        _drawMode = DrawMode.Ellipse;
    }

    public void DrawingArrowDown() {
        _drawMode = DrawMode.Arrow;
    }


    private void StartDrawEllipse() {
        RecordUndo(_capturedScreenshot);
        _startDrawFigurePos = (Vector2)Input.mousePosition;
        TempRawImage.enabled = true;
    }

    private void CompleteDrawEllipse() {
        DrawEllipse(ref _capturedScreenshot);
        TempRawImage.enabled = false;
    }

    private void StartDrawArrow() {
        RecordUndo(_capturedScreenshot);
        _startDrawFigurePos = (Vector2)Input.mousePosition;
        TempRawImage.enabled = true;
    }

    private void CompleteDrawArrow() {

        DrawArrow(ref _capturedScreenshot);
        TempRawImage.enabled = false;
    }



    public void Update() {
        if (_capturedScreenshot) {
            if (Input.GetMouseButtonDown(0)) {
                switch (_drawMode) {
                    case DrawMode.Free:
                        RecordUndo(_capturedScreenshot);
                        _startDrawFigurePos = (Vector2)Input.mousePosition;
                        break;
                    case DrawMode.Ellipse:
                        StartDrawEllipse();
                        break;
                    case DrawMode.Arrow:
                        StartDrawArrow();
                        break;
                }
            }
            if (Input.GetMouseButtonUp(0)) {
                switch (_drawMode) {
                    case DrawMode.Free:
                        break;
                    case DrawMode.Ellipse:
                        CompleteDrawEllipse();
                        break;
                    case DrawMode.Arrow:
                        CompleteDrawArrow();
                        break;
                }
            }

            if (Input.GetMouseButton(0)) {
                switch (_drawMode) {
                    case DrawMode.Free:
                        DrawLine(_capturedScreenshot, (Vector2)Input.mousePosition, _startDrawFigurePos);
                        _startDrawFigurePos = (Vector2)Input.mousePosition;
                        break;
                    case DrawMode.Ellipse:
                        Graphics.CopyTexture(_emptyTexture, TempRawImage.texture);
                        Texture2D texture1 = TempRawImage.texture as Texture2D;
                        DrawEllipse(ref texture1);
                        break;
                    case DrawMode.Arrow:
                        Graphics.CopyTexture(_emptyTexture, TempRawImage.texture);
                        Texture2D texture2 = TempRawImage.texture as Texture2D;
                        DrawArrow(ref texture2);
                        break;
                }
            }
        }
    }

    private void DrawPoint(Texture2D texture2D, float x, float y) {
        texture2D.SetPixel((int)x, (int)y, Color.red);
        texture2D.SetPixel((int)x + 1, (int)y, Color.red);
        texture2D.SetPixel((int)x - 1, (int)y, Color.red);
        texture2D.SetPixel((int)x, (int)y + 1, Color.red);
        texture2D.SetPixel((int)x, (int)y - 1, Color.red);
    }

    private void DrawLine(Texture2D texture, Vector2 from, Vector2 to) {
        Vector2 delta = to - from;
        Vector2 direction = delta.normalized;
        for (int i = 0; i < delta.magnitude; i++) {
            Vector2 pos = from + direction * i;
            DrawPoint(texture, pos.x, pos.y);
        }
        texture.Apply();
    }

    private void DrawEllipse(ref Texture2D texture) {
        Vector2 halfMouseShift = ((Vector2)Input.mousePosition - _startDrawFigurePos) / 2;
        float radius = halfMouseShift.magnitude;
        Vector2 center = _startDrawFigurePos + halfMouseShift;


        float step = 1 / (radius * CIRCLE_POINT_DENSITY);
        float currentAngleR = 0f;

        while (currentAngleR < 2 * Math.PI) {
            float x = center.x + radius * Mathf.Cos(currentAngleR);
            float proportion = halfMouseShift.y / halfMouseShift.x;
            float y = center.y - Mathf.Sign(proportion) * Mathf.Clamp(Mathf.Abs(proportion), 0.1f, 10) * radius * Mathf.Sin(currentAngleR);
            currentAngleR += step;
            DrawPoint(texture, x, y);
        }
        texture.Apply();
    }

    private void DrawArrow(ref Texture2D texture) {
        DrawLine(texture, _startDrawFigurePos, Input.mousePosition);
        Vector2 leftArrowLine = (Vector2)Input.mousePosition + (_startDrawFigurePos - (Vector2)Input.mousePosition).Rotate(30).normalized * ARROW_LINES_LENGTH;
        DrawLine(texture, Input.mousePosition, leftArrowLine);
        Vector2 rightArrowLine = (Vector2)Input.mousePosition + (_startDrawFigurePos - (Vector2)Input.mousePosition).Rotate(-30).normalized * ARROW_LINES_LENGTH;
        DrawLine(texture, Input.mousePosition, rightArrowLine);
    }



}
