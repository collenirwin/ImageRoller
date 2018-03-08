using BoinWPF;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageRoller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        /// <summary>
        /// Directory currently open
        /// </summary>
        DirectoryInfo directory;

        Random ran = new Random();

        /// <summary>
        /// Common image file extensions
        /// </summary>
        string[] imageExtensions = new string[]
        { 
            ".jpg",
            ".jpeg",
            ".png",
            ".bmp",
            ".bitmap",
            ".gif",
            ".tiff"
        };

        #endregion

        #region Methods

        #region Constructor, Window Events

        public MainWindow()
        {
            InitializeComponent();

            // load all past directories and throw 'em into the content StackPanel
            try
            {
                var pastDirs = PastDirectory.load();

                foreach (var dir in pastDirs)
                {
                    addPastDirectory(dir);
                }
            }
            catch (Exception ex)
            {
                Alert.showMoreInfoDialog("There was a problem loading your directory history",
                    ex.Message, App.NAME);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // write all past opened directories to their file
            PastDirectory.save();
        }

        #endregion

        #region Main Menu

        /// <summary>
        /// Adds a <see cref="PastDirectory"/> control to the <see cref="content"/> StackPanel
        /// </summary>
        /// <param name="pastDir"><see cref="PastDirectory"/> control to add</param>
        private void addPastDirectory(PastDirectory pastDir)
        {
            pastDir.click += pastDirectory_click;
            content.Children.Add(pastDir);
        }

        private void pastDirectory_click(object sender, EventArgs e)
        {
            // open the selected directory
            openDirectory((sender as PastDirectory).directory);
        }

        private void newDirectory_Click(object sender, RoutedEventArgs e)
        {
            // let the user select a directory, then open it
            var dialog = new FolderBrowserDialog();

            var result = dialog.ShowDialog();

            // unfortunately there is a namespace collision here if I'm not this specific
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var pastDir = PastDirectory.create(dialog.SelectedPath);
                if (pastDir != null)
                {
                    addPastDirectory(pastDir);
                    openDirectory(pastDir.directory);
                }
                else if (PastDirectory.hasDirectory(dialog.SelectedPath))
                {
                    openDirectory(new DirectoryInfo(dialog.SelectedPath));
                }
                else
                {
                    Alert.showDialog("Cannot access the specified folder", App.NAME);
                }
            }
        }

        private void openDirectory(DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                Alert.showDialog(
                    string.Format("Cannot find the following folder: \"{0}\"", directory.FullName), App.NAME);
                return;
            }

            // switch to the imageContent screen
            switchStackPanels(imageContent, content);

            // set the current directory
            this.directory = directory;

            // get a random image
            roll();
        }

        #endregion

        #region Image Screen

        /// <summary>
        /// Pick a random image from within <see cref="directory"/>
        /// and displays it in <see cref="image"/>
        /// </summary>
        private void roll()
        {
            // default the file label to "Not found" or similar
            fileLabel.Text = fileLabel.Tag.ToString();

            // default to no image
            image.Source = null;

            // get out if we have a bad directory
            if (directory == null || !directory.Exists)
            {
                return;
            }

            // get all image files with the directory
            var files = getImageFiles(directory, (bool)recursive.IsChecked);

            // get out if there aren't any files within the directory
            if (!files.Any())
            {
                return;
            }

            // grab a random image from the list
            var file = files[ran.Next(files.Length)];

            // get the bitmap of the image file
            var bmp = new BitmapImage(new Uri(file.FullName));

            // show the image
            image.Source = bmp;

            // let's also give this to the dockpanel
            dockPanel.Background = new ImageBrush(bmp);
            dockPanel.Background.Opacity = 0.5;

            // display the image's name
            fileLabel.Text = file.Name;
        }

        /// <summary>
        /// Gets an array of all image files within the given directory
        /// </summary>
        /// <param name="directory">directory to search within</param>
        /// <param name="recursive">search recursively within?</param>
        /// <returns>Array of all images with directory as FileInfo objects</returns>
        private FileInfo[] getImageFiles(DirectoryInfo directory, bool recursive = false)
        {
            var files = directory
                .GetFiles()
                .Where(x => imageExtensions.Contains(x.Extension.ToLower()))
                .ToArray();

            if (recursive)
            {
                // go through each directory within this one and add the images to the array
                foreach (var dir in directory.GetDirectories())
                {
                    files = files.Concat(getImageFiles(dir, recursive)).ToArray();
                }
            }

            return files;
        }

        private void roll_Click(object sender, RoutedEventArgs e)
        {
            // get a random image
            roll();
        }

        private void copy_Click(object sender, RoutedEventArgs e)
        {
            // explicit due to namespace collision
            System.Windows.Clipboard.SetImage((BitmapSource)image.Source);
        }

        private void mainMenu_Click(object sender, RoutedEventArgs e)
        {
            // back to main menu
            switchStackPanels(content, imageContent);
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Positions the given StackPanel at the end of <see cref="dockPanel"/>
        /// </summary>
        /// <param name="panel">StackPanel to move to the end</param>
        private void positionAtEnd(StackPanel panel)
        {
            dockPanel.Children.Remove(panel);
            dockPanel.Children.Add(panel);
        }

        private void switchStackPanels(StackPanel newPanel, StackPanel oldPanel)
        {
            // show newPanel instead of oldPanel
            oldPanel.Visibility = Visibility.Collapsed;
            newPanel.Visibility = Visibility.Visible;

            // move newPanel to the end of dockPanel.Children so it fills the screen
            positionAtEnd(newPanel);

            // scroll to the top
            scrollViewer.ScrollToTop();
        }

        #endregion

        #endregion
    }
}
