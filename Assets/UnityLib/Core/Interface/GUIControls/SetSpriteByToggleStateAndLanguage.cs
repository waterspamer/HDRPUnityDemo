using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class SetSpriteByToggleStateAndLanguage : SetSpriteByToggleState {
        public Language DefaultLaguage = Language.Ru;
        public LanguageImageSettings[] LanguageSpritesOn;
        public LanguageImageSettings[] LanguageSpritesOff;

        private Language _currentLanuage = Language.Ru;
        private LanguageController _languageController;

        public override Sprite GetOnSprite() {
            if (_currentLanuage == DefaultLaguage) {
                return OnSprite;
            }
            else {
                foreach (LanguageImageSettings image in LanguageSpritesOn) {
                    if (image.Lang == _currentLanuage) {
                        return image.Image;
                    }
                }
                return OnSprite;
            }
        }

        public override Sprite GetOffSprite() {
            if (_currentLanuage == DefaultLaguage) {
                return OffSprite;
            }
            else {
                foreach (LanguageImageSettings image in LanguageSpritesOff) {
                    if (image.Lang == _currentLanuage) {
                        return image.Image;
                    }
                }
                return OffSprite;
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

    }
}