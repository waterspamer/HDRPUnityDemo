using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nettle {
    public class ImageByLoadingScene : MonoBehaviour {
        [System.Serializable]
        public class ImageForScenes {
            public List<string> Names;
            public Sprite Image;
        }

        public Image ImageComponent;
        public List<ImageForScenes> Images;

        private void Reset() {
            ImageComponent = GetComponent<Image>();
        }

        void Start() {
            if (ImageComponent != null) {
                ImageForScenes image = Images.Find(x => x.Names!=null && x.Names.Contains(LoadScreenManager.LastLoadedScene));
                if (image != null) {
                    ImageComponent.sprite = image.Image;
                }
            }
        }
        
    }
}
