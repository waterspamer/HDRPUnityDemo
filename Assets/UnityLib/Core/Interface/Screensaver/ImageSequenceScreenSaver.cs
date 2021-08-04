using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nettle {

    [ExecuteAfter(typeof(DefaultTime))]
    public class ImageSequenceScreenSaver : DeprecatedScreenSaver {
        public float UpdateTime = 0.0416666666666667f;
        public string NameFormat = "screensaver_frame_#";
        public int FrameCount = 1647;

        private Texture2D _currentFrame;
        private StereoEyes _stereoEyes;

        private Queue<Frame> _framesQueue = new Queue<Frame>();
        private Queue<LoadingFrame> _loadingFramesQueue = new Queue<LoadingFrame>();
        private int _maxLoadingFrames = 10;

        // private int _curFrameId = 0;

        //private Camera _screensaverCam;
        //private Camera[] _sceneCams;

        //private float _frameTime = 0.0f;
        //private float _targetFrameTime = 0.0f;
        private float _movieStartTime;

        private IEnumerator _applyFrame;
        private IEnumerator _updateFrameQueue;

        private int _savedFps;
        private int _screensaverFPS = 120;


        private const int FrameQueueSize = 72;
        private int _newestFrameInQueue;

        private struct Frame {
            public Frame(Texture2D texture2D, int id) {
                Texture2D = texture2D;
                Id = id;
            }

            public Texture2D Texture2D;
            public int Id;
        }

        private struct LoadingFrame {
            public LoadingFrame(ResourceRequest resourceRequest, int id) {
                ResourceRequest = resourceRequest;
                Id = id;
            }

            public ResourceRequest ResourceRequest;
            public int Id;
        }

        public int GetQueuedFrames() {
            return _framesQueue.Count;
        }

        public override void OnShowScreenSaver() {
            _stereoEyes = FindObjectOfType<StereoEyes>();
            /*
            _sceneCams = Camera.allCameras.Where(v => v.enabled).ToArray();
            foreach (var sceneCam in _sceneCams) {
                sceneCam.enabled = false;
            }

            if (!_screensaverCam) {
                _screensaverCam = gameObject.AddComponent<Camera>();

            }

            _screensaverCam.clearFlags = CameraClearFlags.Color;
            _screensaverCam.backgroundColor = Color.black;
            _screensaverCam.cullingMask = 0;
            _screensaverCam.enabled = true;
            */
            while (_loadingFramesQueue.Count > 0) {
                var frame = _loadingFramesQueue.Dequeue();
                if (frame.ResourceRequest.isDone) {
                    Resources.UnloadAsset(frame.ResourceRequest.asset);
                } else {
                    Debug.LogError("Texture is not loaded!!!!!");
                }
            }

            //preload queue
            _framesQueue.Clear();
            for (int i = 0; i < FrameQueueSize; ++i) {
                _framesQueue.Enqueue(new Frame(Resources.Load<Texture2D>(GetFramePath(i)), i));
            }
            _newestFrameInQueue = _framesQueue.Count - 1;
            _applyFrame = ApplyFrame();

            _savedFps = Application.targetFrameRate;
            Application.targetFrameRate = _screensaverFPS;


            _updateFrameQueue = UpdateFrameQueue();
            StartCoroutine(_updateFrameQueue);
            StartCoroutine(_applyFrame);
            _movieStartTime = Time.realtimeSinceStartup;
        }

        IEnumerator UpdateFrameQueue() {
            while (true) {
                yield return new WaitForEndOfFrame();

                while (_loadingFramesQueue.Count > 0 && _loadingFramesQueue.Peek().ResourceRequest.isDone) {
                    var loadedFrame = _loadingFramesQueue.Dequeue();
                    _framesQueue.Enqueue(new Frame((Texture2D)loadedFrame.ResourceRequest.asset, loadedFrame.Id));
                }

                while (_loadingFramesQueue.Count < _maxLoadingFrames && _framesQueue.Count + _loadingFramesQueue.Count < FrameQueueSize) {
                    int loadingFrameId = _newestFrameInQueue + 1;
                    _loadingFramesQueue.Enqueue(new LoadingFrame(Resources.LoadAsync(GetFramePath(loadingFrameId % FrameCount)), loadingFrameId));
                    _newestFrameInQueue = loadingFrameId;
                }
            }

        }



        public override void OnHideScreenSaver() {
            /*
            if (_sceneCams != null) {
                foreach (var sceneCam in _sceneCams) {
                    if (sceneCam != null) {
                        sceneCam.enabled = true;
                    }
                }
            }

            if (_screensaverCam) {
                Destroy(_screensaverCam);
            }
            */
            StopCoroutine(_updateFrameQueue);
            StopCoroutine(_applyFrame);

            while (_framesQueue.Count != 0) {
                Resources.UnloadAsset(_framesQueue.Dequeue().Texture2D);
            }

            Application.targetFrameRate = _savedFps;
        }

        IEnumerator ApplyFrame() {
            while (true) {
                yield return new WaitForEndOfFrame();
                if (_framesQueue.Count == 0) continue;

                float globalMovieTime = Time.realtimeSinceStartup - _movieStartTime;
                int globalFrameId = Mathf.FloorToInt(globalMovieTime / UpdateTime);

                //unload played textures
                while (_framesQueue.Count > 1 && _framesQueue.Peek().Id < globalFrameId) {
                    Resources.UnloadAsset(_framesQueue.Dequeue().Texture2D);
                    /*if (_framesQueue.Peek().Id != globalFrameId) {
                        Debug.LogFormat(
                            "Video lag!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\r\n" +
                            "_framesQueue.Count={0}::_framesQueue.Peek().Id={1}::globalFrameId={2}::globalMovieTime={3}",
                            _framesQueue.Count, _framesQueue.Peek().Id, globalFrameId, globalMovieTime);
                    } else {
                        Debug.LogFormat(
                            "New frame:: _framesQueue.Count={0}::_framesQueue.Peek().Id={1}::globalFrameId={2}::globalMovieTime={3}",
                            _framesQueue.Count, _framesQueue.Peek().Id, globalFrameId, globalMovieTime);
                    }*/
                }
                _currentFrame = _framesQueue.Peek().Texture2D;
                RenderScreensaver();
            }
        }

        string GetFramePath(int frame) {
            var frameDigitCount = (FrameCount.ToString()).Length;
            var nextFrameName = NameFormat.Replace("#", frame.ToString("D" + frameDigitCount));
            return "ScreenSaverMovie/" + nextFrameName;
        }

        private void RenderScreensaver() {
            if (!Active) { return; }

            if (_currentFrame != null) {
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
                
                Graphics.DrawTexture(new Rect(0,  Screen.height - _stereoEyes.GetEyeRTSize().y, Screen.width, _stereoEyes.GetEyeRTSize().y), _currentFrame);
                Graphics.DrawTexture(new Rect(0, 0, Screen.width, _stereoEyes.GetEyeRTSize().y), _currentFrame);

                GL.PopMatrix();
            } else {
                //Debug.Log("_currentFrame == null");
            }
        }
    }
}
