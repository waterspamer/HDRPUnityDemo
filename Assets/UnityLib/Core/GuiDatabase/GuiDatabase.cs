using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EasyButtons;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
namespace Nettle {


    [Serializable] public class DictionaryOfStringAndString : SerializableDictionary<string, string> { }

    [Serializable]
    public class GuiDatabaseItem
    {
        [SerializeField]
        public DictionaryOfStringAndString Fields;

        public GuiDatabaseItem()
        {
            Fields = new DictionaryOfStringAndString();
        }

        public GuiDatabaseItem(string[] headers, string[] fields)
        {
            Fields = new DictionaryOfStringAndString();
            if (headers.Length != fields.Length)
            {
                Debug.LogErrorFormat("Headers ({0}) and fields ({1}) count mismatch", headers.Length, fields.Length);
                Fields = null;
                return;
            }

            for (int i = 0; i < headers.Length; ++i)
            {
                if (headers[i] == null)
                {
                    continue;
                }
                if (Fields.ContainsKey(headers[i]))
                {
                    Debug.LogError("Dublicate header " + headers[i]);
                    continue;
                }
                if (fields[i] == "" || fields[i] == "null" || fields[i] == "undefined")
                {
                    Fields.Add(headers[i], null);
                }
                else
                {
                    Fields.Add(headers[i], fields[i]);
                }
            }

        }

        public string[] GetHeaders()
        {
            return Fields.Select(field => field.Key).ToArray();
        }

        public bool GetFieldByHeader(string header, out string field)
        {
            return Fields.TryGetValue(header, out field);
        }

        public string this[string header]{
            get
            {
                if (Fields.ContainsKey(header))
                {
                    return Fields[header];
                }
                return "";
            }
            set
            {
                if (Fields.ContainsKey(header)) {
                    Fields[header] = value;
                }
            }
        }

        public string InsertValuesToString(string input) {
            string result = string.Copy(input);
            string[] headers = GetHeaders();
            foreach (var header in headers) {
                string value = "";
                if (GetFieldByHeader(header, out value)) {
                    result = result.Replace("{" + header + "}", value);
                }
            }
            return result;
        }
    }

    public enum DatabaseLocationType {
        Asset, URL, StreamingAssets, Remote
    }

    public enum LoadingMode {
        AtStart, Manual
    }

    public enum DatabaseDocumentType {
        Text, Excel
    }

    public class GuiDatabase : MonoBehaviour {
        public const string PersistentDataPathFileName = "Database";
        /// <summary>
        /// Determines the locaton, where the database file can be found.
        /// Asset - database is loaded from text asset in DatabaseAsset field. No excel support.
        /// URL - loaded from url specified in assetUrl field.
        /// Streaming assets - loaded from folder specified in streamingAssetsPath field.
        /// Remote - loaded from server through unity network. Works only on client.
        /// </summary>
        [ConfigField]
        public DatabaseLocationType databaseLocation = DatabaseLocationType.Asset;
        public TextAsset DatabaseAsset;
        [ConfigField]
        public string assetURL = "";
        [ConfigField]
        public string streamingAssetsPath = "";
        /// <summary>
        /// Called when database finished loading
        /// </summary>
        public UnityEvent OnDatabaseReady;


        public bool EnableStreamingAssetsCache = false;
        //[HideInInspector]
        //[SerializeField]
        public GuiDatabaseScriptableObject GuiDatabaseScriptableObject;
        //[HideInInspector]
        //[SerializeField]
        [NonSerialized]
        public List<GuiDatabaseItem> Items = new List<GuiDatabaseItem>();
        [SerializeField]
        private LoadingMode _loadingMode = LoadingMode.AtStart;

        [SerializeField]
        private bool LoadFallbackIfRemoteFails = true;
        [ConditionalHide("LoadFallbackIfRemoteFails", true)]
        [SerializeField]
        private NEvent networkState;
        [ConditionalHide("LoadFallbackIfRemoteFails", true)]
        [SerializeField]
        private DatabaseLocationType fallbackLocation;
        [ConditionalHide("LoadFallbackIfRemoteFails", true)]
        [SerializeField]
        private float fallbackLoadTimeout = 5;
        /// <summary>
        /// Type of the loaded document
        /// </summary>
        public DatabaseDocumentType LoadedType { get; private set; }
        public byte[] LoadedBytes { get; private set; }

        public string DBCacheStatus = "N/A";
        [HideInInspector]
        [SerializeField]
        private string _dbCacheLastUpdate = "N/A";
        [HideInInspector]
        [SerializeField]
        private string _dbCacheState = "N/A";

        private bool isReady = false;
        private bool isLoading = false;

        [HideInInspector]
        [SerializeField]
        private long _loadedDBCreationTime;
        [HideInInspector]
        [SerializeField]
        private long _loadedDBLastWriteTime;

        public static void CheckAllDB(string[] importedAssets = null) {
            foreach (var guiDatabase in GameObject.FindObjectsOfType<GuiDatabase>()) {
                guiDatabase.UpdateDatabase(importedAssets);
            }
        }

        /// <summary>
        /// True if the database has been successfully loaded
        /// </summary>
        public bool IsReady {
            get {
                return isReady;
            }
        }

        private void Start() {
            if (_loadingMode == LoadingMode.AtStart) {
                LoadDatabase();
            }
        }

        private void InitDatabaseFromText(string dbText) {
            LoadedBytes = System.Text.Encoding.ASCII.GetBytes(dbText);
            Items.Clear();
            dbText = dbText.Replace("\r", "");
            var lines = dbText.Split('\n');
            var headers = lines[0].Split('\t');
            for (int i = 1; i < lines.Length; ++i) {
                if (lines[i] == "") { continue; }
                var fields = lines[i].Split('\t');
                var item = new GuiDatabaseItem(headers, fields);
                Items.Add(item);
            }
            isReady = true;
            if (OnDatabaseReady != null) {
                OnDatabaseReady.Invoke();
            }
            isLoading = false;
        }

        private void OnLoadError(string error) {
            isLoading = false;
        }

        private void InitDatabaseFromExcel(byte[] bytes) {
            LoadedBytes = bytes;
            bool error = false;
            try {
                string tempFileName = Application.persistentDataPath + "/" + PersistentDataPathFileName + ".xlsx";
                if (File.Exists(tempFileName)) {
                    File.Delete(tempFileName);
                }
                File.WriteAllBytes(tempFileName, bytes);
                //PlayerPrefs.SetString("RemoteDatabaseFileExtension", ".xlsx");
                ExcelParser parser = new ExcelParser(tempFileName);
                Items = parser.Items;
                Debug.Log("InitDatabaseFromExcel::Success");
            } catch (System.Exception ex) {
                Debug.LogError("Error loading excel database:\n" + ex.Message + "\n" + ex.StackTrace);
                error = true;
            }
            if (!error) {
                isReady = true;
                if (OnDatabaseReady != null) {
                    OnDatabaseReady.Invoke();
                }
            }
            isLoading = false;
        }

        public void OnValidate() {
#if UNITY_EDITOR
            if (gameObject.scene.name == null) {
                return;
            }
#endif
            if (EnableStreamingAssetsCache && databaseLocation != DatabaseLocationType.StreamingAssets) {
                databaseLocation = DatabaseLocationType.StreamingAssets;
            }
            UpdateDatabase();
        }

        [Button]
        public void Update_Database() { //inspector button
            UpdateDatabase(null, true);
        }

        public void UpdateDatabase(string[] importedAssets = null, bool forced = false) {
            if (!EnableStreamingAssetsCache) {
                return;
            }
            if (string.IsNullOrEmpty(streamingAssetsPath)) {
                Debug.LogError("streamingAssetsPath empty");
                return;
            }
            string fullPath = "Assets/StreamingAssets/" + streamingAssetsPath;
            if (importedAssets != null && !importedAssets.Contains(fullPath)) {
                return;
            }

            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + "/" + streamingAssetsPath);
            if (!fileInfo.Exists) {
                Debug.LogError("File " + Application.streamingAssetsPath + "/" + streamingAssetsPath + " doesn't exist");
                return;
            }
            if (!forced && _loadedDBCreationTime == fileInfo.CreationTime.Ticks && _loadedDBLastWriteTime == fileInfo.LastWriteTime.Ticks) {
                return;
            }
#if UNITY_EDITOR
            int dialogResult =
                EditorUtility.DisplayDialogComplex("Database needs to be updated", "Database file " + streamingAssetsPath + " was changed. Do you want update databese now?", "Update", "Later", "Skip");

            if (dialogResult == 0 || dialogResult == 2) {
                Undo.RecordObject(this, "UpdateActualTime");
                _loadedDBCreationTime = fileInfo.CreationTime.Ticks;
                _loadedDBLastWriteTime = fileInfo.LastWriteTime.Ticks;
            }
            if (dialogResult == 0) {
                LoadDatabase();
                GuiDatabaseScriptableObject = ScriptableObject.CreateInstance<GuiDatabaseScriptableObject>();
                AssetDatabase.CreateAsset(GuiDatabaseScriptableObject, "Assets/" + fileInfo.Name + ".asset");
                GuiDatabaseScriptableObject.Items = new List<GuiDatabaseItem>();
                foreach (var item in Items) {
                    GuiDatabaseScriptableObject.Items.Add(item);
                }
                EditorUtility.SetDirty(GuiDatabaseScriptableObject);
                EditorUtility.SetDirty(this);
                //AssetDatabase.SaveAssets();
                _dbCacheLastUpdate = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
                _dbCacheState = "Actual";
            } else if (dialogResult == 1) {
                _dbCacheState = "Later";
            } else if (dialogResult == 2) {
                _dbCacheState = "Skipped";
            }
            DBCacheStatus = "State: " + _dbCacheState + "; Last Update: " + _dbCacheLastUpdate;



#endif
        }

        public void LoadDatabase() {
            Debug.Log("GuiDatabase.LoadDatabese::Start");
            LoadDatabase(databaseLocation);
            Debug.Log("GuiDatabase.LoadDatabese::End::Items.count=" + (Items != null ? Items.Count.ToString() : "null"));

        }

        private void LoadDatabase(DatabaseLocationType location) {
            databaseLocation = location;
            isLoading = true;
            if (databaseLocation == DatabaseLocationType.Asset) {
                if (DatabaseAsset == null) {
                    Debug.LogError("Database asset is null");
                }
                var dbText = DatabaseAsset.text;
                LoadedType = DatabaseDocumentType.Text;
                InitDatabaseFromText(dbText);
            } else if (databaseLocation == DatabaseLocationType.URL) {
                LoadedType = GetDocumentType(assetURL);
                if (LoadedType == DatabaseDocumentType.Excel) {
                    TextAssetDownloader.Instance.Load(assetURL, InitDatabaseFromExcel, OnLoadError);
                }
                if (LoadedType == DatabaseDocumentType.Text) {
                    TextAssetDownloader.Instance.Load(assetURL, InitDatabaseFromText, OnLoadError);
                }
            } else if (databaseLocation == DatabaseLocationType.StreamingAssets) {
                if (EnableStreamingAssetsCache && Application.isPlaying) {
                    Items = GuiDatabaseScriptableObject.Items;
                    isReady = true;
                    if (OnDatabaseReady != null) {
                        OnDatabaseReady.Invoke();
                    }
                } else {
                    LoadDatabaseFromPath(Application.streamingAssetsPath + "/" + streamingAssetsPath);
                }
            } else if (databaseLocation == DatabaseLocationType.Remote) {
                if (LoadFallbackIfRemoteFails) {
                    StartCoroutine(WaitForFallback());
                }
            }
        }

        private IEnumerator WaitForFallback() {
            yield return new WaitForSeconds(fallbackLoadTimeout);
            if (!networkState && !isReady) {
                string path = Application.persistentDataPath + "/" + PersistentDataPathFileName + PlayerPrefs.GetString("RemoteDatabaseFileExtension", ".xlsx");
                if (File.Exists(path)) {
                    Debug.Log("Can't load database remotely, loading last received file");
                    LoadDatabaseFromPath(path);
                } else {
                    Debug.Log("Can't load database remotely and no previous file was found, loading fallback from " + fallbackLocation);
                    LoadDatabase(fallbackLocation);
                }
            }
        }

        public void ReceiveRemoteDatabase(byte[] bytes, DatabaseDocumentType type) {
            Debug.Log("Received " + type + " database from server");
            if (databaseLocation == DatabaseLocationType.Remote) {
                if (type == DatabaseDocumentType.Excel) {
                    InitDatabaseFromExcel(bytes);
                } else {
                    InitDatabaseFromText(System.Text.Encoding.ASCII.GetString(bytes));
                }
            }
            //Save file for later use as fallback
            string extension = ".xlsx";
            if (type == DatabaseDocumentType.Text) {
                extension = ".txt";
            }
            string path = Application.persistentDataPath + "/" + PersistentDataPathFileName + extension;
            File.WriteAllBytes(path, bytes);
            PlayerPrefs.SetString("RemoteDatabaseFileExtension", extension);
        }

        private void LoadDatabaseFromPath(string path) {
            if (File.Exists(path)) {
                LoadedType = GetDocumentType(path);
                if (LoadedType == DatabaseDocumentType.Excel) {
                    byte[] bytes = File.ReadAllBytes(path);
                    InitDatabaseFromExcel(bytes);
                } else if (LoadedType == DatabaseDocumentType.Text) {
                    string text = File.ReadAllText(path);
                    InitDatabaseFromText(text);
                }
            } else {
                Debug.LogError("File doesn't exist at path " + path);
            }
        }

        public GuiDatabaseItem GetItemWhere(string header, string value) {
            for (int i = 0; i < Items.Count; ++i) {
                string curValue;
                if (Items[i].GetFieldByHeader(header, out curValue) && curValue == value) {
                    return Items[i];
                }
            }
            return null;
        }

        public List<GuiDatabaseItem> GetItemsWhere(string header, string regex = null) {
            return Items.Where(v => {
                string value;
                return regex == null || (v.GetFieldByHeader(header, out value) && Regex.Match(value, regex).Success);
            }
            ).ToList();
        }

        public bool GetItemIDWhere(string header, string value, ref int outValue) {
            for (int i = 0; i < Items.Count; ++i) {
                string curValue;
                if (Items[i].GetFieldByHeader(header, out curValue) && curValue == value) {
                    outValue = i;
                    return true;
                }
            }
            return false;
        }

        private DatabaseDocumentType GetDocumentType(string fileName) {
            string[] split = fileName.ToLower().Split('.');
            if (split[split.Length - 1].Equals("xlsx")) {
                return DatabaseDocumentType.Excel;
            }
            return DatabaseDocumentType.Text;
        }


    }

#if UNITY_EDITOR
    class GuiDatabaseAssetPostprocessor : AssetPostprocessor {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (importedAssets.Length > 0) {
                GuiDatabase.CheckAllDB(importedAssets);
            }
        }
    }
#endif
}
