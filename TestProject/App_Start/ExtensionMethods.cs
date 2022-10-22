using System;

namespace TestProject.App_Start
{
    public static class ExtensionMethods
    {
        public static string GetFileSize(this long length)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (length >= 1024 && order < sizes.Length - 1)
            {
                order++;
                length = length / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", length, sizes[order]);
            return result;
        }
    }
}