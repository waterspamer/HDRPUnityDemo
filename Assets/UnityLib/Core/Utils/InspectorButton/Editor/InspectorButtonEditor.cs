using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Nettle {
//using System;

[CustomPropertyDrawer (typeof (InspectorButton))]
public class InspectorButtonDrawer : PropertyDrawer {




	public override void OnGUI (UnityEngine.Rect pos, SerializedProperty prop, GUIContent label) {
		SerializedObject O = prop.GetType ().GetField ("m_SerializedObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(prop) as SerializedObject;
		Object[] TargetObjects = O.targetObjects;


		string MethodName = prop.name;
		char[] charsToTrim = {'_'};
		MethodName = MethodName.Trim(charsToTrim);
		MethodInfo TargetMethod = TargetObjects[0].GetType ().GetMethod (MethodName);
		if (TargetMethod == null) {
			GUI.color = Color.red;
			GUI.Label(pos,"Method "+MethodName+" not found.");
			GUI.color = Color.white;
			return;
		}

		if (GUI.Button (pos, MethodName)) {
			foreach (Object o in TargetObjects)
				TargetMethod.Invoke(o,new object[0]);
		}
	
	}

}
}
