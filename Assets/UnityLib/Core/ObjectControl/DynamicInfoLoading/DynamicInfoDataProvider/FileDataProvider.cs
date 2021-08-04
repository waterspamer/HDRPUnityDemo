using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Nettle {
    public abstract class FileDataProvider : MonoBehaviour, IDynamicInfoDataProvider {
        public string StreamingAssetsPath = "DynamicInfo.xml";
        public string URL;
        public bool WriteLocalBackupFile = true;
        [Tooltip ("Time span after which the local file is considered outdated (in hours)")]
        public float LocalFileUpToDateDuration = 24;
        public DataConversionRules ConversionRules;
        protected GUIDatabaseFromFile _fileDatabase;

        public string FullFilePath {
            get {
                return Application.streamingAssetsPath + "/" + StreamingAssetsPath;
            }
        }

        public void GetItems(DatabaseItemsLoadedCallback callback)
        {
            if (callback == null)
            {
                Debug.LogError("Null callback passed to file data provider");
            }
            else
            {
                StartCoroutine(GetItemsAsync(callback));
            }
        }

        protected abstract GUIDatabaseFromFile CreateDatabase ();

        private IEnumerator GetItemsAsync(DatabaseItemsLoadedCallback callback)
        {
            _fileDatabase = CreateDatabase();
            _fileDatabase.ConversionRules = ConversionRules;
            DataLoadResult result = new DataLoadResult();
            //First, try to load a remote file
            if (URL != "")
            {
                UnityWebRequest wr = UnityWebRequest.Get(URL);
                yield return wr.SendWebRequest();
                if (wr.isNetworkError)
                {
                    Debug.LogError("Error loading file at " + URL + " : " + wr.error);
                }
                else
                {
                    bool parseSuccess = false;
                    try
                    {
                        _fileDatabase.LoadText(wr.downloadHandler.text);
                        parseSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error parsing remote database file: " + ex.Message);
                    }

                    if (parseSuccess)
                    {

                        if (WriteLocalBackupFile)
                        {
                            try
                            {
                                string dir = Path.GetDirectoryName(FullFilePath);
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }
                                File.WriteAllText(FullFilePath, wr.downloadHandler.text);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning("Error writing database to a local file: " + ex.Message);
                            }
                        }

                        result.Items = LoadItemsFromFile();
                        result.Error = false;
                        callback.Invoke(result);
                        yield break;
                    }
                }
            }
            //If the remote file hasn't been loaded, try loading the local file
            if (!File.Exists(FullFilePath))
            {
                result.Error = true;
                callback.Invoke(result);
                yield break;
            }
            //Check if the file has been updated recently
            if (WriteLocalBackupFile)
            {
                TimeSpan diff = DateTime.Now - File.GetLastWriteTime(FullFilePath);
                if (diff.TotalHours >= LocalFileUpToDateDuration)
                {
                    Debug.LogError("Outdated local database file at path " + FullFilePath);
                    result.Error = true;
                    callback.Invoke(result);
                    yield break;
                }
            }

            try
            {
                _fileDatabase.Load(FullFilePath);
                result.Items = LoadItemsFromFile();
                result.Error = false;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error reading local database file: " + ex.Message);
                result.Error = true;
            }
            callback.Invoke(result);
        }

        protected abstract GuiDatabaseItem[] LoadItemsFromFile ();

    }
}