using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;

namespace Nettle {


    /// <summary>
    /// This class unzips xlsx file and then parses contained xml, forming the list of table rows.
    /// It ignores all data that is not necessary for initializing GuiDatabase.
    /// </summary>
    public class ExcelParser {
        private const string worksheetPath = "xl/worksheets/sheet1.xml";
        private const string sharedStringsPath = "xl/sharedStrings.xml";

        public List<GuiDatabaseItem> Items { get; private set; }
        private int columnsCount = 0;
        private string extractedDataFolder;
        private string[] sharedStrings;
        private XmlNamespaceManager namespaceManager;

        public ExcelParser(string fileName) {
            extractedDataFolder = Application.persistentDataPath + "/ExtractedDatabase";
            Unzip(fileName, extractedDataFolder);
            namespaceManager = CreateDefaultNSM();
            LoadSharedStrings();
            LoadTable();
            Directory.Delete(extractedDataFolder, true);
        }

        private XmlNamespaceManager CreateDefaultNSM() {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace(string.Empty, "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            namespaceManager.AddNamespace("d", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            return namespaceManager;
        }

        private void LoadSharedStrings() {
            XmlDocument doc = LoadXml(extractedDataFolder + "/" + sharedStringsPath);

            XmlNodeList xmlNodeList = doc.SelectNodes("//d:sst/d:si", namespaceManager);
            sharedStrings = new string[xmlNodeList.Count];
            if (xmlNodeList != null) {
                int i = 0;
                foreach (XmlNode xmlNode1 in xmlNodeList) {
                    XmlNode xmlNode2 = xmlNode1.SelectSingleNode("d:t", namespaceManager);
                    if (xmlNode2 != null)
                        sharedStrings[i] = xmlNode2.InnerText;
                    else
                        sharedStrings[i] = xmlNode1.InnerXml;
                    i++;
                }
            }
        }

        private void CountColumns(XmlDocument doc) {
            //Get top row in 
            XmlNode headersRow = doc.SelectSingleNode("//d:sheetData/d:row", namespaceManager);
            XmlNodeList columns = headersRow.SelectNodes("d:c", namespaceManager);
            columnsCount = columns.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">The address of the cell, for example A1</param>
        /// <returns>The position of the cell in its row as a number</returns>
        private int GetCellPosition(string address) {
            return address[0] - 'A';
        }

        private void LoadTable() {
            Items = new List<GuiDatabaseItem>();
            XmlDocument doc = LoadXml(extractedDataFolder + "/" + worksheetPath);
            CountColumns(doc);
            XmlNodeList rowsList = doc.SelectNodes("//d:sheetData/d:row", namespaceManager);
            bool parsingHeaders = true;
            string[] headers = new string[columnsCount];
            if (rowsList != null) {
                foreach (XmlNode row in rowsList) {
                    bool rowIsEmpty = true;
                    string[] fields = new string[columnsCount];
                    XmlNodeList cellsList = row.SelectNodes("d:c", namespaceManager);
                    foreach (XmlNode cell in cellsList) {
                        XmlNode cellValue = cell.SelectSingleNode("d:v", namespaceManager);
                        XmlNode addressAtribute = cell.Attributes.GetNamedItem("r");
                        int position = GetCellPosition(addressAtribute.Value);
                        string valueString = "";
                        if (cellValue != null) {
                            XmlNode typeAttr = cell.Attributes.GetNamedItem("t");
                            string type = "";
                            if (typeAttr != null) {
                                type = typeAttr.Value;
                            }
                            if (type == "s") {
                                //Insert shared string into the cell
                                int stringId = 0;
                                if (int.TryParse(cellValue.InnerText, out stringId)) {
                                    if (stringId >= 0 && stringId < sharedStrings.Length) {
                                        valueString = sharedStrings[stringId];
                                    }
                                    else {
                                        Debug.LogError("Shared string id is out of range!");
                                    }
                                }
                                else {
                                    Debug.LogError("Invalid shared string id");
                                }
                            }
                            else {
                                //Fix fucked up numeric values
                                Match floatCheck = Regex.Match(cellValue.InnerText, @"\d+[\.,\,]\d{2}");
                                if (floatCheck.Success) {
                                    valueString = floatCheck.Groups[0].Value;
                                    if (valueString.EndsWith("00")) {
                                        valueString = valueString.Substring(0, valueString.Length - 3);
                                    }
                                }
                                else {
                                    valueString = cellValue.InnerText;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(valueString)) {
                            valueString = valueString.TrimStart().TrimEnd();
                        }
                        if (position >= 0 && position < columnsCount) {
                            if (parsingHeaders) {
                                headers[position] = valueString;
                            }
                            else {
                                fields[position] = valueString;
                            }
                            if (!string.IsNullOrEmpty(valueString)) {
                                rowIsEmpty = false;
                            }
                        }
                    }
                    if (!parsingHeaders && !rowIsEmpty) {
                        try {
                            Items.Add(new GuiDatabaseItem(headers, fields));
                        }
                        catch (System.Exception ex) {
                            var outStr = "";
                            for (int i = 0; i < headers.Length; ++i) {
                                outStr += headers[i] + " : " + fields[i] + "\n";
                            }
                            Debug.LogError(outStr);

                            throw;
                        }
                    }
                    else {
                        parsingHeaders = false;
                    }
                }
            }
        }

        private XmlDocument LoadXml(string file) {
            XmlDocument result = new XmlDocument();
            using (FileStream stream = new FileStream(file, FileMode.Open)) {
                result.Load(stream);
            }
            return result;
        }

        private void Unzip(string archiveFilenameIn, string outFolder) {
            ZipFile zf = null;
            try {
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR)
                ZipConstants.DefaultCodePage = 0;
#endif
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf) {
                    if (!zipEntry.IsFile) {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.
                    if (entryFileName.Equals(worksheetPath) || entryFileName.Equals(sharedStringsPath)) {
                        byte[] buffer = new byte[4096];     // 4K is optimum
                        Stream zipStream = zf.GetInputStream(zipEntry);

                        // Manipulate the output filename here as desired.
                        String fullZipToPath = Path.Combine(outFolder, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                        // of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using (FileStream streamWriter = File.Create(fullZipToPath)) {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
            }
            finally {
                if (zf != null) {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
    }
}
