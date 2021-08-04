using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class StringDictionary: IEnumerable {

    [System.Serializable]
    public class KeyValuePair {
        public string Key;
        public string Value;
    }

    [SerializeField]
    private List<KeyValuePair> _pairs = new List<KeyValuePair>();

    public string[] Keys {
        get {
            string[] result = new string[_pairs.Count];
            for (int i = 0; i < _pairs.Count; i++) {
                result[i] = _pairs[i].Key;
            }
            return result;
        }
    }

    public string[] Values {
        get {
            string[] result = new string[_pairs.Count];
            for (int i = 0; i < _pairs.Count; i++) {
                result[i] = _pairs[i].Value;
            }
            return result;
        }
    }

    public string this[string key] {
        get {
            int i = FindKeyIndex(key);
            if (i >= 0) {
                return _pairs[i].Value;
            }
            else {
                throw new KeyNotFoundException();
            }
        }
        set {
            int i = FindKeyIndex(key);
            if (i >= 0) {
                _pairs[i].Value = value;
            }
            else {
                throw new KeyNotFoundException();
            }
        }
    }

    public IEnumerator GetEnumerator() {
        return _pairs.GetEnumerator();
    }


    public void Add(string key, string value) {
        if (ContainsKey(key)) {
            throw (new System.ArgumentException());
        }
        else {
            KeyValuePair pair = new KeyValuePair();
            pair.Key = key;
            pair.Value = value;
            _pairs.Add(pair);
        }
    }

    public bool ContainsKey(string key) {
        return FindKeyIndex(key) >= 0;
    }

    public void RemoveKey(string key) {
        int i = FindKeyIndex(key);
        if (i >= 0) {
            _pairs.RemoveAt(i);
        }
    }


    public void Clear() {
        _pairs.Clear();
    }

    private int FindKeyIndex(string key) {
        for (int i = 0; i < _pairs.Count; i++) {
            if (_pairs[i].Key.Equals(key)) {
                return i;
            }
        }
        return -1;
    }

}
