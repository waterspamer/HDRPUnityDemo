using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using IniParser;
using IniParser.Model;

namespace Nettle
{

    public class BuildPostprocessor
    {
        public static string VersionAssetDir = "Assets\\BuildInfo\\";


        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {

            string buildDir = GetBuildDir(pathToBuiltProject);
            string errorFilePath = buildDir + "\\WARNING_BUILD_FOR_x86";

            if (target == BuildTarget.StandaloneWindows)
            {
                Debug.LogError("Build for x32");
                File.WriteAllText(errorFilePath, "This is an x86 build. Are you sure you don't want to rebuild for x86_64?");
            }
            else if (File.Exists(errorFilePath))
            {
                File.Delete(errorFilePath);
            }

            string buildAltApiPath = buildDir + "\\AltApi.dll";
            if (File.Exists(buildAltApiPath))
            {
                File.Delete(buildAltApiPath);
            }
            string assetsAltApiPath;
            if (target == BuildTarget.StandaloneWindows64)
            {
                assetsAltApiPath = "Assets\\UnityLib\\Core\\Plugins\\x86_64\\Dependency\\AltApi.dll";
            }
            else
            {
                assetsAltApiPath = "Assets\\UnityLib\\Core\\Plugins\\x86\\Dependency\\AltApi.dll";
            }

            if (File.Exists(assetsAltApiPath))
            {
                File.Copy(assetsAltApiPath, buildAltApiPath);
            }
            else
            {
                Debug.LogError("Can't find altApi at path " + assetsAltApiPath + " Place the dll inside the build folder manually.");
            }

            string[] configTypePathes = Directory.GetDirectories("Assets\\Configs\\Overriding\\");
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.AssigmentSpacer = "";
            foreach (string configTypePath in configTypePathes)
            {
                string configType = new DirectoryInfo(configTypePath).Name;
                string destinationDirPath = GetDataPath(pathToBuiltProject) + "\\Configs\\" + configType + "\\";
                Directory.CreateDirectory(destinationDirPath);
                foreach (string pathToFile in Directory.GetFiles("Assets\\Configs"))
                {
                    if (Path.GetExtension(pathToFile) == ".meta")
                    {
                        continue;
                    }
                    string fileName = Path.GetFileName(pathToFile);
                    string overridingFilePath = "Assets\\Configs\\Overriding\\" + configType + "\\" + fileName;
                    string configFilePath = "Assets\\Configs\\" + fileName;
                    string destinationFilePath = destinationDirPath + fileName;

                    if (!File.Exists(overridingFilePath))
                    {
                        File.Copy(configFilePath, destinationFilePath, true);
                    }
                    else
                    {
                        IniData data = parser.ReadFile(overridingFilePath);
                        IniData targetData = parser.ReadFile(configFilePath);
                        targetData.Merge(data);
                        parser.WriteFile(destinationFilePath, targetData, new UTF8Encoding(false));
                    }
                }
            }
        }

        [PostProcessBuild(10)]
        static void IncBuildVersion(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.StandaloneWindows64)
            {
                string versionTxt = "Date: " + DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;
                versionTxt += "Machine: " + Environment.MachineName + Environment.NewLine;
                versionTxt += "User: " + Environment.UserName + Environment.NewLine;
                File.AppendAllText(GetDataPath(pathToBuiltProject) + "/build.info", versionTxt);
            }
        }

        private static string GetDataPath(string pathToBuiltProject)
        {
            return GetBuildDir(pathToBuiltProject) + "/" + Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data";
        }


        private static string GetBuildDir(string pathToBuiltProject)
        {
            string result = Path.GetDirectoryName(pathToBuiltProject);
            if (pathToBuiltProject[0] == pathToBuiltProject[1] && pathToBuiltProject[0] == '/')
            {
                result = "\\" + result;
            }
            return result;
        }

    }

}
