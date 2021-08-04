using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Nettle {

/// <summary>
/// преобразованный блок настроек текстового файла в виде объекта
/// </summary> 
public class ConfigBlock {
    public const string HeaderPattern = @"^\[.+\]$";
    public const string FieldPattern = @"^[^=\[\]].+[^=\[\]]$";
    public const string InccorrectFormatMsg = "incorrect format";
    //символ разделения gameobject и component в заголовке
    private const char _separator = '/';

    /// <summary>
    /// заголовок блока
    /// </summary>
    public string Header {
        get { return _header; }
        set { _header = value; }
    }

    /// <summary>
    /// имя конфигурируемого объекта 
    /// </summary>
    public string GoName { get { return _goName; } }

    /// <summary>
    /// имя компонента конфигурируемого объекта 
    /// </summary>
    public string ComponentName { get { return _componentName; } }

    /// <summary>
    /// количество всех полей(игнорируя ошибки парсинга)
    /// </summary>
    public int AllFieldCount { get; private set; }

    /// <summary>
    /// список ошибок 
    /// </summary>
    public List<string> Errors { get { return _errors; } }

    /// <summary>
    /// номер строки в конфигурационном файле
    /// </summary>
    public int HeaderLineNumber { get; set; }

    /// <summary>
    /// формат:[GameObject/Component]
    /// </summary>
    private string _header;
    /// <summary>
    /// формат:ConfigField->Value
    /// </summary>
    private Dictionary<string, string> _fields;

    private string _goName;

    private string _componentName;

    private List<string> _errors;


    public ConfigBlock() {
        _fields = new Dictionary<string, string>();
        _errors = new List<string>();
    }

    public Dictionary<string, string> Fields { get { return _fields; } }

    public void AddField(string key, string value) {
        _fields.Add(key, value);
    }

    /// <summary>
    /// Debug
    /// </summary>
    public void ShowFields() {
        foreach (KeyValuePair<string, string> keyValue in _fields) {
            Debug.Log(keyValue.Key + " - " + keyValue.Value);
        }
        Debug.Log("*********");
    }

    public static bool HeaderIsCorrect(string s) {
        return Regex.IsMatch(s, ConfigBlock.HeaderPattern);
    }

    public static bool FieldIsCorrect(string s) {
        return s.ToCharArray().Where(e => e == '=').Count() == 1 && Regex.IsMatch(s, ConfigBlock.FieldPattern);
    }

    public void AddError(string s) {
        _errors.Add(s);
    }

    /// <summary>
    /// возвращает форматированное сообщение об ошибке формата
    /// </summary>
    /// <param name="path">путь к файлу с ошибкой</param>
    /// <param name="line">номер строки с ошибкой</param>
    /// <returns></returns>
    public static string GetInccorrectFormatMsg(string path, string line) {
        return string.Format("{0} in file: {1}, line: {2}", InccorrectFormatMsg, path, line);
    }

    /// <summary>
    /// возвращает форматированное сообщение об ошибке несуществующего объекта
    /// </summary>
    /// <param name="type">тип объекта</param>
    /// <param name="go">имя объекта</param>
    /// <param name="path">путь к файлу с ошибкой</param>
    /// <param name="line">номер строки с ошибкой</param>
    /// <returns></returns>
    public static string GetNotExistMsg(string type, string go, string path, string line) {
        return string.Format("{0} {1} doesn't exist. File: {2}, line: {3}", type, go, path, line);
    }

    /// <summary>
    /// читает строки после заголовка и до пустой строки
    /// </summary>
    /// <param name="stringsFile">конфиг файл</param>
    /// <param name="headerLine">номер строки заголовка</param>
    public void AddFields(string[] stringsFile, int headerLine, string path) {
        for (int j = headerLine + 1; j < stringsFile.Length; j++) {
            if (stringsFile[j].Equals("")) {
                break;
            }
            AllFieldCount += 1;
            if (!ConfigBlock.FieldIsCorrect(stringsFile[j])) {
                AddError(ConfigBlock.GetInccorrectFormatMsg(path, (j + 1).ToString()));
                continue;
            }

            var parameters = stringsFile[j].Split('=');
            this.AddField(parameters[0], parameters[1]);
        }
    }

    /// <summary>
    /// разбивает заголовок на имена gameobject и component 
    /// <returns>возвращает -1 в случае ошибки</returns>
    /// </summary>
    public void SplitHeader(string header) {
        var pattern = @"[\[\]]";
        var target = "";
        Regex regex = new Regex(pattern);
        this.Header = regex.Replace(this.Header, target);
        var elements = this.Header.Split(_separator);
        _goName = elements[0];
        _componentName = elements[1];
    }
}
}
