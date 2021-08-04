using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

//using SharpSvn;

namespace Nettle.SVN {
    public static class Svn {
        public enum NodeType {
            File,
            Directory
        }

        public static string[] SplitLines(string svnOutput) {
            return svnOutput.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        [Serializable]
        public class Entry {
            public NodeType Kind;
            public string Name;
            public long Size;
            public int Revision;
            public string Author;
            public string Date;
        }

        #region SVN_Common

        /// <summary>
        /// Executes svn copy command.
        /// </summary>
        /// <param name="sourceUrl">Source repo folder.</param>
        /// <param name="destUrl">Destination repo folder.</param>
        /// <param name="revision">Source revision to copy, possible values:    HEAD(latest in repository), 
        ///                                                                     NUMBER(revision number), 
        ///                                                                     BASE(base rev of item's working copy), 
        ///                                                                     COMMITTED(last commit at or before BASE),
        ///                                                                     PREV(revision just before COMMITTED).</param>
        public static bool Copy(string sourceUrl, string destUrl, string logMessage, string revision = null) {
            var executeResult = Shell.Execute("svn copy -m \""+ logMessage + "\" " + (revision == null ? "" : "-r" + revision + " ") + sourceUrl + " " + destUrl);
            if (!executeResult.Success) {
                PrintError("Copy", executeResult.StdErr);
            }

            return executeResult.Success;
        }

        /// <summary>
        /// Create an unversioned copy of a tree.
        /// <para>usage: 1. Export(<paramref name="srcUrl"/> = URL, <paramref name="dstDirectory"/> = PATH, optional <paramref name="revision"/>)</para>
        /// <para>Exports a clean directory tree from the repository specified by
        /// URL, at revision <paramref name="revision"/> if it is given, otherwise at HEAD, into PATH.</para>
        /// 
        /// <para>usage: 2. Export(<paramref name="srcUrl"/> = PATH1, <paramref name="dstDirectory"/> = PATH2, optional <paramref name="revision"/>)</para>
        /// <para>Exports a clean directory tree from the working copy specified by
        /// PATH1, at revision REV if it is given, otherwise at WORKING, into PATH2.
        /// If <paramref name="revision"/> is not specified, all local changes will be preserved.  Files 
        /// not under version control will not be copied.</para>
        /// </summary>
        /// <param name="srcUrl">Repo URL or working copy path.</param>
        /// <param name="dstDirectory">Destination folder path.</param>
        /// <param name="revision">Optional: target revision.</param>
        public static void Export(string srcUrl, string dstDirectory, string revision = null)
        {
            string command = "svn export " + (revision == null ? "" : "-r" + revision + " ") +"\"" + srcUrl + "\" \"" + dstDirectory + "\"";
            //UnityEngine.Debug.Log(command);
            string result = Shell.Execute(command).StdErr;
            /*if (!string.IsNullOrEmpty(result))
            {
                UnityEngine.Debug.Log("Svn export error: " + result);
            }*/
        }

        /// <summary>
        /// List directory entries in the repository.
        /// </summary>
        /// <param name="repoUrl">Repo URL or working copy path</param>
        /// <returns></returns>
        public static string[] List(string repoUrl, bool recursive = true) {
            string outputPath = UnityEngine.Application.persistentDataPath + "/ListResult.txt";
            Shell.CommandProperties properties = new Shell.CommandProperties() { OutputToFile = true, OutputPath = outputPath};
            try
            {
                Shell.ProcessResult commandResult = Shell.Execute(@"svn list " + (recursive ? " --depth infinity " : " ")  + repoUrl, properties);
                string[] result = File.ReadAllLines(outputPath);
                File.Delete(outputPath);
                return result;
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError("Error executing List command: " + e.Message + "\n" +e.StackTrace);            
                return null;
            }
        }

        /// <summary>
        /// List directory entries in the repository.
        /// </summary>
        /// <param name="repoUrl">Repo URL or working copy path</param>
        /// <param name="recursive">List all folders and files recursively</param>
        /// <returns></returns>
        public static List<Entry> ListVerbose(string repoUrl, bool recursive = true) {
            //float startTime = Time.realtimeSinceStartup;
            string xmlPath = UnityEngine.Application.persistentDataPath + "/ListResult.xml";
            Shell.CommandProperties properties = new Shell.CommandProperties() { OutputToFile = true, OutputPath = xmlPath};
            Shell.ProcessResult commandResult = Shell.Execute(@"svn list" + (recursive ? " --depth infinity" : "") + " --xml " + repoUrl,properties);
            //string names = commandResult.StdOut;
            //UnityEngine.Debug.Log(names);
            /*float stopTime = Time.realtimeSinceStartup;
            Debug.Log("Dump time: " + (stopTime - startTime).ToString());*/
            XmlDocument xDoc = new XmlDocument();
            FileStream xmlStream = File.OpenRead(xmlPath);
            var result = new List<Entry>();
            try
            {
                xDoc.Load(xmlStream);
                XmlElement xRoot = xDoc.DocumentElement;
                if (xRoot.ChildNodes[0].Name != "list")
                {
                    Debug.WriteLine("Svn.ListVerbose Failed! Invalid output");
                    return null;
                }

                var listNode = xRoot.ChildNodes[0];

                foreach (XmlNode entryNode in listNode)
                {
                    var entry = new Entry();
                    string kind = entryNode.Attributes[0].Value;

                    entry.Kind = (kind == "file") ? NodeType.File : NodeType.Directory;

                    foreach (XmlNode propNode in entryNode)
                    {
                        if (propNode.Name == "name")
                        {
                            entry.Name = propNode.InnerText;
                        }
                        else if (propNode.Name == "size")
                        {
                            entry.Size = 0;
                            long.TryParse(propNode.InnerText,out entry.Size);
                        }
                        else if (propNode.Name == "commit")
                        {
                            entry.Revision = int.Parse(propNode.Attributes[0].Value);
                            entry.Author = propNode["author"].InnerText;
                            //TODO: implement
                            //entry.Date = DateTime.Parse(propNode["date"].Value);
                        }
                        else
                        {
                            Debug.WriteLine("Unknown entry property: " + propNode.Name);
                        }
                    }

                    result.Add(entry);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError("Error reading Xml response: " + e.Message + "\n" +e.StackTrace);
            }
            xmlStream.Close();
            File.Delete(xmlPath);
            return result;
        }

        /*public static string[] ListRecursive(string repoUrl) {
            var names = Shell.Execute(@"svn list --depth infinity " + repoUrl).StdOut;
            return SplitLines(names);
        }*/

        /// <summary>
        /// Get information about a local or remote item.
        /// </summary>
        /// <param name="url">Repo URL or working copy path.</param>
        /// <returns></returns>
        public static InfoResult Info(string url) {
            var executeResult = Shell.Execute(@"svn info " + url);
            return new InfoResult(executeResult.StdOut);
        }

        public static string[] PropGet(string url, string prop) {
            var executeResult = Shell.Execute(@"svn propget " + prop + " " + url);
            return SplitLines(executeResult.StdOut);
        }

        /// <summary>
        /// Set svn property on a file or folder
        /// </summary>
        /// <param name="url">Working copy path</param>
        /// <param name="prop">Name of the property to set</param>
        /// <param name="propValue">Value to set</param>
        /// <returns>True if operation was successfull</returns>
        public static bool PropSet(string url, string prop, string propValue) {
            string tempFilePath = Shell.CurrentDirectory + "/svn_propset_temp";
            File.WriteAllText(tempFilePath, propValue);

            string command = @"svn propset " + prop + " -F " + tempFilePath + " " + url;

            var executeResult = Shell.Execute(command);
            if (!executeResult.Success) {
                PrintError("SetProp", executeResult.StdErr);
            }
            File.Delete(tempFilePath);
            return executeResult.Success;
        }

        /// <summary>
        /// Update the working copy to mirror a new URL within the repository.
        /// </summary>
        /// <param name="url">Repo URL to switch</param>
        /// <param name="workingCopyPath">Working copy path</param>
        /// <param name="revision">Repo specific revision</param>
        /// <param name="force">If used, unversioned obstructing paths in the working
        /// copy do not automatically cause a failure if the switch attempts to
        /// add the same path.If the obstructing path is the same type (file
        /// or directory) as the corresponding path in the repository it becomes
        /// versioned but its contents are left 'as-is' in the working copy.
        /// This means that an obstructing directory's unversioned children may
        /// also obstruct and become versioned.For files, any content differences
        /// between the obstruction and the repository are treated like a local
        /// modification to the working copy.All properties from the repository
        /// are applied to the obstructing path.</param>
        /// <returns></returns>
        public static bool Switch(string url, string workingCopyPath, string revision = null, bool force = false) {
            var executeResult = Shell.Execute(@"svn switch " + (force ? "--force" : "") + url + (revision == null ? "" : "@" + revision) + " " + workingCopyPath);
            if (!executeResult.Success) {
                PrintError("Switch", executeResult.StdErr);
            }

            return executeResult.Success;
        }

        /// <summary>
        /// Executes svn update command.
        /// </summary>
        /// <param name="path">Repo working copy path</param>
        /// <param name="revision">Source revision to copy, possible values:    HEAD(latest in repository), 
        ///                                                                     NUMBER(revision number), 
        ///                                                                     BASE(base rev of item's working copy), 
        ///                                                                     COMMITTED(last commit at or before BASE),
        ///                                                                     PREV(revision just before COMMITTED).</param>
        public static bool Update(string path, string revision = null) {
            var executeResult = Shell.Execute(@"svn update " + (revision ?? "") + " " + path);
            if (!executeResult.Success) {
                PrintError("Update", executeResult.StdErr);
            }

            return executeResult.Success;
        }
        /// <summary>
        /// Commit changes.
        /// </summary>
        /// <param name="message">Commit message</param>
        /// <param name="files">Array of file names or a full working copy path</param>
        /// <returns></returns>
        public static bool Commit(string message, params string[] files) {
            string command = "svn ci -m \"" + message + "\"";
            foreach (string file in files) {
                command += " " + file;
            }
            var executeResult = Shell.Execute(command);
            if (!executeResult.Success) {
                PrintError("Commit", executeResult.StdErr);
            }
            return executeResult.Success;

        }

        #endregion SVN_Common

        #region SVN_Extended

        /// <summary>
        /// Create a new branch using a HEAD revision.
        /// </summary>
        /// <param name="sourceUrl">Source repo URL.</param>
        /// <param name="destUrl">Destination repo URL.</param>
        public static bool BranchHead(string sourceUrl, string destUrl, string logMessage) {
            return Copy(sourceUrl, destUrl, logMessage);
        }

        /// <summary>
        /// Create a new branch using a specified revision.
        /// </summary>
        /// <param name="sourceUrl">Source repo URL.</param>
        /// <param name="destUrl">Destination repo URL.</param>
        /// <param name="revision">Target revision to branch</param>
        public static bool BranchRevision(string sourceUrl, string destUrl, string revision) {
            return Copy(sourceUrl, destUrl, revision);
        }

        /// <summary>
        /// Create a new branch using a working copy as a source.
        /// </summary>
        /// <param name="sourceUrl">Source working copy path.</param>
        /// <param name="destUrl">Destination repo URL.</param>
        public static bool BranchWorkingCopy(string workingCopyPath, string destUrl, string logMessage) {
            return Copy(workingCopyPath, destUrl,logMessage);
        }

        /// <summary>
        /// Get all branches for repo.
        /// </summary>
        /// <param name="repoUrl">Repo ROOT URL. If you have URL targeting some folder or file inside repo, use GetRepoRootUrl to get root link.</param>
        /// <param name="branches">OUT: branches in repo</param>
        /// <returns></returns>
        public static bool GetAllRepoBranches(string repoUrl, out string[] branches) {
            branches = null;
            var rootFolders = List(repoUrl);
            if (rootFolders == null || rootFolders.Length == 0 || !Array.Exists(rootFolders, v => v == "branches/")) {
                PrintError("GetAllRepoBranches", "Failed to find branches folder in repo " + repoUrl);
                return false;
            }

            var branchesUrl = repoUrl + "/branches";
            branches = List(branchesUrl);
            for (var i = 0; i < branches.Length; ++i) {
                if (branches[i].EndsWith("/")) {
                    branches[i] = branches[i].Remove(branches[i].Length - 1, 1);
                }
            }
            return true;
        }

        /// <summary>
        /// Returns root URL of repository.
        /// </summary>
        /// <param name="path">Working copy path or repo URL. May be targeted to any folder inside repo.</param>
        /// <returns></returns>
        public static string GetRepoRootUrl(string path) {
            string result = null;
            var executeResult = Shell.Execute(@"svn info --show-item repos-root-url " + path);
            if (executeResult.Success) {
                result = RemoveLineEndingsFromString(executeResult.StdOut);
            } else {
                PrintError("GetRepoRootUrl", executeResult.StdErr);
            }
            return result;
        }

        /// <summary>
        /// Returns the name of the branch from its URL
        /// </summary>
        /// <param name="url">Branch URL</param>
        /// <returns></returns>
        public static string GetBranchName(string url) {
            if (string.IsNullOrEmpty(url)) {
                return "";
            }
            int branchesId = url.LastIndexOf("/branches/");
            if (branchesId < 0) {
                //Given URL is not a branch
                return "";
            }
            //10 is the number of symbols in the string "/branches/"
            branchesId += 10;
            return url.Substring(branchesId);
            
        }

        /// <summary>
        /// Returns working copy root folder. If <paramref name="path"/> targets to some folder inside external repo, it returns local path to this external repo.
        /// </summary>
        /// <param name="path">Path to any file/folder inside working copy.</param>
        /// <returns></returns>
        public static string GetWorkingCopyRoot(string path) {
            string result = null;
            var executeResult = Shell.Execute(@"svn info --show-item wc-root " + path);
            if (executeResult.Success) {
                result = executeResult.StdOut;
            } else {
                PrintError("GetWorkingCopyRoot", executeResult.StdErr);
            }
            return result;
        }

        public static bool GetRepoNameFromUrl(string url, out string name) {
            name = null;
            if (string.IsNullOrEmpty(url)) {
                return false;
            }

            name = url.Remove(0, url.LastIndexOf('/') + 1);
            return true;
        }
        
        public static List<ExternalInfo> PropGetExternals(string url, bool recursive = false) {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<string> allExternals = new List<string>();
            var result = new List<ExternalInfo>();

            long svnPropGetMilliseconds = sw.ElapsedMilliseconds;
            var executeResult = Shell.Execute(@"svn propget svn:externals --xml " + (recursive ? " -R " : "") + url);


            if (!executeResult.Success) {
                Debug.WriteLine("Svn.GetPropExternals Failed! Shell command error");
                return new List<ExternalInfo>();
            }
            svnPropGetMilliseconds = sw.ElapsedMilliseconds - svnPropGetMilliseconds;
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(executeResult.StdOut);
            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode targetNode in xRoot.ChildNodes) {

                string local = targetNode.Attributes["path"].Value;

                foreach (XmlNode propNode in targetNode) {
                    if (propNode.Attributes["name"].Value == "svn:externals") {
                        ParseExternalProperty(local, result, propNode.InnerText);
                    }
                }
            }
            sw.Stop();
            return result;
        }

        private static void ParseExternalProperty(string local, List<ExternalInfo> result, string propertyValue) {
            var externals = SplitLines(propertyValue);
            foreach (var external in externals) {
                var target = new ExternalInfo();

                var splitCharId = external.IndexOf(' ');
                var externalUrl = external.Remove(splitCharId, external.Length - splitCharId);
                var wcFolderName = external.Remove(0, splitCharId + 1);

                target.FolderName = wcFolderName;
                var revisionSeparatorId = externalUrl.IndexOf('@');
                if (revisionSeparatorId != -1) {
                    target.RepoUrl = externalUrl.Remove(revisionSeparatorId, externalUrl.Length - revisionSeparatorId);
                    target.PinnedRevision = externalUrl.Remove(0, revisionSeparatorId + 1);
                }
                else {
                    target.RepoUrl = externalUrl;
                }

                target.WorkingCopyFolderPath = local;
                //Try to get external repo root URL directly from external URL. This is much faster than using SVN query, but will only work for repos with correct folder structure
                int rootEndId = target.RepoUrl.IndexOf("branches");
                if (rootEndId < 0) {
                    rootEndId = target.RepoUrl.IndexOf("tags");
                }
                if (rootEndId < 0) {
                    rootEndId = target.RepoUrl.IndexOf("trunk");
                }
                if (rootEndId > 0) {
                    target.RepoRoot = target.RepoUrl.Substring(0, rootEndId - 1);
                }
                else {
                    //Fallback to SVN query
                    target.RepoRoot = GetRepoRootUrl(target.RepoUrl);
                }
                result.Add(target);
            }
        }

        public static bool PropSetExternals(string url, string value) {
            return PropSet(url, "svn:externals", value);
        }

        #endregion SVN_Extended

        #region Utils
        private static void PrintError(string methodName, string errorMsg) {
            Debug.WriteLine(string.Format ("Method \"{0}\" failed with error: {1}", methodName, errorMsg));
        }
        

        private static string RemoveLineEndingsFromString(string source) {
            if (source == null) {
                return null;
            }

            return source.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        }
        #endregion
    }
}
