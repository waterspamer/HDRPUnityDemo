using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Nettle {
    public class DynamincInfo : MonoBehaviour {
        private const float _autoReloadTimeout = 600;

        [SerializeField]
        private bool _loadOnAwake = true;
        [SerializeField]
        private bool _autoReload = true;
        private IDynamicInfoDataProvider _dataProvider;
        public GuiDatabaseItem[] Items { get; private set; }
        public delegate void DynamicInfoEvent ();
        public DynamicInfoEvent OnReload;
        public bool LoadingError { get; private set; }
        private DateTime _oldFileTime;
        private bool _loading = false;

        public void Reload () {
            _loading = true;
            _dataProvider.GetItems (ItemsLoaded);
        }

        private void ItemsLoaded (DataLoadResult result) {
            _loading = false;
            Items = result.Items;
            LoadingError = result.Error;
            if (OnReload != null) {
                OnReload.Invoke ();
            }
        }

        public GuiDatabaseItem FindFirst (Func<GuiDatabaseItem, bool> predicate) {
            try {
                return Items.Where (x => predicate (x)).First ();
            } catch {
                return null;
            }
        }

        private void Awake () {
            _dataProvider = GetComponent<IDynamicInfoDataProvider> ();
            if (_dataProvider == null) {
                Debug.LogError ("Dynamic Info needs a data provider script attached to it!", gameObject);
                enabled = false;
            }
            if (_loadOnAwake) {
                _loading = true;
                //_oldFileTime  = File.GetLastWriteTime(FullFilePath);
            }
        }

        private void OnEnable () {
            if (_loading)
            {
                Reload();
            }
            if (_autoReload) {
                StartCoroutine (AutoReload ());
            }
        }

        private IEnumerator AutoReload () {
            WaitForSeconds interval = new WaitForSeconds (_autoReloadTimeout);
            while (true) {
                yield return interval;
                Reload ();
            }
        }
    }
}