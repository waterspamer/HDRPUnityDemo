using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nettle {

public class TextureAtlasCreator : EditorWindow {
    private string _rootFolderPath = "";
    private string _modelFileName = "";
    private static TextureAtlasCreator _currentWindow;
    [MenuItem("Nettle/Create Texture Atlases")]
    static void ShowEditor() {
        _currentWindow = CreateInstance<TextureAtlasCreator>();
        _currentWindow.titleContent.text = "Atlas Creator";

        _currentWindow.Show();
    }
    private void OnEnable() {
        maxSize = new Vector2(400, 150);
        minSize = new Vector2(400, 150);
    }
    private void OnGUI() {
        EditorGUILayout.LabelField("To create texture atlases, select the folder containing xml files with atlas descriptions.",EditorStyles.wordWrappedLabel);
        EditorGUILayout.PrefixLabel("Model file name");
        _modelFileName =  EditorGUILayout.TextField(_modelFileName);
        EditorGUILayout.PrefixLabel("XML files folder");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Browse", GUILayout.Width(75))) {
            _rootFolderPath = EditorUtility.OpenFolderPanel("Select root folder", "", "");
        }
        EditorGUILayout.SelectableLabel(_rootFolderPath);
        GUILayout.EndHorizontal();
        if (!string.IsNullOrEmpty(_rootFolderPath)) {
            if (GUILayout.Button("Create")) {
                CreateAtlases();
            }
        }
    }

    private void CreateAtlases() {
        string[] files = Directory.GetFiles(_rootFolderPath);
        TextureAtlasInfo atlasInfo = CreateInstance<TextureAtlasInfo>();
        atlasInfo.ModelFileName = _modelFileName;
        foreach (string path in files) {
            if (Path.GetExtension(path).ToLower() == ".xml") {
                ParseXMLFile(path, ref atlasInfo);
            }
        }
        AssetDatabase.CreateAsset(atlasInfo, "Assets/Maps/"+ _modelFileName + "Atlas.asset");
        AssetDatabase.SaveAssets();
    }

    private void ParseXMLFile(string path, ref TextureAtlasInfo atlasInfo) {
        Stream stream = File.Open(path, FileMode.Open);
        XmlDocument document = new XmlDocument();
        document.Load(stream);
        XmlNodeList atlasNodes = document.SelectNodes("/TextureAtlas");
        foreach (XmlNode atlasNode in atlasNodes) {
            TextureAtlasInfo.Atlas newAtlas = new TextureAtlasInfo.Atlas();
            XmlAttribute attribute = atlasNode.Attributes.GetNamedItem("imagePath") as XmlAttribute;
            if (attribute != null) {
                string textureName = attribute.Value.Substring(0, attribute.Value.LastIndexOf('.'));
                string[] searchResults = AssetDatabase.FindAssets(textureName + " t:Texture2D");
                string assetPath = AssetDatabase.GUIDToAssetPath(searchResults.Where(s => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(s)).Equals(textureName)).FirstOrDefault());
                Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (asset != null) {
                    newAtlas.Texture = asset as Texture2D;
                }
            }
            if (newAtlas.Texture == null) {
                Debug.LogError("Couldn't find atlas image");
                continue;
            }
            int atlasWidth = 0;
            attribute = atlasNode.Attributes.GetNamedItem("width") as XmlAttribute;
            if (attribute != null) {
                int.TryParse(attribute.Value, out atlasWidth);
            }
            int atlasHeight = 0;
            attribute = atlasNode.Attributes.GetNamedItem("height") as XmlAttribute;
            if (attribute != null) {
                int.TryParse(attribute.Value, out atlasHeight);
            }
            if (atlasWidth == 0 || atlasHeight == 0) {
                Debug.LogError("Couldn't determine atlas size");
                continue;
            }
            XmlNodeList spriteNodes = atlasNode.SelectNodes("sprite");
            if (spriteNodes.Count == 0) {
                Debug.LogError("No sprites defined in atlas");
                continue;
            }
            foreach (XmlNode spriteNode in spriteNodes) {
                TextureAtlasInfo.AtlasedObject newObject = new TextureAtlasInfo.AtlasedObject();
                attribute = spriteNode.Attributes.GetNamedItem("n") as XmlAttribute;
                if (attribute == null) {
                    Debug.LogError("Sprite name not defined");
                    continue;
                }
                string pattern = @"(.+)_\[]_(.+)\..+";
                Regex r = new Regex(pattern);
                Match match = r.Match(attribute.Value);
                if (match.Success) {
                    newObject.ObjectName = match.Groups[1].Value;
                    newObject.ImageType = match.Groups[2].Value;
                }
                else {
                    Debug.LogError("Can't parse sprite name");
                    continue;
                }
                int x=-1;
                int y=-1;
                int w=-1;
                int h=-1;
                attribute = spriteNode.Attributes.GetNamedItem("x") as XmlAttribute;
                if (attribute != null) {
                    int.TryParse(attribute.Value, out x);
                }
                attribute = spriteNode.Attributes.GetNamedItem("y") as XmlAttribute;
                if (attribute != null) {
                    int.TryParse(attribute.Value, out y);
                }
                attribute = spriteNode.Attributes.GetNamedItem("w") as XmlAttribute;
                if (attribute != null) {
                    int.TryParse(attribute.Value, out w);
                }
                attribute = spriteNode.Attributes.GetNamedItem("h") as XmlAttribute;
                if (attribute != null) {
                    int.TryParse(attribute.Value, out h);
                }
                if (x < 0 || y < 0 || w < 0 || h < 0) {
                    Debug.LogError("Sprite coordinates not defined");
                    continue;
                }
                newObject.UVOffset = new Vector2(((float)x)/ atlasWidth, (atlasHeight - (float)y - h)/atlasHeight);
                newObject.UVScale = new Vector2(((float)w)/atlasWidth, ((float)h) / atlasHeight);
                newAtlas.Objects.Add(newObject);
            }
            if (newAtlas.Objects.Count > 0) {
                atlasInfo.Atlases.Add(newAtlas);
            }
            else {
                Debug.Log("No objects defined in atlas");
            }
        }
        stream.Close();
    }
}
}
