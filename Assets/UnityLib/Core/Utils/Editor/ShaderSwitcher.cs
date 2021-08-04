using System;
using UnityEditor;
using UnityEngine;

public class ShaderSwitcher {

    [MenuItem("CONTEXT/Material/SwitchShader", true, 10000)]
    private static bool SwitchShaderValidate(MenuCommand command) {
        Material material = (Material)command.context;
        string name = material.shader.name;
        return name == "Standard" || name == "Standard (Specular setup)";
    }

    [MenuItem("CONTEXT/Material/SwitchShader", false, 10000)]
    private static void SwitchShader(MenuCommand command) {
        Material material = (Material)command.context;
        Debug.Log(material.shader.name);

        string prefix = "";
        string body = "Standard";
        string postfix = "";
        float _Glossiness = 0f;

        if (material.shader.name == "Standard") {
            prefix = "MR_";
            _Glossiness = material.GetFloat("_Glossiness");
        }


        if (material.shader.name == "Standard (Specular setup)") {
            prefix = "SS_";
            if (material.GetTexture("_SpecGlossMap")) {
                _Glossiness = material.GetFloat("_GlossMapScale");
            } else {
                _Glossiness = material.GetFloat("_Glossiness");
            }
        }


        //Opaque
        if (Math.Abs(material.GetFloat("_Mode")) < 0.001f) {
            postfix = "";
        }

        //Cutout
        if (Math.Abs(material.GetFloat("_Mode") - 1) < 0.001f) {
            postfix = "_Clip";
        }

        //Fade
        if (Math.Abs(material.GetFloat("_Mode") - 2) < 0.001f) {
            postfix = "_Fade";
        }

        //Transparent
        if (Math.Abs(material.GetFloat("_Mode") - 3) < 0.001f) {
            postfix = "_Transparent";
        }


        Undo.RecordObject(material, "SwitchShader");
        material.shader = Shader.Find("Nettle/Generated/" + prefix + body + postfix);

        material.SetFloat("_Glossiness", _Glossiness);
        material.SetFloat("_Roughness", 1 - _Glossiness);
    }
}