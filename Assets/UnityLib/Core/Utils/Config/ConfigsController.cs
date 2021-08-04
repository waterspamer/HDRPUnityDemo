using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IniParser;
using IniParser.Model;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nettle {
    public class ConfigsController : MonoBehaviour {

        [HideInInspector]
        public ConfigData[] Configs;
        [HideInInspector]
        public string PathToFolder = "";
        //[HideInInspector]
        //public bool UseAssetsFolder = false;

        /// <summary>
        /// ошибки парсинга конфигурационных файлов
        /// </summary>
        public List<string> Errors;

        /// <summary>
        /// поиск номера строки в которой присутствует не существующий объект
        /// </summary>
        /// <param name="notExistGO">имя несуществующего объекта</param>
        public List<int> FindLineIndex(string notExistGO, string propertyName, List<ConfigBlock> blocks) {
            var list = new List<int>();
            foreach (var block in blocks) {
                if (propertyName.Equals("GameObject")) {
                    if (notExistGO.Equals(block.GoName)) {
                        list.Add(block.HeaderLineNumber);
                    }
                } else if (propertyName.Equals("Component")) {
                    if (notExistGO.Equals(block.ComponentName)) {
                        list.Add(block.HeaderLineNumber);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Путь до папки с конфигами.
        /// </summary>
        public string Path {
            //get { return !UseAssetsFolder ? Application.dataPath : PathToFolder; }
            get { return Application.dataPath + "/" + PathToFolder; }
        }

        /// <summary>
        /// Сериализация всех указанных конфигов.
        /// </summary>
        public void Serialize() {
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            foreach (var config in Configs) {
                if (config == null) {
                    Debug.LogError("Missing config in ConfigController on scene "+gameObject.scene.name);
                }
                SerializeConfig(config.Config);
            }
        }

        /// <summary>
        /// Десериализация всех указанных конфигов.
        /// </summary>
        public void Deserialize() {
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            foreach (var config in Configs) {
                if (config == null) {
                    Debug.LogError("Missing config in ConfigController on scene " + gameObject.scene.name);
                }
                DeserializeConfig(config.Config);
            }
            //вывод ошибок
            //foreach (var item in Errors) {
            //    Debug.Log(item);
            //}
        }

        /// <summary>
        /// возвращает ошибки парсинга конфиг файлов
        /// </summary>
        /// <returns>ошибки парсинга (null - если ошибок нет)</returns>
        public string GetErrors() {
            string result = null;
            foreach (var element in Errors) {
                result += element + "\n";
            }
            return result;
        }


        void Reset() {
            EditorInit();
        }

        void OnValidate() {
            EditorInit();
        }

        void EditorInit() {
            if (!OverridingExist()) {
                ReimportOverridingConfigs();
            }
        }

        private void Awake() {
            Errors = new List<string>();
            PathToFolder += GetSubfolder();
            Deserialize();
        }

        private string GetSubfolder() {
            string qualityName = QualitySettings.names[QualitySettings.GetQualityLevel()].ToLower().Replace(" ", "");
#if UNITY_EDITOR
            return "";
#endif
            string subfolder = "/";

            if (qualityName.Contains("remotedisplay")) {
                subfolder += "RemoteDisplay";
            } else if (qualityName.Contains("framepacking")) {
                subfolder += "FramePacking";
            } else if (qualityName.Contains("nvidia3dvision")) {
                subfolder += "NVidia3DVision";
            } else {
                subfolder += "FramePacking";
            }

            return subfolder;
        }

        /// <summary>
        /// во множестве gameObjectsNames - остаются только gameobjects которые существуют в конфиг файле, но не существуют в unity.
        /// тоже самое для componentsName - только компоненты
        /// </summary>
        /// <param name="hComponents"></param>
        /// <param name="gameObjectsNames"></param>
        /// <param name="componentsName"></param>
        private void RemoveNotExist(Hashtable hComponents, ref HashSet<string> gameObjectsNames, ref HashSet<string> componentsName) {
            foreach (var key in hComponents.Keys) {
                var go = key as GameObject;
                var components = hComponents[key] as HashSet<Type>;
                if (components != null && go != null) {
                    if (gameObjectsNames.Contains(go.name)) {
                        gameObjectsNames.Remove(go.name);
                    }
                    foreach (var component in components) {
                        if (componentsName.Contains(component.Name)) {
                            componentsName.Remove(component.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// преобразование файла конфига в список ConfigBlock
        /// </summary>
        /// <param name="path">путь к файлу конфига</param>
        private List<ConfigBlock> DeserializeConfigFile(string path) {
            var strings = GetStringsInFile(path);
            var blocks = new List<ConfigBlock>();
            for (var i = 0; i < strings.Length; i++) {

                if (string.IsNullOrEmpty(strings[i])) continue;

                if (ConfigBlock.HeaderIsCorrect(strings[i])) {
                    ConfigBlock block = new ConfigBlock();
                    block.Header = strings[i];
                    block.HeaderLineNumber = i;
                    block.AddFields(strings, i, path);
                    i += block.AllFieldCount;
                    blocks.Add(block);
                    //ошибки парсинга полей
                    foreach (var item in block.Errors) {
                        Errors.Add(item);
                    }
                } else {
                    //ошибки парсинга заголовков
                    Errors.Add(ConfigBlock.GetInccorrectFormatMsg(path, (i + 1).ToString()));
                }
            }
            return blocks;
        }

        /// <summary>
        /// получить номер строки в конфиг файле, на которой находится искомое поле
        /// </summary>
        /// <returns>-1 - строка не найдена</returns>
        public int GetFieldLineNumber(List<ConfigBlock> blocks, string goName, string componentName, string fieldName) {
            var lineNumber = -1;
            foreach (var item in blocks) {
                if (item.GoName.Equals(goName) && item.ComponentName.Equals(componentName)) {
                    lineNumber = item.HeaderLineNumber;
                    foreach (var field in item.Fields) {
                        lineNumber += 1;
                        if (field.Key.Equals(fieldName)) {
                            //номер реальной строки в файле = номер строки в массиве строк + 1
                            return lineNumber + 1;
                        }
                    }
                }
            }
            return lineNumber;
        }

        /// <summary>
        /// установка новых значений для компонентов
        /// </summary>
        private void SetFields(Hashtable hComponents, string[] strings, string path, Config config, List<ConfigBlock> blocks) {
            foreach (var key in hComponents.Keys) {
                var go = key as GameObject;
                var components = hComponents[key] as HashSet<Type>;
                if (components != null && go != null) {
                    foreach (var component in components) {
                        var nameArea = string.Format("[{0}/{1}]", go.name, component.Name);
                        var values = GetValuesForBlock(nameArea, strings);
                        config.SetMembers(component, go, values, this, blocks, path);
                    }
                }
            }
        }

        /// <summary>
        /// Десериализация указанного конфига.
        /// </summary>
        /// <param name="config">Конфиг, который необходимо десериализовать.</param>
        public void DeserializeConfig([NotNull] Config config) {
            var path = string.Format("{0}/{1}", Path, config.FileName);
            var strings = GetStringsInFile(path);
            var hComponents = config.GetConfigComponents();
            var blocks = DeserializeConfigFile(path);
            var headersGo = new HashSet<string>();
            var headersСomponents = new HashSet<string>();

            foreach (var item in blocks) {
                item.SplitHeader(item.Header);
                headersGo.Add(item.GoName);
                headersСomponents.Add(item.ComponentName);
            }
            RemoveNotExist(hComponents, ref headersGo, ref headersСomponents);
            GenerateErrors(headersGo, blocks, path, "GameObject");
            GenerateErrors(headersСomponents, blocks, path, "Component");

            SetFields(hComponents, strings, path, config, blocks);
        }

        /// <summary>
        /// генерация ошибок несуществующих объектов
        /// </summary>
        /// <param name="collection">множество несуществуюищх объектов</param>
        /// <param name="blocks">список блоков конфига</param>
        /// <param name="path">путь к файлу конфига</param>
        /// <param name="nameNotExistObj">имя несуществующего объекта</param>
        void GenerateErrors(HashSet<string> collection, List<ConfigBlock> blocks, string path, string nameNotExistObj) {
            foreach (var item in collection) {
                var indexes = FindLineIndex(item, nameNotExistObj, blocks);
                foreach (var index in indexes) {
                    Errors.Add(ConfigBlock.GetNotExistMsg(nameNotExistObj, item, path, (index + 1).ToString()));
                }
            }
        }

        /// <summary>
        /// Получение значений и имен полей для указанного блока.
        /// </summary>
        /// <param name="nameBlock">Имя блока.</param>
        /// <param name="strings">Строки из ini файла.</param>
        /// <returns>Возвращает Hashtable с именами и значениями полей.</returns>
        private Hashtable GetValuesForBlock(string nameBlock, string[] strings) {
            var result = new Hashtable();

            var list = strings.ToList();
            var index = list.IndexOf(nameBlock);

            for (var i = index + 1; i < strings.Length; i++) {
                if (string.IsNullOrEmpty(strings[i])) continue;
                if (strings[i][0] == '[' && strings[i][strings[i].Length - 1] == ']') break;

                var param = strings[i].Split('=');
                result.Add(param[0], param[1]);
            }

            return result;
        }


        /// <summary>
        /// Сериализация указанного конфига.
        /// </summary>
        /// <param name="config">Конфиг, который необходимо сериализовать.</param>
        public void SerializeConfig([NotNull] Config config) {
            var path = string.Format("{0}/{1}", Path, config.FileName);
            var strings = new string[] { };

            var hComponents = config.GetConfigComponents();
            foreach (var key in hComponents.Keys) {
                var go = key as GameObject;
                var components = hComponents[key] as HashSet<Type>;
                foreach (var component in components) {
                    var nameArea = string.Format("[{0}/{1}]", go.name, component.Name);
                    SerializeBlock(nameArea, config.GetMembers(component, go), ref strings);
                }
            }

            SaveStringsInFile(path, strings);
        }

        /// <summary>
        /// Получение всех строк из файла по указанному пути.
        /// </summary>
        /// <param name="path">Путь к ini файлу.</param>
        /// <returns>Возвращает массив строк.</returns>
        private string[] GetStringsInFile(string path) {
            string[] result = null;

            if (!File.Exists(path)) {
                Serialize();
            }

            return File.ReadAllLines(path);
        }

        /// <summary>
        /// Сохранение указанных строк в файл по указанному пути.
        /// </summary>
        /// <param name="path">Путь к ini файлу.</param>
        /// <param name="strings">Строки, которые необходимо записать в файл.</param>
        private void SaveStringsInFile(string path, string[] strings) {
            //create folders if need
            string[] folderNames;
            if (!PathToFolder.Equals("")) {
                folderNames = PathToFolder.Split(new char[] { '/' });
                string currentFolder = Application.dataPath;
                foreach (string folderName in folderNames) {
                    currentFolder = currentFolder + "/" + folderName;
                    Directory.CreateDirectory(currentFolder);
                }
            }

            File.WriteAllLines(path, strings);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private void SerializeBlock(string nameBlock, [NotNull] Hashtable dictionary, ref string[] strings) {
            var list = strings.ToList();
            if (!list.Contains(nameBlock)) {
                list.Add(nameBlock);

                foreach (var key in dictionary.Keys) {
                    string value;
                    if (dictionary[key] is Vector3) {
                        //value = ((Vector3)dictionary[key]).ToStringEx();
                        Vector3 v = (Vector3) dictionary[key];
                        value = string.Format("({0}; {1}; {2})", v.x, v.y, v.z);

                    } else {
                        value = dictionary[key].ToString();
                    }
                    list.Add(string.Format("{0}={1}", key, value));
                }

                list.Add("");
            } else {
                var index = list.IndexOf(nameBlock);

                foreach (var key in dictionary.Keys) {
                    var si = GetIndexParamWithValue(key.ToString(), index + 1, list);
                    if (si != -1) {
                        list[si] = string.Format("{0}={1}", key, dictionary[key]);
                    }
                }
            }

            strings = list.ToArray();
        }

        private int GetIndexParamWithValue(string value, int startIndex, IList<string> strings) {
            var result = -1;

            for (var i = startIndex; i < strings.Count; i++) {
                if (string.IsNullOrEmpty(strings[i])) break;

                var param = strings[i].Split('=');
                if (param[0].Trim() == value) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        public bool OverridingExist() {
            return Directory.Exists(@"Assets/Configs/Overriding/");
        }

        public void ReimportOverridingConfigs() {
            //todo return
            return;
            foreach (string file in Directory.GetFiles(@"Assets/Configs/Overriding/", "*.ini", SearchOption.AllDirectories)) {
                string path = file.Replace(@"\", "/");
                ReimportOverridingConfig(path);
            }
        }

        public static void ReimportOverridingConfig(string path) {
            Encoding utf8WithoutBom = new UTF8Encoding(true);
            if (Regex.IsMatch(path, @".*Assets/Configs/Overriding/.*/.*\.ini")) {
                string configType = Regex.Match(path, @".*/UnityLib/Core/Utils/Config/Overriding/(.*)/.*\.ini")
                    .Groups[1]
                    .Value;
                string fileName = System.IO.Path.GetFileName(path);

                Directory.CreateDirectory(@"Assets/Configs/Overriding/" + configType);
                string pathToTargetFile = @"Assets/Configs/Overriding/" + configType + "/" + fileName;
                if (!File.Exists(pathToTargetFile)) {
                    File.Copy(path, pathToTargetFile, true);
                } else {
                    var parser = new FileIniDataParser();
                    parser.Parser.Configuration.AssigmentSpacer = "";
                    IniData overridingData = parser.ReadFile(path);
                    IniData targetData = parser.ReadFile(pathToTargetFile);

                    overridingData.Merge(targetData);
                    targetData.Merge(overridingData);
                    parser.WriteFile(pathToTargetFile, targetData, new UTF8Encoding(false));
                }
            }
        }
    }



    [Serializable]
    public class ConfigData {
        [SerializeField]
        public Config Config;
    }



    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ConfigFieldAttribute : Attribute {
        public string NameField;

        public ConfigFieldAttribute(string nameField = "") {
            NameField = nameField;
        }
    }


}