using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Nettle.SVN {
    public class ExternalInfo {
        public string FolderName;
        public string WorkingCopyFolderPath;
        public string RepoUrl;
        public string PinnedRevision;
        public string RepoRoot;

        public bool IsPinned {
            get {
                return !string.IsNullOrEmpty(PinnedRevision);
            }
        } 
    }
}
