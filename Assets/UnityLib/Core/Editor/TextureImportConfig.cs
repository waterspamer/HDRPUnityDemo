using UnityEngine;
using UnityEditor;

namespace Nettle {

public class TextureImportConfig : EditorWindow {
	
	[MenuItem("Nettle/TextureImportConfig")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(TextureImportConfig));
	}
	

	static Object[] GetSelectedTextures() 
	{ 
		return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets); 
	}

	static int[] sizeValues;
	static string[] sizeNames;
	static int sizesCount = 12;
	static int basePower = 2;


	void OnGUI()
	{
		if(sizeValues == null || sizeValues.Length != sizesCount){
			sizeValues = new int[sizesCount];
			sizeNames = new string[sizesCount];
			for(int i = 0; i < sizesCount; ++i){
				sizeValues[i] = (int)Mathf.Pow(2, i + basePower);
				sizeNames[i] = sizeValues[i].ToString();
			}
		}


		var textures = GetSelectedTextures();
		if(textures == null || textures.Length == 0){
			EditorGUILayout.LabelField("Nothing selected");
			return;
		}

		TextureImporter[] importers = new TextureImporter[textures.Length];
		for(int i = 0; i < textures.Length; ++i){
			string path = AssetDatabase.GetAssetPath(textures[i]); 
			importers[i] = AssetImporter.GetAtPath(path) as TextureImporter; 
		}


		//size popup
		EditorGUI.BeginChangeCheck ();
		int selectedSize = importers[0].maxTextureSize;
		bool mixed = false;
		for(int i = 1; i < importers.Length; ++i){
			if(importers[i].maxTextureSize != selectedSize){
				mixed = true;
				break;
			}
		}
		EditorGUI.showMixedValue = mixed;
		selectedSize = EditorGUILayout.IntPopup("Max Size", selectedSize, sizeNames, sizeValues);
		if (EditorGUI.EndChangeCheck ()){
			foreach(var importer in importers){
				importer.maxTextureSize = selectedSize;
			}
		}



        //deprecated
		//EditorGUIUtility.LookLikeControls();
		//EditorGUI.indentLevel = 0;
		//Vector3 position = EditorGUILayout.Vector3Field("Position", view.pivot);
		//Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rotation", view.rotation.eulerAngles);
        //deprecated
		//EditorGUIUtility.LookLikeInspector();
	
		/*EditorGUI.BeginChangeCheck ();
		EditorGUI.showMixedValue = mixed;
		bool tmp = EditorGUILayout.Toggle(b);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck ()){
			mixed = false;
			b = tmp;
		}*/
		
		if(GUILayout.Button("Reimport selected")){
			for(int i = 0; i < textures.Length; ++i){
				string path = AssetDatabase.GetAssetPath(textures[i]); 
				AssetDatabase.ImportAsset(path);  
			}
			/*Object[] textures = GetSelectedTextures(); 
			Selection.objects = new Object[0];
			foreach (Texture2D texture in textures)  {
				string path = AssetDatabase.GetAssetPath(texture); 
				TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
				textureImporter.maxTextureSize = 8192;	
				AssetDatabase.ImportAsset(path); 
				Debug.Log("Selected texture: " + texture.name);
			}*/
		}
		
		if (GUI.changed)
		{
		
		}
	}
}
}
