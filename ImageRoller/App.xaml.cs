using System;
using System.IO;
using System.Windows;

namespace ImageRoller
{
    public partial class App : Application
    {
        /// <summary>
        /// Name of the app
        /// </summary>
        public const string NAME = "ImageRoller";

        /// <summary>
        /// C:\Users\[name]\AppData\Local\[App.NAME]
        /// </summary>
        public static readonly string DATA_PATH = Path.Combine(
           Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
           App.NAME);

        /// <summary>
        /// Creates the file's parent directory if it hasn't been created,
        /// then creates the specified (empty) file within it if needed
        /// </summary>
        /// <param name="path">Full path to file</param>
        public static void createDirectoryAndFile(string path)
        {
            string dirPath = Path.Combine(path, "../");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (!File.Exists(path))
            {
                using (var writer = File.CreateText(path))
                {
                    writer.Write("");
                }
            }
        }
    }
}
