using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Nettle {
    [RequireComponent(typeof(Camera))]
    public class OverlayCameraMixer : MonoBehaviour {

        [HideInInspector]
        [SerializeField]
        private List<OverlayCameraEffect> _cameras;

        private RenderTexture _renderTexture;
        private RenderTexture _renderTexture1;

        private void Start() {
            foreach (OverlayCameraEffect cam in _cameras) {
                cam.SetMainCamera(GetComponent<Camera>());
            }
            
        }
        

        public void OnDestroy() {
            _renderTexture.Release();
            Destroy(_renderTexture);
            if (_renderTexture1 != null)
            {
                _renderTexture1.Release();
                Destroy(_renderTexture1);
            }
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_renderTexture == null) {
                _renderTexture = new RenderTexture(source.width, source.height, 32);
            }

            OverlayCameraEffect[] cameras = _cameras.Where(x => x != null && x.enabled && x.BlendValue>0).ToArray();
            if (cameras.Length == 0) {
                Graphics.Blit(source, destination);                
                return;
            }
            if (cameras.Length > 2 && _renderTexture1==null)
            {
                _renderTexture1 = new RenderTexture(source.width, source.height, 32);
            }
            RenderTexture src = source;
            RenderTexture dest = _renderTexture;            
            for (int i =0; i< cameras.Length; i++) {
                if (i == cameras.Length - 1) {
                    //Render the last camera directly to screen
                    dest = destination;
                }
                cameras[i].RenderOverlay(src, dest);
                if (i == cameras.Length - 1)
                {
                    break;
                }
                RenderTexture swap;
                if (i > 0)
                {
                    swap = src;
                }else
                {
                    swap = _renderTexture1;
                }
                src = dest;
                dest = swap;
            }
        }

    }
}