using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageRoller
{
    /// <summary>
    /// Control that contains info on a previously opened directory
    /// </summary>
    public partial class PastDirectory : UserControl
    {
        #region Fields / Properties

        const string FILE_NAME = "past_directories.txt";

        static List<DirectoryInfo> directories = new List<DirectoryInfo>();
        
        /// <summary>
        /// Control main area on mouse down
        /// </summary>
        public event EventHandler click;

        DirectoryInfo _directory;
        public DirectoryInfo directory
        {
            get
            {
                return _directory;
            }
            private set
            {
                _directory = value;

                // update labels
                dirLabel.Text = directory.Name;
                pathLabel.Text = directory.FullName;

                // add to the static list of directories
                directories.Add(directory);
            }
        }

        #endregion

        #region Constructors

        private PastDirectory()
        {
            InitializeComponent();
        }

        private PastDirectory(DirectoryInfo directory) : this()
        {
            this.directory = directory;
        }

        private PastDirectory(string path) : this(new DirectoryInfo(path)) { }

        #endregion

        #region Static Methods

        /// <summary>
        /// Attempts to create a PastDirectory control,
        /// returns null if one exists with the same directory,
        /// or if the path does not exist
        /// </summary>
        /// <param name="path">path of the directory</param>
        /// <returns>a PastDirectory control or null</returns>
        public static PastDirectory create(string path)
        {
            var directory = new DirectoryInfo(path);

            if (directory.Exists && !hasDirectory(directory.FullName))
            {
                return new PastDirectory(directory);
            }

            return null;
        }

        /// <summary>
        /// Gets all past opened directories from <see cref="DATA_PATH"/>
        /// </summary>
        /// <returns>IEnumerable of PastDirectories opened</returns>
        public static IEnumerable<PastDirectory> load()
        {
            string path = Path.Combine(App.DATA_PATH, FILE_NAME);
            App.createDirectoryAndFile(path);

            return File.ReadAllText(path)
                .Replace("\r", "")
                .Split('\n')
                .Select(x => create(x))
                .Where(x => x != null);
        }

        /// <summary>
        /// Saves all directory paths in <see cref="directories"/>
        /// to <see cref="App.DATA_PATH"/>\<see cref="FILE_NAME"/>
        /// </summary>
        public static void save()
        {
            string path = Path.Combine(App.DATA_PATH, FILE_NAME);
            App.createDirectoryAndFile(path);

            File.WriteAllText(path, string.Join("\n", directories.Select(x => x.FullName)));
        }

        /// <summary>
        /// Does the DirectoryInfo List (<see cref="directories"/>) contain the given directory
        /// </summary>
        /// <param name="directory">directory to check for</param>
        /// <returns>true if the passed directory is in the list</returns>
        public static bool hasDirectory(string path)
        {
            path = path.ToLower();
            return directories.Any(x => x.FullName.ToLower() == path);
        }

        #endregion

        #region Instance Events

        /// <summary>
        /// This acts as the click event for clicking somewhere on the usercontrol
        /// </summary>
        private void border_Click(object sender, MouseButtonEventArgs e)
        {
            if (click != null)
            {
                click(this, e);
            }
        }

        private void x_Click(object sender, RoutedEventArgs e)
        {
            // x button clicked - delete this contol, remove its directory from the static list
            if ((Parent as Panel) != null)
            {
                directories.Remove(directory);
                (Parent as Panel).Children.Remove(this);
            }
        }

        #endregion
    }
}
