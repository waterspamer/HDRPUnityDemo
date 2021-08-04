using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class MaterialInstance {
    private Material _originalMaterial;
    public Material InstancedMaterial { get; private set; }
    private Texture2D _texture;

    public MaterialInstance(Material originalMaterial, Texture2D texture) {
        _originalMaterial = originalMaterial;
        InstancedMaterial = Object.Instantiate(_originalMaterial);
        InstancedMaterial.name = originalMaterial.name + "#" + AtlasedMaterialInstancer.Instance.InstanceId;
        _texture = texture;
    }

    public bool Compare(Material originalMaterial, Texture2D texture) {
        return _originalMaterial == originalMaterial && _texture == texture;
    }
}

public class AtlasedMaterialInstancer{
    private List<MaterialInstance> _instances = new List<MaterialInstance>();
    public int InstanceId { get; private set; }

    private static AtlasedMaterialInstancer _instance;
    public static AtlasedMaterialInstancer Instance {
        get {
            if (_instance == null) {
                _instance = new AtlasedMaterialInstancer();
            }
            return _instance;
        }
    }

    private AtlasedMaterialInstancer() {
        InstanceId = 1;
    }

    public Material GetMaterial(Material originalMaterial, Texture2D texture) {
        MaterialInstance selectedInstance = null;
        foreach (MaterialInstance instance in _instances) {
            if (instance.Compare(originalMaterial, texture)) {
                selectedInstance = instance;
                break;
            }
        }
        if (selectedInstance == null) {
            selectedInstance = new MaterialInstance(originalMaterial, texture);
            _instances.Add(selectedInstance);
            InstanceId++;
        }
        return selectedInstance.InstancedMaterial;
    }
}
}
