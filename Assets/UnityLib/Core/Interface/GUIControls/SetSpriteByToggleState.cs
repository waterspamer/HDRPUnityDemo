using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Nettle {

    public class SetSpriteByToggleState : MonoBehaviour {
        public Sprite OnSprite;
        public Sprite OffSprite;
        [SerializeField]
        private Toggle _toggle;
        [SerializeField]
        private Image _targetImage;

        private void OnValidate() {
            EditorInit();
        }

        private void Reset() {
            EditorInit();
        }

        private void EditorInit() {
            if (_toggle == null) {
                _toggle = GetComponent<Toggle>();
            }
            if (_targetImage == null && _toggle != null) {
                _targetImage = _toggle.targetGraphic as Image;
            }

        }

#if UNITY_EDITOR
        [EasyButtons.Button]
        public void TryFindSprites_Editor() {
            if (_targetImage && _targetImage.sprite != null && (OnSprite == null || OffSprite == null)) {
                string spriteName = Regex.Replace(_targetImage.sprite.name, @"(_on|_off|_active|_inactive)$", "");

                string path = UnityEditor.AssetDatabase.GetAssetPath(_targetImage.sprite);
                string extension = Path.GetExtension(path);
                string[] files = Directory.GetFiles(Path.GetDirectoryName(path), "*" + extension);
                string onRegex = spriteName + @"(_on|_active)\" + extension + @"$";
                string offRegex = spriteName + @"(_off|_inactive)\" + extension + @"$";
                string anyRegex = spriteName + extension + @"$";
                var possibleOnSprites = files.Where(v => Regex.Match(Path.GetFileName(v), onRegex).Success).ToList();
                var possibleOffSprites = files.Where(v => Regex.Match(Path.GetFileName(v), offRegex).Success).ToList();
                string anySpritePath = files.FirstOrDefault(v => Regex.Match(Path.GetFileName(v), anyRegex).Success);

                string onSpritePath = string.Empty;
                string offSpritePath = string.Empty;
                if (possibleOnSprites.Count > 0) {
                    onSpritePath = possibleOnSprites[0];
                } else {
                    onSpritePath = anySpritePath;
                }
                if (possibleOffSprites.Count > 0) {
                    offSpritePath = possibleOffSprites[0];
                } else {
                    offSpritePath = anySpritePath;
                }

                if (OnSprite == null && !string.IsNullOrEmpty(onSpritePath)) {
                    OnSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(onSpritePath);
                }
                if (OffSprite == null && !string.IsNullOrEmpty(offSpritePath)) {
                    OffSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(offSpritePath);
                }

            }
        }
#endif

        public virtual Sprite GetOnSprite() {
            return OnSprite;
        }

        public virtual Sprite GetOffSprite() {
            return OffSprite;
        }



        private void Awake() {
            if (_toggle == null) {
                _toggle = GetComponent<Toggle>();
            }
            if (_toggle != null) {
                if (_targetImage == null) {
                    _targetImage = _toggle.targetGraphic as Image;
                }
                _toggle.onValueChanged.AddListener(SetColorByState);
                SetColorByState(_toggle.isOn);
            }
        }

        public void SetColorByState(bool state) {
            if (_targetImage == null) { return; }

            _targetImage.sprite = state ? GetOnSprite() : GetOffSprite();
        }

        public void Refresh() {
            if (_toggle != null) {
                SetColorByState(_toggle.isOn);
            }
        }
    }
}
