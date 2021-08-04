using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.Rendering;

[InitializeOnLoad]
public class PostprocessingStack2Patcher {
    /// <summary>
    /// Post Processing Stack 2.0 tends to screw up our image by resetting camera projection matrix every frame.
    /// That's why we have to manually comment out all ResetProjectionMatrix() calls whenever the package is imported.
    /// We also have to override the default Depth-Normal texture rendering shader to work with custom vertex shaders
    /// </summary>
    [MenuItem("Nettle/Patch PostProcessingStack2.0")]    
    public static void ForcePatch(){
        Patch(true);
    }

    private static void Patch(bool force) {
        return;
        string folderPath = Application.dataPath + "/../Library/PackageCache";
        string[] packages = Directory.GetDirectories(folderPath);
        if (packages.Length == 0) {
            return;
        }
        try{
            folderPath = packages.Where(x => x.Contains("com.unity.postprocessing")).First();
        }
        catch{
            return;
        }
        folderPath += "/PostProcessing/Runtime";
        string infoFilePath = folderPath + "/PatchInfo.txt";
        if(!force && File.Exists(infoFilePath)){
            //Already patched
            return;
        }
        File.WriteAllText(infoFilePath, "Patched by Nettle. Removed automatic projection matrix reset in each frame.");
        File.WriteAllText(infoFilePath + ".meta","");//Create meta file, otherwise Unity will give error message
        string scriptPath =  folderPath + "/PostProcessLayer.cs";
        if (!File.Exists(scriptPath)) {
            Debug.LogError("Couldn't find PostrProcessLayer script");
            return;
        }
        
        string code = File.ReadAllText(scriptPath);
        code = code.Replace("m_Camera.ResetProjectionMatrix()", "//m_Camera.ResetProjectionMatrix()");
        File.SetAttributes(scriptPath, File.GetAttributes(scriptPath) & ~FileAttributes.ReadOnly);
        File.WriteAllText(scriptPath, code);
        AssetDatabase.Refresh();
        Debug.Log("Patched successfully");

        GraphicsSettings.SetShaderMode(BuiltinShaderType.DepthNormals, BuiltinShaderMode.UseCustom);
        GraphicsSettings.SetCustomShader(BuiltinShaderType.DepthNormals, Shader.Find("Hidden/Nettle-DepthNormalsTexture"));
    }

    //Since 2019.2 we patch automatically, because Unity has seemingly added automatic package restoration on each editor restart
    static PostprocessingStack2Patcher(){
        Patch(false);
    }
}
