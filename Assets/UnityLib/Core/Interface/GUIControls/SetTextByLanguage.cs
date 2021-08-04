using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Nettle {
    [Serializable]
    public class LanguageTextSettings {
        public Language Lang;
        [TextArea]
        public string Text;
    }

    public class SetTextByLanguage : MonoBehaviour {
        public Language DefaultLaguage = Language.Ru;
        public LanguageTextSettings[] Texts;

        private Language _currentLanuage = Language.Ru;
        private LanguageController _languageController;

        [SerializeField]
        private Text _text;

        private void Reset() {
            EditorInit();
        }

        private void OnValidate() {
            EditorInit();
        }

        private void EditorInit() {
            if (_text == null) {
                _text = GetComponent<Text>();
            }
        }

        private void Start() {
            _languageController = FindObjectOfType<LanguageController>();
            if (_languageController != null) {
                _currentLanuage = _languageController.GetCurrentLanguage();
                _languageController.OnLanguageChangedEvent.AddListener(LanguageChanged);
                Refresh();
            } else {
                _currentLanuage = DefaultLaguage;
                Debug.LogWarning("Language controller not found!");
            }
        }

        private void LanguageChanged(Language language) {
            _currentLanuage = language;
            Refresh();
        }

        private void OnDestroy() {
            if (_languageController != null) {
                _languageController.OnLanguageChangedEvent.RemoveListener(LanguageChanged);
            }
        }

        public void Refresh() {
            if (_text != null) {
                var languageTextSettings = Texts.FirstOrDefault(v => v.Lang == _currentLanuage);
                if (languageTextSettings != null) {
                    _text.text = languageTextSettings.Text;
                }

            }
        }


    }
}
