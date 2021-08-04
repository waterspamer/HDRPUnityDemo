using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Nettle {

public class MonoScriptInfo {
    public Type ScriptType;
    public int ExecuteTime = 0;
    public int GraphLevelRelativeToDefaultTime = 0;
    public int GraphLevel {
        get {
            if (Before.Count == 0) {
                return 1;
            }
            else {
                return Before.Max(x => x.GraphLevel) + 1;
            }
        }
    }
    public bool ConnectedToDefaultTime {
        get {
            return _connectedToDefaultTime;
        }
    }
    private bool _connectedToDefaultTime = false;
    
    public void MarkAsConnectedToDefaultTime() {
        _connectedToDefaultTime = true;
        foreach (MonoScriptInfo script in Before) {
            if (!script.ConnectedToDefaultTime) {
                script.MarkAsConnectedToDefaultTime();
            }
        }
        foreach (MonoScriptInfo script in After) {
            if (!script.ConnectedToDefaultTime) {
                script.MarkAsConnectedToDefaultTime();
            }
        }
    }

    /// <summary>
    /// Scripts that should be executed after this one (children)
    /// </summary>
    public List<MonoScriptInfo> After = new List<MonoScriptInfo>();
    /// <summary>
    /// Scripts that should be executed before this one (parents)
    /// </summary>
    public List<MonoScriptInfo> Before = new List<MonoScriptInfo>();
    /// <summary>
    /// Checks recoursively, whether adding another script after this one will cause dependency loop
    /// </summary>
    /// <param name="other">script to add</param>
    /// <returns>true if the script to add was already added before this script</returns>
    public bool CheckLoop(MonoScriptInfo other) {
        bool result = false;
        foreach (MonoScriptInfo parent in Before) {
            if (parent.Equals(other)){
                return true;
            }
            else {
                result |= parent.CheckLoop(other);
            }
        }
        return result;
    }
}

public class ExecutionOrderManager {
    private static int _timeOffset = 50;
    private List<MonoScriptInfo> _scripts = new List<MonoScriptInfo>();
    public List<MonoScriptInfo> Scripts {
        get {
            return _scripts;
        }
    }
    private static ExecutionOrderManager _instance;
    public static ExecutionOrderManager Instance {
        get {
            if (_instance == null) {
                _instance = new ExecutionOrderManager();
            }
            return _instance;
        }
    }

    public delegate void ChangeOrderEvent();
    public ChangeOrderEvent OnChangeOrder;

    public void SetOrder() {
        FindScripts();
        int defaultTimeLevel = 0;
        MonoScriptInfo defaultTimeScript = _scripts.Find(x=>x.ScriptType.Equals(typeof(DefaultTime)));         
        if (defaultTimeScript != null) {
            defaultTimeLevel = defaultTimeScript.GraphLevel;
            defaultTimeScript.MarkAsConnectedToDefaultTime();
        }
        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts()) {
            MonoScriptInfo script = _scripts.Find(x => x.ScriptType.Equals(monoScript.GetClass()));
            if (script != null) {
                script.GraphLevelRelativeToDefaultTime = script.GraphLevel;
                if (script.ConnectedToDefaultTime) {
                    script.GraphLevelRelativeToDefaultTime -= defaultTimeLevel;
                }
                script.ExecuteTime = script.GraphLevelRelativeToDefaultTime * _timeOffset;
                //Debug.Log("Setting order for " + script.ScriptType.ToString() + " to " + (script.ExecuteTime - defaultTime));
                if (MonoImporter.GetExecutionOrder(monoScript) != script.ExecuteTime) {
                    MonoImporter.SetExecutionOrder(monoScript, script.ExecuteTime);
                }
            }
        }
        if (OnChangeOrder != null) {
            OnChangeOrder();
        }
    }
    
    private MonoScriptInfo FindScriptInfoOrCreateNew(Type type) {
        if (type == null) {
            return null;
        }
        MonoScriptInfo script = _scripts.Where(x => x.ScriptType == type).FirstOrDefault();
        if (script == null) {
            script = new MonoScriptInfo();
            script.ScriptType = type;
        }
        return script;
    }

    private void AddScriptToListOnce(MonoScriptInfo info, List<MonoScriptInfo> list) {
        if (!list.Contains(info)) {
            list.Add(info);
        }
    }

    private void FindScripts() {
        _scripts.Clear();
        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts()) {
            Type c = monoScript.GetClass();
            if (c != null) {
                MonoScriptInfo script = FindScriptInfoOrCreateNew(c);
                foreach (Attribute attr in script.ScriptType.GetCustomAttributes(true)) {
                    if (attr is ExecutionOrderAttribute) {
                        AddScriptToListOnce(script,_scripts);
                        MonoScriptInfo otherScript = FindScriptInfoOrCreateNew((attr as ExecutionOrderAttribute).OtherScript);
                        if (otherScript != null) {                            
                            if (attr is ExecuteAfterAttribute) {
                                if (!otherScript.CheckLoop(script)) {
                                    //Debug.Log(script.ScriptType.ToString() + " goes after " + otherScript.ScriptType.ToString());
                                    AddScriptToListOnce(script, otherScript.After);
                                    AddScriptToListOnce(otherScript, script.Before);
                                    AddScriptToListOnce(otherScript,_scripts);
                                }
                                else {
                                    Debug.LogWarning("Dependency loop found at script " + script.ScriptType.ToString());
                                }
                            }
                            else if (attr is ExecuteBeforeAttribute) {
                                if (!script.CheckLoop(otherScript)) {
                                    //Debug.Log(script.ScriptType.ToString() + " goes before " + otherScript.ScriptType.ToString());
                                    AddScriptToListOnce(otherScript, script.After);
                                    AddScriptToListOnce(script, otherScript.Before);
                                    AddScriptToListOnce(otherScript, _scripts);
                                }
                                else {
                                    Debug.LogWarning("Dependency loop found at script " + script.ScriptType.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}
}
