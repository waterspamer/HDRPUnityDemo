using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Nettle {

public class ScriptOrderEditorNode {
    private Rect _windowRect;
    public Rect WindowRect {
        get {
            return new Rect(_windowRect.x + ExecutionOrderNodeEditor.ScrollPosition.x, _windowRect.y + ExecutionOrderNodeEditor.ScrollPosition.y, _windowRect.width, _windowRect.height);
        }
        set {
            _windowRect = value;
            _windowRect.position -= ExecutionOrderNodeEditor.ScrollPosition;
        }
    }
    public MonoScriptInfo AssociatedScript;
}

public class ExecutionOrderNodeEditor : EditorWindow{

    private List<ScriptOrderEditorNode> _nodes = new List<ScriptOrderEditorNode>();

    [MenuItem("Window/Script Execution Order Graph")]
    static void ShowEditor() {
        ExecutionOrderNodeEditor editor = GetWindow<ExecutionOrderNodeEditor>();
        editor.Init();
    }

    // Use this for initialization
    public void Init () {
        SyncNodes();
        titleContent.text = "Script Order";
        ExecutionOrderManager.Instance.OnChangeOrder += SyncNodes;
    }

    public static Vector2 ScrollPosition = Vector2.zero;

    private void OnGUI() {
        foreach (ScriptOrderEditorNode node in _nodes) {
            foreach (MonoScriptInfo script in node.AssociatedScript.After) {
                ScriptOrderEditorNode otherNode = _nodes.Where(x => x.AssociatedScript.Equals(script)).First();
                if (otherNode != null) {
                    DrawNodeCurve(node.WindowRect, otherNode.WindowRect);
                }
            }
        }
        BeginWindows();
        int i = 0;
        foreach (ScriptOrderEditorNode node in _nodes) {
            node.WindowRect = GUI.Window(i, node.WindowRect, DrawNodeWindow, node.AssociatedScript.ScriptType.ToString());
            i++;
        }
        EndWindows();
        if (Event.current.type == EventType.MouseDrag) {
            ScrollPosition += Event.current.delta;
            Repaint();
        }
    }

    void DrawNodeWindow(int id) {
        GUILayout.BeginHorizontal();
        GUILayout.Label(_nodes[id].AssociatedScript.ExecuteTime.ToString());
        if (_nodes[id].AssociatedScript.ScriptType != typeof(DefaultTime)) {
            if (GUILayout.Button("Select")) {
                string[] assets = AssetDatabase.FindAssets(_nodes[id].AssociatedScript.ScriptType.ToString() + " t:Script");
                string assetPath = AssetDatabase.GUIDToAssetPath(assets.Where(s => System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(s)).Equals(_nodes[id].AssociatedScript.ScriptType.ToString())).FirstOrDefault());
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
            }
        }
        GUILayout.EndHorizontal();
        GUI.DragWindow();
    }

    void DrawNodeCurve(Rect start, Rect end) {
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null,1);
        
        float arrowScale = 10;
        Handles.color = Color.black;
        Handles.DrawAAPolyLine(3, new Vector3[3] {startPos + Vector3.up * 0.25f * arrowScale, startPos + Vector3.right * arrowScale, startPos - Vector3.up * 0.25f * arrowScale });
        Handles.DrawAAPolyLine(3 , new Vector3[3] { endPos + (Vector3.left + Vector3.up * 0.25f) * arrowScale, endPos, endPos + (Vector3.left - Vector3.up * 0.25f) * arrowScale });
        Handles.color = Color.white;
    }
    

    private void OnDestroy() {
        _nodes.Clear();
        ExecutionOrderManager.Instance.OnChangeOrder -= SyncNodes;
    }

    private void SyncNodes() {
        _nodes.Clear();
        int maxGraphLevel = ExecutionOrderManager.Instance.Scripts.Max(x=>x.GraphLevelRelativeToDefaultTime);
        int minGraphLevel = ExecutionOrderManager.Instance.Scripts.Min(x => x.GraphLevelRelativeToDefaultTime);
        int[] scriptsOnLevels = new int[maxGraphLevel - minGraphLevel + 1];
        foreach (MonoScriptInfo script in ExecutionOrderManager.Instance.Scripts) {
            int levelId = script.GraphLevelRelativeToDefaultTime - minGraphLevel;
            scriptsOnLevels[levelId]++;
        }
        int yOffset = (scriptsOnLevels.Max() * 50)/2 + 10;
        int[] currentScriptLevelId = new int[maxGraphLevel - minGraphLevel + 1];
        foreach (MonoScriptInfo script in ExecutionOrderManager.Instance.Scripts) {
            int levelId = script.GraphLevelRelativeToDefaultTime - minGraphLevel;
            float blockHeight = scriptsOnLevels[levelId] * 50;
            ScriptOrderEditorNode node = new ScriptOrderEditorNode();
            node.AssociatedScript = script;
            node.WindowRect = new Rect(50 + (levelId) * 200, currentScriptLevelId[levelId] * 50 - blockHeight/2 + yOffset, 140, 40);
            currentScriptLevelId[levelId]++;
            _nodes.Add(node);
        }
    }

}
}
