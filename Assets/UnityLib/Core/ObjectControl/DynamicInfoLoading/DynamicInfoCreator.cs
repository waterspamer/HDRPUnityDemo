using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public abstract class DynamicInfoCreator : MonoBehaviour
    {
        private const string _outdatedDataMessage = "Устаревшие данные CRM";

        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                RecreateInfo();
            }
        }
        [SerializeField]
        public DynamincInfo _info;

        [SerializeField]
        private bool _visible = false;

        [SerializeField]
        private float _labelsScale = 1;

        [SerializeField]
        private DynamicInfoLabel _labelPrefab;

        protected List<GameObject> _spawnedLabels = new List<GameObject>();
        private bool _errorMessageShown = false;

        protected virtual void Start()
        {

            _info.OnReload += (RecreateInfo);
        }

        protected virtual void OnEnable()
        {
            RecreateInfo();
        }

        protected virtual void OnDestroy()
        {
            _info.OnReload -= RecreateInfo;
        }

        public virtual void RecreateInfo()
        {
            foreach (GameObject label in _spawnedLabels)
            {
                Destroy(label.gameObject);
            }
            _spawnedLabels.Clear();
            if (_info.LoadingError)
            {
                if (Visible)
                {
                    if (!_errorMessageShown && MessageBox.Instance != null)
                    {
                        MessageBox.Instance.ShowMessage(_outdatedDataMessage);
                        _errorMessageShown = true;
                    }
                }
                return;
            }
            _errorMessageShown = false;
            CreateInfo();
        }

        public abstract void CreateInfo();

        protected DynamicInfoLabel CreateLabel(GuiDatabaseItem item, Vector3 position, Quaternion rotation)
        {

            DynamicInfoLabel label = Instantiate(_labelPrefab.gameObject, position, rotation).GetComponent<DynamicInfoLabel>();
            _spawnedLabels.Add(label.gameObject);
            label.OutputInfo(item);
            label.gameObject.SetActive(Visible);
            label.transform.localScale = Vector3.one * _labelsScale;
            label.transform.SetParent(transform);
            return label;
        }
    }
}