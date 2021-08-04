using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Nettle.SVN.Svn;

namespace Nettle.SVN {
    public class InfoResult {
        public string Path;
        public string URL;
        public string RelativeURL;
        public string RepositoryRoot;
        public string RepositoryUUID;
        public string WorkingCopyRootPath;
        public int Revision;
        public NodeType NodeKind;
        public string LastChangedAuthor;
        public int LastChangedRev;
        public DateTime LastChangedDate;

        public InfoResult(string svnOutput) {
            string[] lines = SplitLines(svnOutput);

            foreach (var line in lines) {
                int nameEnd = line.IndexOf(':');
                string propName = line.Substring(0, nameEnd);
                string propValue = line.Substring(nameEnd + 1).Trim();

                if (propName.Equals("Path", StringComparison.InvariantCultureIgnoreCase)) {
                    Path = propValue;
                } else if (propName.Equals("Last Changed Author", StringComparison.InvariantCultureIgnoreCase)) {
                    LastChangedAuthor = propValue;
                } else if (propName.Equals("Last Changed Rev", StringComparison.InvariantCultureIgnoreCase)) {
                    LastChangedRev = int.Parse(propValue);
                } else if (propName.Equals("URL", StringComparison.InvariantCultureIgnoreCase)) {
                    URL = propValue;
                } else if (propName.Equals("Relative URL", StringComparison.InvariantCultureIgnoreCase)) {
                    RelativeURL = propValue;
                } else if (propName.Equals("Repository Root", StringComparison.InvariantCultureIgnoreCase)) {
                    RepositoryRoot = propValue;
                } else if (propName.Equals("Repository UUID", StringComparison.InvariantCultureIgnoreCase)) {
                    RepositoryUUID = propValue;
                } else if (propName.Equals("Revision", StringComparison.InvariantCultureIgnoreCase)) {
                    Revision = int.Parse(propValue);
                } else if (propName.Equals("Node Kind", StringComparison.InvariantCultureIgnoreCase)) {

                    if (propValue.Equals("directory", StringComparison.InvariantCultureIgnoreCase)) {
                        NodeKind = NodeType.Directory;
                    } else if (propValue.Equals("file", StringComparison.InvariantCultureIgnoreCase)) {
                        NodeKind = NodeType.File;
                    } else {
                        throw new Exception("Unknown node kind " + propValue);
                    }
                } else if (propName.Equals("Last Changed Date", StringComparison.InvariantCultureIgnoreCase)) {
                    propValue = propValue.Substring(0, propValue.IndexOf('+'));

                    if (!DateTime.TryParse(propValue, out LastChangedDate)) {
                        Debug.WriteLine("Failed to parse date");
                    }
                }
                else if (propName.Equals("Working Copy Root Path")){
                    WorkingCopyRootPath = propValue;
                }
                else {
                    Debug.WriteLine("Unknown property " + propName);
                }
            }
        }
    }
}
