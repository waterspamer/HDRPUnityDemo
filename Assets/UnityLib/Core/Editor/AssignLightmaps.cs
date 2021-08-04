using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Nettle {

public class AssignLightmaps {
	[MenuItem("Nettle/Assign Lightmaps")]
	public static void ShowWindow()
	{	
		string lightmapsPath = Application.dataPath + "/Model/Lightmaps";
		string[] lightmaps = Directory.GetFiles (lightmapsPath);

		lightmaps = lightmaps.Where (v => !v.EndsWith (".meta")).ToArray ();

		foreach (var src in lightmaps) {
			var filename = Path.GetFileNameWithoutExtension(src);
			var objName = filename.Substring(0, filename.IndexOf("[") - 1);

			var obj = GameObject.Find(objName);
			if (obj != null) {
				var lm = obj.GetComponent<LightMap>();
				if (lm != null) {
					var id = src.IndexOf("Assets");
					var assetPath = src.Substring(id, src.Length - id);
					//Debug.Log (assetPath);
					lm.Lightmap = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof (Texture2D));
					lm.SetLightmap();
				}
			}
		}


		Debug.Log ("Assignment done");
	}
	
}
}
