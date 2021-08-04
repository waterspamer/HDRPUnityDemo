using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace Nettle {

    public delegate void VisibilityListDelegate(List<VisibilityControl> list, string oldTag, string newTag);
    public delegate void VisibilityHideShowDelegate(List<VisibilityControl> hide, List<VisibilityControl> show, string oldTag, string newTag);

    [Serializable]
    public class OnTagChangedEvent : UnityEvent<string> { }

    [ExecuteInEditMode]
    [ExecuteBefore(typeof(DefaultTime))]
    public class VisibilityManager : MonoBehaviour {
        public GameObject Display;
        [HideInInspector]
        public bool EditorModeSlice = false;
        public List<VisibilityControl> Controls { get; private set; }

        private bool _firstSwitch = true;
        private static VisibilityManager _instance;
        private static VisibilityManager Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<VisibilityManager>();
                }
                return _instance;
            }
        }
        private const string BorderNameStart = "border";
        private const string NoTag = "";
        private string _oldTag = "";
        private string _currentTag = "";
        private string _newTag = NoTag;
        private bool _waitForApply;
        private List<VisibilityControl> _showList;
        private List<VisibilityControl> _hideList;

        public event VisibilityListDelegate AppliedHideList;
        public event VisibilityListDelegate AppliedShowList;

        public event Action BeginShowTag;
        public event Action EndShowTag;
        public event Action<string> OnBeginSwitch;
        public string OldTag {
            get => _oldTag;
            set => _oldTag = value;
        }
        public string CurrentTag {
            get => _currentTag;
            private set {
                _currentTag = value;
                OnTagChanged();
            }
        }
        public string NewTag {
            get => _newTag;
            set => _newTag = value;
        }

        /// <summary>
        /// Enlists all visibility tags that exist in the scene
        /// </summary>
        public List<string> AllTags { get; private set; } = new List<string>();
        public List<VisibilityControl> HideList {
            get => _hideList;
            private set => _hideList = value;
        }
        public List<VisibilityControl> ShowList {
            get => _showList;
            private set => _showList = value;
        }

        public OnTagChangedEvent TagChanged = new OnTagChangedEvent();

        protected virtual void OnBeginShowTag() {
            if (BeginShowTag != null) {
                BeginShowTag.Invoke();
            }
        }

        protected virtual void OnTagChanged() {
            if (TagChanged != null) {
                TagChanged.Invoke(_currentTag);
            }
        }


        protected virtual void Awake() {//todo почему тут был старт?
            
        Initialize();
#if !UNITY_EDITOR
        if (Display != null) {
            SetDisplayObjectEnabled(false);
        } else {
            Debug.LogWarning("VisibilityManagerStreaming: mp3d is not set!");
        }
#endif
        }

        void SetDisplayObjectEnabled(bool enable) {
            if (Display != null) {
                Display.gameObject.SetActive(enable);
            }
        }

        protected virtual void Initialize() {
            ShowList = new List<VisibilityControl>();
            HideList = new List<VisibilityControl>();
            UpdateTargets();

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                string noSliceTag = AllTags.Find(x => x.ToLower() == "no_slice");
                if (!string.IsNullOrEmpty(noSliceTag)) {
                    BeginSwitch(noSliceTag);
                }
            }
#endif
        }

        public void UpdateTargets() {
            Controls = FindTargets();
#if UNITY_EDITOR
            AllTags = new List<string>();
            VisibilityZone[] zones = FindObjectsOfType<VisibilityZone>();
            foreach (VisibilityZone zone in zones) {
                if (!AllTags.Contains(zone.VisibilityTag)) {
                    AllTags.Add(zone.VisibilityTag);
                }
            }
            VisibilityControl[] controls = FindObjectsOfType<VisibilityControl>();
            foreach (VisibilityControl control in controls) {
                foreach (string tag in control.Tags) {
                    if (!AllTags.Contains(tag)) {
                        AllTags.Add(tag);
                    }
                }
            }
            AllTags.Sort();
#endif
        }

        public static List<VisibilityControl> FindTargets() {
            return SceneUtils.FindObjectsOfType<VisibilityControl>(true);
        }

        public static void QueryObjects(string visibilityTag, List<VisibilityControl> objects, ref List<VisibilityControl> showList, ref List<VisibilityControl> hideList) {
            showList.Clear();
            hideList.Clear();
            if (objects == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying && !Instance.EditorModeSlice) {
                showList.AddRange(objects);
                return;
            }
#endif
            foreach (var c in objects) {
                if (c.HasTag(visibilityTag)) {
                    showList.Add(c);
                } else
                    hideList.Add(c);
            }
        }

        public void BeginSwitchUnsafe(string newTag) {
            BeginSwitch(newTag);
        }

        public virtual bool BeginSwitch(string newTag, bool forced = false) {
            
            if (!forced) {
                if ((CurrentTag == newTag) && (_newTag == NoTag)) {
                    return false;
                }

                if (_newTag == newTag) {
                    return false;
                }
            }
            _newTag = newTag;

            if (OnBeginSwitch != null)
                OnBeginSwitch.Invoke(_newTag);

            //TODO: hide/show lists by ref
            QueryObjects(newTag, Controls, ref _showList, ref _hideList);

            OnBeginShowTag();

            _waitForApply = true;
            return true;
        }

        public virtual void LateUpdate() {
            if (_firstSwitch) {
                SetDisplayObjectEnabled(true);
                _firstSwitch = false;
            }
            if (_waitForApply) {
                ApplyHideList();
                ApplyShowList();
                OldTag = _currentTag;
                CurrentTag = _newTag;
                _newTag = NoTag;
                ShowList.Clear();
                HideList.Clear();
                _waitForApply = false;
                OnEndShowTag();
            }
        }

        public void ResetTag() {
            _currentTag = "";
        }

        protected virtual void ApplyHideList() {
            if (HideList == null)
                return;
            foreach (VisibilityControl t in HideList.Where(t => t != null)) {
                t.DisableVisibilityFactor("tagVisibility");
            }

            if (AppliedHideList != null) {
                AppliedHideList.Invoke(HideList, OldTag, NewTag);
            }
        }

        protected virtual void ApplyShowList() {
            if (ShowList != null) {
                foreach (VisibilityControl t in ShowList.Where(t => t != null)) {
                    t.EnableVisibilityFactor("tagVisibility");
                }
                if (AppliedShowList != null) {
                    AppliedShowList.Invoke(ShowList, _oldTag, _newTag);
                }
            }
        }

        protected virtual bool CanSwitchTag() {
            return true;
        }
        protected virtual void OnEndShowTag() {
            EndShowTag?.Invoke();
        }
    }


}
