using System.Collections.Generic;

namespace Nettle {

public static class StringUtils {

	public static string PackStrings(string[] src){
		string result = "";
		if (src == null || src.Length == 0) {
			return result;
		}
		foreach (var s in src) {
			if(s == null){
				result += "-1.";
			}else{
				result += s.Length.ToString() + "." + s;
			}
		}
		return result;
	}
	
	public static string[] UnpackStrings(string packed){
		if (string.IsNullOrEmpty (packed)) {
			return null;
		}
		
		List<string> result = new List<string> ();
		while (packed.Contains(".")) {
			var dotId = packed.IndexOf(".");
			var lengthString = packed.Substring(0, dotId);
			var length = int.Parse(lengthString);
			
			if(length > 0){
				result.Add(packed.Substring(dotId + 1, length));
				int offset = dotId + 1 + length;
				packed = packed.Substring(offset, packed.Length - offset);
			}else{
				if(length < 0){
					result.Add(null);
				}else if(length == 0){
					result.Add("");
				}
				int offset = dotId + 1;
				packed = packed.Substring(offset, packed.Length - offset);
			}
		}
		return result.ToArray ();
	}
}
}
