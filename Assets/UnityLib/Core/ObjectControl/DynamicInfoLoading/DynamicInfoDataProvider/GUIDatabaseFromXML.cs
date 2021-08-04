using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

namespace Nettle
{
    public class GUIDatabaseFromXML:GUIDatabaseFromFile
    {
        private XmlDocument _document;
        XmlNamespaceManager _namespaceManager;

        public override void Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Debug.LogError("XML file not found: " + fileName);
                return;
            }
            _document = new XmlDocument();
            using (Stream fileStream = File.OpenRead(fileName))
            {
                _document.Load(fileStream);
            }
            _namespaceManager = new XmlNamespaceManager(_document.NameTable);
        }

        public override void LoadText(string text)
        {            
            _document = new XmlDocument();
            _document.LoadXml(text);
            _namespaceManager = new XmlNamespaceManager(_document.NameTable);
        }

        public void AddNamespace(string prefix, string uri)
        {
            _namespaceManager.AddNamespace(prefix, uri);
        }

        public GuiDatabaseItem[] GetItemsWithPath(string xpath)
        {
            if (_document == null)
            {
                return new GuiDatabaseItem[0];
            }
            bool noRulesProvided = ConversionRules == null;
            XmlNodeList nodeList = _document.SelectNodes(xpath,_namespaceManager);
            GuiDatabaseItem[] result = new GuiDatabaseItem[nodeList.Count];
            List<string> headers = new List<string>();
            List<string> values = new List<string>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNodeList childNodeList = nodeList[i].SelectNodes(".//*", _namespaceManager);
                foreach (XmlNode node in childNodeList)
                {
                    string name = node.Name;
                    string value = node.InnerText;
                    if (noRulesProvided || ConversionRules.ConvertFieldNameAndValue(ref name, ref value))
                    {
                        headers.Add(name);
                        values.Add(value);
                    }                    
                }
                result[i] = new GuiDatabaseItem(headers.ToArray(), values.ToArray());
                headers.Clear();
                values.Clear();
            }
            return result;
        }
    }
}