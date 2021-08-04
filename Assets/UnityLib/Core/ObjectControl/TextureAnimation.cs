using UnityEngine;
using System;
using System.Collections.Generic;

namespace Nettle {

[Serializable]
public class TextureAnimationSettings {
    public int MaterialIndex;
    public Vector2 UvAnimationRate;
    public string TextureName;

    public TextureAnimationSettings() {
        MaterialIndex = 0;
        UvAnimationRate = new Vector2(0.0f, 0.0f);
        TextureName = "_MainTex";
    }
}

public class TextureAnimation : MonoBehaviour {
    [HideInInspector]
    public List<TextureAnimationSettings> TexAnimations = new List<TextureAnimationSettings>();
    private Renderer _renderer;
    private int _materialsCount;

    void Start() {
        _renderer = GetComponent<Renderer>();
        if (!_renderer) {
            enabled = false;
            return;
        }
        _materialsCount = _renderer.sharedMaterials.Length;
    }

    void LateUpdate() {
        if (!_renderer.enabled) {
            return;
        }
        for (int i = 0; i < TexAnimations.Count; ++i) {
            if (TexAnimations[i] == null) {
                continue;
            }

#if UNITY_EDITOR
            if (TexAnimations[i].MaterialIndex < 0 || TexAnimations[i].MaterialIndex >= _materialsCount) {
                Debug.LogError(string.Format("{0}: Material index at {1} element is out of range!", gameObject.name, TexAnimations[i].MaterialIndex));
                continue;
            }
#endif

            Material material = _renderer.materials[TexAnimations[i].MaterialIndex];
            Vector2 texOffset = material.GetTextureOffset(TexAnimations[i].TextureName);
            texOffset = new Vector2(Mathf.Repeat(texOffset.x + TexAnimations[i].UvAnimationRate.x * Time.deltaTime, 1),
                Mathf.Repeat(texOffset.y + TexAnimations[i].UvAnimationRate.y * Time.deltaTime, 1));
            material.SetTextureOffset(TexAnimations[i].TextureName, texOffset);
        }
    }
}
}
