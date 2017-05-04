using System;
using System.IO;

namespace DOBAR.Helper
{
    internal static class Helper
    {
        internal static string WorkingDirectory
        {
            get
            {
                var tempPath = Environment.GetEnvironmentVariable("appdata") + @"\RoleBot";
                var d = new DirectoryInfo(tempPath);

                tempPath = d.FullName;

                if (!tempPath.EndsWith("\\"))
                {
                    tempPath = string.Concat(tempPath, "\\");
                }

                if (!d.Exists)
                {
                    Directory.CreateDirectory(tempPath);
                }

                return tempPath;
            }
        }
    }
}
