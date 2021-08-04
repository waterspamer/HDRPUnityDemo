using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Nettle {

    public class LanguageInfoController : MonoBehaviour {
        [SerializeField]
        private bool _shown = true;

        [Serializable]
        public struct LangugaeInfoParent {
            public Language Language;
            public GameObject InfoParent;
            public bool AnyLanguage;
        }
        public LangugaeInfoParent[] InfoParents;

        private List<LangugaeInfoParent> _allInfoObjects = new List<LangugaeInfoParent>();

        public UnityEvent OnShowInfo;
        public UnityEvent OnHideInfo;

        private Language _lang = Language.Ru;

        private static LanguageInfoController _instance;
        public static LanguageInfoController Instance {
            get {
                return _instance;
            }
        }

        private void Awake() {
            FindInfoObjects();
            _instance = this;
        }

        private void Start() {
            ShowLanguageInfo();
            InvokeEvent();
            if (!_shown) {
                DisableAll();
            }
        }

        public void RefreshInfoObjects() {
            FindInfoObjects();
            if (_shown) {
                ShowLanguageInfo();
            }
            else {
                DisableAll();
            }
        }

        private void FindInfoObjects() {
            _allInfoObjects.Clear();
            _allInfoObjects.AddRange(InfoParents);
            LocalizedInfoObject[] dynamicObjects = FindObjectsOfType<LocalizedInfoObject>();
            foreach (LocalizedInfoObject obj in dynamicObjects) {
                _allInfoObjects.Add(obj.InfoParent);
            }
        }

        public void AddLocalizedInfoObject(LangugaeInfoParent obj) {
            if (!_allInfoObjects.Contains(obj)) {
                _allInfoObjects.Add(obj);
                ShowInfoParent(obj);
            }
        }

    public void SetLanguage(Language lang) {
        _lang = lang;
        ShowLanguageInfo();
    }

    public void ToggleInfoVisibility() {
        ToggleInfoVisibility(!_shown);
    }

    public void ToggleInfoVisibility(bool on) {
        _shown = on;
        if (_shown) {
            ShowLanguageInfo();
        } else {
            DisableAll();
        }
        InvokeEvent();
    }

        private void ShowLanguageInfo() {
            if (!_shown) { return; }

            foreach (var value in _allInfoObjects) {
                ShowInfoParent(value);
            }
        }

        private void ShowInfoParent(LangugaeInfoParent parent) {
            if (parent.InfoParent != null) {
                parent.InfoParent.SetActive(parent.Language == _lang || parent.AnyLanguage);
            }
        }

    private void DisableAll() {
        foreach (var value in _allInfoObjects) {
            value.InfoParent.SetActive(false);
        }
    }

    private void InvokeEvent() {
        if (_shown && OnShowInfo != null) {
            OnShowInfo.Invoke();
            Debug.Log("Shown");
        } else if (!_shown && OnHideInfo != null) {
            OnHideInfo.Invoke();
            Debug.Log("Hided");
        }
    }

}
}
