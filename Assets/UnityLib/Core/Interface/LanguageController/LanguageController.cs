using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Nettle
{

    public enum Language
    {
        En,
        Ru,
        Tatar,
        Uz
    }

    [Serializable]
    public class OnLanguageChanged : UnityEvent<Language> { }

    public class LanguageController : MonoBehaviour
    {

        public Language Lang = Language.Ru;

        [Tooltip("The list of languages that are acually used in this project. Leave empty to use all supported languages")]
        public List<Language> UsedLanguages = new List<Language>();

        public OnLanguageChanged OnLanguageChangedEvent = new OnLanguageChanged();

        private void Start()
        {
            SetSceneLanguage(Lang);
        }
        
        public bool IsLanguageUsed(Language lang) {
            return UsedLanguages.Count == 0 || UsedLanguages.Contains(lang);
        }

        public void SetSceneLanguage(Language lang)
        {
            if (Lang != lang && IsLanguageUsed(lang))
            {
                Lang = lang;
                OnLanguageChangedEvent.Invoke(Lang);
            }
        }

        public void SetSceneLanguage(int id)
        {
            if (Enum.GetValues(typeof(Language)).Length > id)
            {
                if (Lang != (Language)id)
                {
                    Lang = (Language)id;
                    OnLanguageChangedEvent.Invoke(Lang);
                }
            }
            else
            {
                Debug.LogError("Language is out of range");
            }
        }

        public void ToggleLanguage()
        {
            var values = Enum.GetValues(typeof(Language));
            int id = 0;
            for (int i = 0; i < values.Length; ++i)
            {
                if (((Language)values.GetValue(i)) == Lang)
                {
                    id = i;
                    break;
                }
            }
            int newId = (id + 1) % values.Length;
            int attemptCounter = 0;
            while (!IsLanguageUsed((Language)newId))
            {
                newId = (newId + 1) % values.Length;
                attemptCounter++;
                if (attemptCounter>=values.Length)
                {
                    Debug.LogWarning("Can't change language because all languages seem to be excluded");
                    return;
                }
            }
            Lang = (Language)newId;
            OnLanguageChangedEvent.Invoke(Lang);
        }



        /*public void SetLanguage(string _lang) {
            string lang = _lang.ToLower();
            Language newLang = Language.En;
            switch (lang) {
                case "en": newLang = Language.En; break;
                case "ru": newLang = Language.Ru; break;
                default: Debug.LogError("Unknown language"); break;
            }

            Lang = newLang;
            OnLanguageChangedEvent.Invoke(Lang);
        }*/

        public Language GetCurrentLanguage()
        {
            return Lang;
        }
    }
}