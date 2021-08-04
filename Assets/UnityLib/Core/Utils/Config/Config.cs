using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Nettle {
    [ExecuteAfter(typeof(ConfigsController))]
    [ExecuteBefore(typeof(DefaultTime))]
    public class Config : MonoBehaviour {
        private const BindingFlags BINDING_FLAG = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public string FileName = "Config.ini";

        [HideInInspector]
        public ObjectData[] Objects;

        /// <summary>
        /// Получает все компоненты с атрибутом.
        /// </summary>
        /// <returns></returns>
        public Hashtable GetConfigComponents() {
            var result = new Hashtable();
            foreach (var data in Objects) {
                if (data.SceneObject != null) {
                    var list = new HashSet<Type>();
                    foreach (var component in data.SceneObject.GetComponents(typeof(MonoBehaviour))) {
                        if(component == null)
                            continue;
                        var configFieldsCount = GetConfigMembers(component.GetType()).Count();


                        if (configFieldsCount != 0) {
                            list.Add(component.GetType());
                        }
                    }

                    if (!result.ContainsKey(data.SceneObject)) {
                        result.Add(data.SceneObject, list);
                    } else {
                        var oldList = result[data.SceneObject] as HashSet<Type>;
                        if (oldList != null) {
                            foreach (var type in list) {
                                oldList.Add(type);
                            }
                            result[data.SceneObject] = oldList;
                        }
                    }
                }
            }

            return result;
        }

        public IEnumerable<MemberInfo> GetConfigMembers(Type type) {
            return type.GetMembers(BINDING_FLAG).Where(v => v.GetCustomAttributes(typeof(ConfigFieldAttribute), true).Length != 0);
        }

        /// <summary>
        /// Получает все поля в указанном классе и в указанном объекте с атрибутом.
        /// </summary>
        /// <param name="type">Класс, в котором происходит поиск полей.</param>
        /// <param name="@object">Объект, который реализует указанный класс.</param>
        /// <returns>Возвращает найденные поля и их значения.</returns>
        public Hashtable GetMembers([NotNull] Type type, [NotNull] GameObject @object) {
            var result = new Hashtable();

            var t = Type.GetType(type.FullName);
            if (t != null) {
                foreach (var memberInfo in t.GetMembers(BINDING_FLAG)) {
                    
                    var attributes = memberInfo.GetCustomAttributes(typeof(ConfigFieldAttribute), true);
                    if (attributes.Length > 0) {
                        var attribute = attributes.First(s => s is ConfigFieldAttribute) as ConfigFieldAttribute;
                        if (attribute != null) {
                            var nameField = string.IsNullOrEmpty(attribute.NameField) ? memberInfo.Name : attribute.NameField;
                            object value;
                            if (memberInfo is FieldInfo) {
                                value = (memberInfo as FieldInfo).GetValue(@object.GetComponent(t));
                            } else if (memberInfo is PropertyInfo) {
                                value = (memberInfo as PropertyInfo).GetValue(@object.GetComponent(t));
                            } else {
                                continue;
                            }
                            result.Add(nameField, value);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Устанавливает значения полей указанных в values.
        /// </summary>
        /// <param name="type">Класс, в котором происходит поиск полей.</param>
        /// <param name="@object">Объект, который реализует указанный класс.</param>
        /// <param name="values">Новые значения полей.</param>
        public void SetMembers([NotNull] Type type, [NotNull] GameObject @object, [NotNull] Hashtable values, ConfigsController configsController, List<ConfigBlock> blocks, string path) {



            var t = Type.GetType(type.FullName);
            var component = @object.GetComponent(t);
            if (component != null) {

                foreach (var memberInfo in GetConfigMembers(t)) {

                    var fieldName = memberInfo.Name;

                    //override name from attribute
                    var attributes = memberInfo.GetCustomAttributes(typeof(ConfigFieldAttribute), true);
                    if (attributes.Length > 0) {
                        var attribute = (ConfigFieldAttribute)attributes[0];
                        if (!string.IsNullOrEmpty(attribute.NameField)) {
                            fieldName = attribute.NameField;
                        }
                    }

                    Type valueType;
                    if (memberInfo is FieldInfo) {
                        valueType = (memberInfo as FieldInfo).FieldType;
                    } else {
                        valueType = (memberInfo as PropertyInfo).PropertyType;
                    }

                    if (values.ContainsKey(fieldName)) {
                        object value;
                        if (valueType == typeof(Vector2)) {
                            value = CreateVector<Vector2>(values, fieldName);
                        } else if (valueType == typeof(Vector3)) {
                            value = CreateVector<Vector3>(values, fieldName);
                        } else if (valueType == typeof(Vector4)) {
                            value = CreateVector<Vector4>(values, fieldName);
                        } else if (valueType.IsEnum) {
                            try {
                                value = Enum.Parse(valueType, values[fieldName].ToString());
                            } catch (ArgumentException) {
                                var lineNumber = configsController.GetFieldLineNumber(blocks, @object.name, type.Name, fieldName);
                                if (lineNumber != -1) {
                                    configsController.Errors.Add(string.Format("incorrect enum of value. File: {0} Line: {1}", path, lineNumber));
                                } else {
                                    Debug.LogError("lineNumber doesn't exist");
                                }

                                break;
                            }
                        } else {
                            try {
                                value = Convert.ChangeType(values[fieldName], valueType);
                            } catch (FormatException) {
                                var lineNumber = configsController.GetFieldLineNumber(blocks, @object.name, type.Name, fieldName);
                                if (lineNumber != -1) {
                                    configsController.Errors.Add(string.Format("incorrect type of value. File: {0} Line: {1}", path, lineNumber));
                                } else {
                                    Debug.LogError("lineNumber doesn't exist");
                                }
                                break;
                            }
                        }

                        if (memberInfo is FieldInfo) {
                           (memberInfo as FieldInfo).SetValue(component, value);
                        } else {
                            (memberInfo as PropertyInfo).SetValue(component, value);
                        }
                        values.Remove(fieldName);
                    }
                }
            }
            //поля которые есть у компонента в конфиг файле, но отсутствуют у компонента в unity
            if (values.Count > 0) {
                foreach (DictionaryEntry pair in values) {
                    var lineNumber = configsController.GetFieldLineNumber(blocks, @object.name, type.Name, (string)pair.Key);
                    configsController.Errors.Add(string.Format("field doesn't exist. File: {0} Line: {1}", path, lineNumber));
                }
            }
        }

        object CreateVector<T>(Hashtable values, string nameField) {
            var axis = values[nameField].ToString().Split('(')[1].Split(')')[0].Split(';');
            Type t = typeof(T);
            if (t.Name.Equals("Vector2")) {
                return new Vector2(Single.Parse(axis[0]), Single.Parse(axis[1]));
            } else if (t.Name.Equals("Vector3")) {
                return new Vector3(Single.Parse(axis[0]), Single.Parse(axis[1]), Single.Parse(axis[2]));
            } else if (t.Name.Equals("Vector4")) {
                new Vector4(Single.Parse(axis[0]), Single.Parse(axis[1]), Single.Parse(axis[2]),
                                        Single.Parse(axis[3]));
            }

            return null;
        }
    }


    [Serializable]
    public class ObjectData {
        [SerializeField]
        public GameObject SceneObject;
    }

#if UNITY_EDITOR
[CustomEditor(typeof(Config))]
public class ConfigEditor : Editor {
    private ReorderableList _list;
    private Config _source;

    private void OnEnable() {
        _source = target as Config;

        _list = new ReorderableList(serializedObject, serializedObject.FindProperty("Objects"), true, true, true, true);
        _list.drawElementCallback = (rect, index, isActive, isFocused) => {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 30f, EditorGUIUtility.singleLineHeight), index.ToString());

            EditorGUI.PropertyField(new Rect(rect.x + 30, rect.y, rect.xMax - (rect.x + 30), EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("SceneObject"), GUIContent.none);
        };
        _list.drawHeaderCallback = rect => {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 200f, EditorGUIUtility.singleLineHeight), "Objects for save to config");
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (_source != null) {
            //_source.FileName = EditorGUILayout.TextField("Name: ", _source.FileName);
            serializedObject.Update();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
}
