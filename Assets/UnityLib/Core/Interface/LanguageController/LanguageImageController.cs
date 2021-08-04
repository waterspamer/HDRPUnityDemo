using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace Nettle {

    [Serializable]
    public class LanguageImageSettings {
        public Language Lang;
        public Sprite Image;
    }

    public class LanguageImageController : MonoBehaviour {

        public LanguageController Manager;
        public Image ImageComponent;
        public SpriteRenderer SpriteRenderer;
        public LanguageImageSettings[] Settings;


        private void Reset() {
            EditroInit();
        }

        private void OnValidate() {
            EditroInit();
        }

        private void EditroInit() {
            if (!Manager) {
                Manager = SceneUtils.FindObjectIfSingle<LanguageController>();
            }

            if (!ImageComponent) {
                ImageComponent = GetComponent<Image>();
            }
            if (!SpriteRenderer) {
                SpriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start() {
            if (!Manager && Manager.OnLanguageChangedEvent == null) { return; }

            SetImage(Manager.GetCurrentLanguage());
            Manager.OnLanguageChangedEvent.AddListener(SetImage);
        }

        private void SetImage(Language lang) {
            var imgSetting = Settings.FirstOrDefault(v => v.Lang == lang);
            if (imgSetting != null) {
                Sprite imageToSet = imgSetting.Image;
                if (imageToSet) {
                    if (ImageComponent) {
                        ImageComponent.sprite = imageToSet;
                    }
                    if (SpriteRenderer) {
                        SpriteRenderer.sprite = imageToSet;
                    }
                }
            }
        }

    }
}
