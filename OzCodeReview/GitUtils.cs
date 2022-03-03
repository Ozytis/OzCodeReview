using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview
{
    public class GitUtils
    {
        public static string GetGitFolder(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            string gitDir = Path.Combine(filePath, ".git");

            if (Directory.Exists(gitDir))
            {
                return gitDir;
            }

            return GetGitFolder(Path.GetDirectoryName(filePath));
        }

        public static string GetCurrentBranch(string filePath)
        {
            string gitFoler = GetGitFolder(filePath);

            if (string.IsNullOrEmpty(gitFoler))
            {
                return null;
            }

            string headFile = Path.Combine(gitFoler, "HEAD");

            if (!File.Exists(headFile))
            {
                return null;
            }

            string content = File.ReadAllText(headFile);

            if (string.IsNullOrEmpty(content))
            {
                return null;
            }
        
            return content.Substring("ref: refs/heads/".Length).Trim();
        }

        public static string GetLastCommit(string filePath, string branchName)
        {

            string gitFoler = GetGitFolder(filePath);

            if (string.IsNullOrEmpty(gitFoler))
            {
                return null;
            }

            string branchRef = Path.Combine(gitFoler, $"refs\\heads\\{branchName}");

            return File.ReadAllText(branchRef).Trim();
        }
    }
}
