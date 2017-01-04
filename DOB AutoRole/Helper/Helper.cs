using System;
using System.Collections.Generic;
using System.Text;

namespace DOB_AutoRole.Helper
{
    internal static class Helper
    {
        internal static string WorkingDirectory
        {
            get
            {
                var tempPath = Environment.GetEnvironmentVariable("appdata") + @"\Nep Bot";
                var d = new DirectoryInfo(tempPath);

                tempPath = d.FullName;

                if (!tempPath.EndsWith("\\"))
                {
                    tempPath = string.Concat(tempPath, "\\");
                }

                if (!d.Exists)
                {
                    Debug("Creating working directory...");
                    Directory.CreateDirectory(tempPath);
                }


                return tempPath;
            }
        }
    }
}
