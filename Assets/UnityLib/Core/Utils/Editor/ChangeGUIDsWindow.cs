using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Nettle {

/// <summary>
/// Need to find and replace the old GUIDs to the new, in the scene
/// </summary>
public class ChangeGUIDsWindow : EditorWindow {

    [MenuItem("Nettle/Replacing Scripts GUIDs")]
    public static void ShowWindow() {
        Init();
    }

    private const float SIZE_X = 550f;
    private const float SIZE_Y = 500f;
    private const float FIRST_COL_WIDTH = 100f;
    private const float SECOND_COL_WIDTH = 330f;
    private const float THIRD_COL_WIDTH = 70f;

    // Edit -> Project Setting -> Editor -> Asset Serialization = Force Text
    private string pathToFile = "Allowed only text format of scene";

    private string objName = "";
    private string indexOfComponent = "";
    private string foundGuid = "";

    private string pathToOldScripts = "";
    private string filenameWithGuid = "";

    private string pathToMetaFile = "";
    private string newGuid = "";

    public static void Init() {
        // Get existing open window or if none, make a new one:
        ChangeGUIDsWindow window = (ChangeGUIDsWindow)EditorWindow.GetWindow(typeof(ChangeGUIDsWindow));
        window.Show();
        window.titleContent.text = "Missing Scripts GUIDs manipulation";
        window.position = new Rect((Screen.currentResolution.width - SIZE_X)/2, (Screen.currentResolution.height - SIZE_Y)/2, SIZE_X, SIZE_Y);
    }

    void OnGUI() {

        DrawSeparator(1);

        DrawInfoLabel();

        DrawSeparator(2);

        DrawFindMissingComonents();

        DrawSeparator(3);

        DrawSelectSceneFileBlock();

        DrawSeparator(3);

        DrawFindGUIDBlock();

        DrawSeparator(3);

        DrawSelectOldScriptsBlock();

        DrawSeparator(3);

        DrawGetGUIDFromMetaBlock();

        DrawSeparator(3);

        DrawReplaceGUIDBlock();
    }

    private void DrawSeparator(int height) {
        for (int i=0; i < height; ++i) {
            EditorGUILayout.Separator();
        }
    }

    private void DrawInfoLabel() {
        GUILayout.Label("Edit -> Project Setting -> Editor -> Asset Serialization = Force Text", EditorStyles.boldLabel);
    }

    private void DrawFindMissingComonents() {
        if (GUILayout.Button("Find missing components in selected gameobjects")) {

            long startTime = DateTime.Now.Ticks;

            FindMissingComponents.Find(Selection.gameObjects);

            TimeSpan delta = new TimeSpan(DateTime.Now.Ticks - startTime);
            Debug.Log("Search Time " + (delta.TotalMilliseconds) + " milliseconds");
        }
    }

    private void DrawSelectSceneFileBlock() {

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Scene File:", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        pathToFile = EditorGUILayout.TextField(pathToFile, GUILayout.Width(SECOND_COL_WIDTH));
        if (GUILayout.Button("Browse", GUILayout.Width(THIRD_COL_WIDTH))) {
            pathToFile = EditorUtility.OpenFilePanelWithFilters("File of scene or prefab", Application.dataPath, new string[] { "Scene", "unity", "Prefab", "prefab" });
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSelectOldScriptsBlock() {

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Old Scripts:", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        pathToOldScripts = EditorGUILayout.TextField(pathToOldScripts, GUILayout.Width(SECOND_COL_WIDTH));
        if (GUILayout.Button("Browse", GUILayout.Width(THIRD_COL_WIDTH))) {
            pathToOldScripts = EditorUtility.OpenFolderPanel("Folder with old scripts", Application.dataPath, "");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Old GUID", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        foundGuid = EditorGUILayout.TextField(foundGuid);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Find script with GUIDs")) {
            FindFilenameInFolder(pathToOldScripts, foundGuid, ref filenameWithGuid);
        }   

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Filename", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        filenameWithGuid = EditorGUILayout.TextField(filenameWithGuid);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFindGUIDBlock() {

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Object Name", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        objName = EditorGUILayout.TextField(objName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Compnt Index", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        indexOfComponent = EditorGUILayout.TextField(indexOfComponent);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Find GUID of missing script in scene file")) {
            int indx;
            if (int.TryParse(indexOfComponent, out indx)) {
                GetGUID(objName, indx, ref foundGuid);
            } else {
                Debug.LogError("Can't parse index");
            }
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Found GUID", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        foundGuid = EditorGUILayout.TextField(foundGuid);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawReplaceGUIDBlock() {

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Old GUID", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        foundGuid = EditorGUILayout.TextField(foundGuid);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("New GUID", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        newGuid = EditorGUILayout.TextField(newGuid);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Replace old GUIDs with new GUIDs")) {
            ReplaceGUIDsInSceneFile();
        }
    }

    private void DrawGetGUIDFromMetaBlock() {

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("New Metafile:", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        pathToMetaFile = EditorGUILayout.TextField(pathToMetaFile, GUILayout.Width(SECOND_COL_WIDTH));
        if (GUILayout.Button("Browse", GUILayout.Width(THIRD_COL_WIDTH))) {
            pathToMetaFile = EditorUtility.OpenFilePanelWithFilters("MetaFile", Application.dataPath, new string[] { "Meta", "meta" });
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Read GUID")) {
            ReadGUIDFromMetafile(pathToMetaFile, ref newGuid);
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("New GUID", EditorStyles.boldLabel, GUILayout.Width(FIRST_COL_WIDTH));
        newGuid = EditorGUILayout.TextField(newGuid);
        EditorGUILayout.EndHorizontal();
    }

    private void ReplaceGUIDsInSceneFile() {
        FileStream file = null;
        StreamReader reader = null;
        StreamWriter writer = null;
        try {
            file = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
            reader = new StreamReader(file);

            string data = reader.ReadToEnd();
            file.Close();

            file = new FileStream(pathToFile, FileMode.Truncate, FileAccess.Write);
            writer = new StreamWriter(file);
            string newdata = data.Replace(foundGuid, newGuid);
            Debug.Log("File Changed: " + !data.Equals(newdata));
            writer.Write(newdata);

        } catch (System.Exception ex) {
            Debug.LogError(ex);
        } finally {
            if (writer != null)
                writer.Close();
            else if (file != null)
                file.Close();
        }
    }

    private void GetGUID(string objectName, int componentIndex, ref string result) {

        FileStream file = null;
        StreamReader reader = null;
        try {
            file = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
            reader = new StreamReader(file);

            string data = reader.ReadToEnd();
            file.Close();

            //find id of missing component
            string FILE_ID = "fileID:";
            int objectNameIndex = data.IndexOf(objectName);
            int startCompIndex = data.LastIndexOf("m_Component", objectNameIndex);
            string allComponentsStr = data.Substring(startCompIndex, objectNameIndex - startCompIndex);
            string[] componentsStrs = allComponentsStr.Split('-');
            string ourComp = componentsStrs[componentIndex+1];
            int startId = ourComp.IndexOf(FILE_ID) + FILE_ID.Length;
            int endId = ourComp.IndexOf("}");
            string idOfComponent = ourComp.Substring(startId, endId - startId);
            idOfComponent = idOfComponent.Trim();

            //find missing component
            int startOfElement = data.IndexOf("&" + idOfComponent);
            int endOfElement = data.IndexOf("---", startOfElement);
            string elemStr = data.Substring(startOfElement, endOfElement - startOfElement);
            string GUID = "guid:";
            int startOfGuid = elemStr.IndexOf(GUID) + GUID.Length;
            int endOfGuid = elemStr.IndexOf(",", startOfGuid);
            string guidOfComponent = elemStr.Substring(startOfGuid, endOfGuid - startOfGuid);
            result = guidOfComponent.Trim();

        } catch (System.Exception ex) {
            Debug.LogError(ex.StackTrace);
        } finally {
            if (file != null)
                file.Close();
        }
    }

    private bool FindFilenameInFolder(string folderPath, string GUID, ref string fileWithGUID) {
        
        FileStream file = null;
        DirectoryInfo dInfo = new DirectoryInfo(folderPath);
        
        try {
            //search in files
            FileInfo[] fileInfos = dInfo.GetFiles();
            foreach (FileInfo fI in fileInfos) {
                file = new FileStream(folderPath + "/" + fI.Name, FileMode.Open, FileAccess.Read);

                string data = (new StreamReader(file)).ReadToEnd();
                file.Close();

                if (data.Contains(GUID)) {
                    fileWithGUID = fI.Name;
                    return true;
                }
            }

            //search in folders
            DirectoryInfo[] directoryInfos = dInfo.GetDirectories();
            foreach (var dI in directoryInfos) {
                if (FindFilenameInFolder(folderPath + "/" + dI.Name, GUID, ref fileWithGUID)) {
                    return true;
                }
            }

        } catch (System.Exception ex) {
            Debug.LogError(ex.StackTrace);
        } finally {
            if (file != null) {
                file.Close();
            }      
        }
        return false;
    }

    private void ReadGUIDFromMetafile(string pathToMfile, ref string filesGUID) {
        FileStream file = null;
        string GSTR = "guid:";
        try {
            file = new FileStream(pathToMfile, FileMode.Open, FileAccess.Read);
            string data = (new StreamReader(file)).ReadToEnd();
            file.Close();

            int startIndx = data.IndexOf(GSTR) + GSTR.Length;
            int endIndx = data.IndexOf("\r\n", startIndx);
            string dGUID = data.Substring(startIndx, endIndx - startIndx);
            filesGUID = dGUID.Trim();

        } catch (System.Exception ex) {
            Debug.LogError(ex);
        } finally {
            if (file != null)
                file.Close();
        }
    }
}
}
