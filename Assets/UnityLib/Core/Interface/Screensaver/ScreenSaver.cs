using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Nettle
{


    public class ScreenSaver : MonoBehaviour
    {

        public class HideObject
        {
            public GameObject Obj;
            public bool State;

            public HideObject(GameObject obj, bool state)
            {
                Obj = obj;
                State = state;
            }
        }

        [ConfigField]
        public bool Enabled = true;
        [ConfigField]
        public float IdleTimeBeforeStart = 300.0f;
        public bool SwitchTo2DMode = true;

        public GameObject Eyes;
        public NettleBoxTracking Tracking;
        public bool CheckEyesPosition = true;

        public UnityEvent ScreensaverStarted = new UnityEvent();
        public UnityEvent ScreensaverExited = new UnityEvent();

        [HideInInspector]
        public bool Active;

        private float _screensaverTime = 0.0f;
        private Vector3 _lastEyesPosition = Vector3.zero;

        private List<HideObject> _objToHide = new List<HideObject>();
        private CameraClearFlags _flags;
        private Color _clearColor;
        private LayerMask _camMask;
        private LayerMask _cameraMask;
        protected Camera _screensaverCam;
        private float _nearClipPlane;
        private Camera[] _sceneCams;
        private bool _clipPlaneControllerState;
        private bool _stereoProjMode;

        private void SceneWasLoaded()
        {
        }

        public virtual void OnStart() { }

        private void Start()
        {
            _cameraMask = 1 << 5;
            Init();
            WakeUp();
            OnStart();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneWasLoaded;
        }

        private void OnDisable()
        {

            SceneManager.sceneLoaded -= SceneWasLoaded;
        }

        private void SceneWasLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Init();
            WakeUp();
        }


        protected virtual void OnEyesChanged()
        {

        }

        private void Init()
        {
            StartCoroutine(FindEyes());
        }

        private IEnumerator FindEyes()
        {
            StereoEyes stereoEyes = FindObjectOfType<StereoEyes>();
            while (stereoEyes == null)
            {
                yield return null;
                stereoEyes = FindObjectOfType<StereoEyes>();
            }
            if (!Eyes)
            {
                var newEyes = stereoEyes.gameObject;
                if (Eyes != newEyes)
                {
                    Eyes = newEyes;
                    OnEyesChanged();
                }
            }
            else
            {
                OnEyesChanged();
            }

            if (!Tracking)
            {
                Tracking = SceneUtils.FindObjectIfSingle<NettleBoxTracking>();
            }
        }

        public void WakeUp()
        {
            _screensaverTime = Time.time + IdleTimeBeforeStart;
        }

        public virtual void OnShowScreenSaver() { }

        private void ShowScreensaver()
        {
            if (SwitchTo2DMode)
            {
                _sceneCams = Camera.allCameras.Where(v => v.enabled).ToArray();
                foreach (var sceneCam in _sceneCams)
                {
                    if (Camera.main == sceneCam)
                    {
                        _camMask = sceneCam.cullingMask;
                        sceneCam.cullingMask = _cameraMask.value;
                        _flags = sceneCam.clearFlags;
                        _clearColor = sceneCam.backgroundColor;

                        sceneCam.clearFlags = CameraClearFlags.Color;
                        sceneCam.backgroundColor = Color.black;

                        var clipPlaneController = sceneCam.gameObject.GetComponent<SetCameraClippingPlanes>();
                        if (clipPlaneController != null)
                        {
                            _nearClipPlane = sceneCam.nearClipPlane;
                            sceneCam.nearClipPlane = 0.01f;
                            _clipPlaneControllerState = clipPlaneController.Enabled;
                            clipPlaneController.Enabled = false;
                        }
                        var mp3DProj = sceneCam.gameObject.GetComponent<MotionParallax3D2>();
                        if (mp3DProj != null)
                        {
                            _stereoProjMode = mp3DProj.UseStereoProjection;
                            mp3DProj.UseStereoProjection = false;
                        }
                        _screensaverCam = sceneCam;
                    }
                    else
                    {
                        sceneCam.enabled = false;
                    }

                }
            }

            /*if (!_screensaverCam) {
                _screensaverCam = gameObject.AddComponent<Camera>();

            }*/

            //_screensaverCam.clearFlags = CameraClearFlags.Color;
            //_screensaverCam.backgroundColor = Color.black;
            //_screensaverCam.cullingMask = 0;
            //_screensaverCam.enabled = true;

            _objToHide.Clear();
            GameObject[] objsWithTag;
            try
            {
                objsWithTag = GameObject.FindGameObjectsWithTag("ScreenSaverHide");
            }
            catch
            {
                objsWithTag = new GameObject[0];
            }
            foreach (GameObject o in objsWithTag)
            {
                _objToHide.Add(new HideObject(o, o.activeSelf));
                o.SetActive(false);
            }

            OnShowScreenSaver();
            Active = true;
            ScreensaverStarted.Invoke();
        }

        public virtual void OnHideScreenSaver() { }

        private void HideScreensaver()
        {
            Debug.Log("HideScreensaver()");
            if (SwitchTo2DMode)
            {
                if (_sceneCams != null)
                {
                    foreach (var sceneCam in _sceneCams)
                    {
                        if (sceneCam != null)
                        {
                            if (sceneCam == Camera.main)
                            {
                                sceneCam.cullingMask = _camMask.value;

                                sceneCam.clearFlags = _flags;
                                sceneCam.backgroundColor = _clearColor;

                                var clipPlaneController = sceneCam.gameObject.GetComponent<SetCameraClippingPlanes>();
                                if (clipPlaneController != null)
                                {
                                    sceneCam.nearClipPlane = _nearClipPlane;
                                    clipPlaneController.Enabled = _clipPlaneControllerState;
                                }

                                var mp3DProj = sceneCam.gameObject.GetComponent<MotionParallax3D2>();
                                if (mp3DProj != null)
                                {
                                    mp3DProj.UseStereoProjection = _stereoProjMode;
                                }
                            }
                            sceneCam.enabled = true;
                        }
                    }
                }
            }
            _screensaverCam = null;

            /*if (_screensaverCam) {
                Destroy(_screensaverCam);
            }*/

            foreach (var o in _objToHide)
            {
                if (o.Obj != null)
                {
                    o.Obj.SetActive(o.State);
                }
            }

            OnHideScreenSaver();
            Active = false;
            ScreensaverExited.Invoke();
        }

        private void Update()
        {
            if (!Enabled)
            {
                WakeUp();
                return;
            }

            if (CheckEyesPosition && Eyes)
            {
                Vector3 curEyesPosition = Eyes.transform.position;
                if (curEyesPosition != _lastEyesPosition)
                {
                    _lastEyesPosition = curEyesPosition;
                    WakeUp();
                }
            }

            if (Tracking && Tracking.Active)
            {
                WakeUp();
            }

            if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.001f ||
                Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.001f ||
                Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.001f)
            {

                WakeUp();
            }

            if (Input.anyKey)
            {
                WakeUp();
            }

            if (Active && Time.time < _screensaverTime)
            {
                HideScreensaver();
            }
            else if (!Active && Time.time > _screensaverTime)
            {
                ShowScreensaver();
            }
        }
    }
}