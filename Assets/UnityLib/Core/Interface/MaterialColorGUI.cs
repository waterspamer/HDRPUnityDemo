using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.Linq;
using System.IO;

public class MaterialColorGUI : MonoBehaviour
{
    private const string _filePath = "MaterialData.xml";


    public string FilePath{
        get
        {
            return Application.streamingAssetsPath + "/" + _filePath;
        }
    }
    [System.Serializable]
    public class EditableMaterial
    {
        public string NameKey;
        public Material MaterialToEdit;
        public string VisibleName;
        public string PropertyName;
        public Color Color;
        public Color DefaultColor;

        public void Apply()
        {
            MaterialToEdit.SetColor(PropertyName, Color);
        }

        public void RestoreDefault()
        {
            Color = DefaultColor;
            Apply();
        }

        public void Read()
        {
            Color = MaterialToEdit.GetColor(PropertyName);
            DefaultColor = Color;
        }
    }

    public struct EditableMaterialData
    {
        public string NameKey;
        public Color Color;
    }

    public List<EditableMaterial> Materials;
    public Dropdown MaterialDropdown;
    public Slider RSlider;
    public Slider GSlider;
    public Slider BSlider;
    public Image ColorSample;
    public Text ColorText;
    private EditableMaterial _selectedMaterial;

    private void OnEnable()
    {
        LoadData();
        UpdateGUI();
    }

    private void OnDisable()
    {
        foreach (EditableMaterial mat in Materials)
        {
            mat.RestoreDefault();
        }
    }

    public void UpdateGUI(){
        MaterialDropdown.ClearOptions();
        foreach (EditableMaterial mat in Materials)
        {
            mat.Read();
            MaterialDropdown.options.Add(new Dropdown.OptionData(mat.VisibleName));
        }
        MaterialDropdown.value = 0;
        MaterialSelected(0);
    }

    
    public void UpdateGUISliders()
    {
        RSlider.value = _selectedMaterial.Color.r;        
        GSlider.value = _selectedMaterial.Color.g;        
        BSlider.value = _selectedMaterial.Color.b;
        UpdateGUIValues();
    }

    public void UpdateGUIValues()
    {
        ColorSample.color = _selectedMaterial.Color;
        ColorText.text = Mathf.RoundToInt(_selectedMaterial.Color.r * 255) + " " + Mathf.RoundToInt(_selectedMaterial.Color.g * 255) + " " + Mathf.RoundToInt(_selectedMaterial.Color.b * 255) + "\n";
        ColorText.text += ColorUtility.ToHtmlStringRGB(_selectedMaterial.Color);
    }


    public void MaterialSelected(int id)
    {
        if (id >= 0 && id < Materials.Count)
        {
            _selectedMaterial = Materials[id];
        }
        UpdateGUISliders();
    }

    public void LoadData()
    {
        if (!File.Exists(FilePath))
        {
            return;
        }
        Stream input = File.Open(FilePath, FileMode.Open);
        XmlSerializer serializer = new XmlSerializer(typeof(EditableMaterialData[]));

        object deserializedSettings = null;
        try
        {
            deserializedSettings = serializer.Deserialize(input);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error loading material settings: " + ex.Message);
        }
        input.Close();
        EditableMaterialData[] loadedData = deserializedSettings as EditableMaterialData[];
        if (loadedData == null)
        {
            return;
        }
        foreach( EditableMaterialData data in loadedData)
        {
            EditableMaterial material = Materials.Find(x => x.NameKey == data.NameKey);
            if (material != null)
            {
                material.Color = data.Color;
                material.Apply();
            }
        }
    }

    public void SaveData()
    {
        string dirPath = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
        EditableMaterialData[] data = Materials.Select(x => new EditableMaterialData() { NameKey = x.NameKey, Color = x.Color }).ToArray();
        XmlSerializer serializer = new XmlSerializer(typeof(EditableMaterialData[]));
        Stream output = File.Open(FilePath, FileMode.Create);
        serializer.Serialize(output, data);
        output.Close();
        foreach (EditableMaterial mat in Materials)
        {
            mat.Read();
        }
    }

    public void SetR(float value)
    {
        _selectedMaterial.Color.r = value;
        _selectedMaterial.Apply();
        UpdateGUIValues();
    }
    public void SetG(float value)
    {
        _selectedMaterial.Color.g = value;
        _selectedMaterial.Apply();
        UpdateGUIValues();
    }
    public void SetB(float value)
    {
        _selectedMaterial.Color.b = value;
        _selectedMaterial.Apply();
        UpdateGUIValues();
    }
}
