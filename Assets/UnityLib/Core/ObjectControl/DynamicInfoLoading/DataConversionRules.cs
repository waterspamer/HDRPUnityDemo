using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataConversionRules", menuName = "Nettle/DataConversionRules")]
public class DataConversionRules : ScriptableObject
{
    [System.Serializable]
    public struct FieldNameConversion
    {
        public string Name;
        public string NewName;
        public ValueConversion[] ValueConversionOverrides;
    }

    [System.Serializable]
    public struct ValueConversion
    {
        public string Value;
        public string NewValue;
    }

    public FieldNameConversion[] RequiredFields;
    public ValueConversion[] ValueConversions;

    public bool ConvertFieldNameAndValue(ref string name, ref string value)
    {
        FieldNameConversion fieldConversion = new FieldNameConversion();
        bool found = false;
        foreach (FieldNameConversion field in RequiredFields)
        {
            if (field.Name == name)
            {
                fieldConversion = field;
                found = true;
                break;
            }            
        }
        if (!found)
        {
            return false;
        }
        if (!string.IsNullOrEmpty(fieldConversion.NewName))
        {
            name = fieldConversion.NewName;
        }
        foreach (ValueConversion valueConversion in fieldConversion.ValueConversionOverrides)
        {
            if (valueConversion.Value == value)
            {
                value = valueConversion.NewValue;
                return true;
            }
        }
        foreach (ValueConversion valueConversion in ValueConversions)
        {
            if (valueConversion.Value == value)
            {                
                value = valueConversion.NewValue;
                break;
            }
        }
        return true;
    }
}
