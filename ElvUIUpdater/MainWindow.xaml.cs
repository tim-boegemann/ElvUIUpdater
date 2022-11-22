using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace ElvUIUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string AddonFolderPathAppend = @"_retail_\Interface\AddOns";
        private const string ElvUiFolderAppend = "ElvUI";
        private const string ElvUiLibFolderAppend = "ElvUI_Libraries";
        private const string ElvUiOptionsFolderAppend = "ElvUI_Options";
        private const string ElvUiUpdaterFolderAppend = "ElvUIUpdater";
        private const string Install = "Install";
        private const string Update = "Update";
        private string _operationPath;
        private string _appDataFolder;
        private string _initFilePath;
        private string _fullAddonFolderPath;
        private bool _autoUpdate = false;
        private GridWrapper _gitWrapper = new GridWrapper();


        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var info = Directory.CreateDirectory(Path.Combine(_appDataFolder, ElvUiUpdaterFolderAppend));
                _operationPath = info.FullName;
                _initFilePath = Path.Combine(_operationPath, "init.txt");
                TryReadInitfile();
                UpdateButtonState();
                if (_autoUpdate)
                {
                    UpdateElvui();
                }
            }
            catch (Exception e)
            {
                MessageWindow.Text += e.Message;
            }
        }

        private bool TryReadInitfile()
        {
            if (File.Exists(_initFilePath))
            {
                var lines = File.ReadLines(_initFilePath).ToArray();
                PathBox.Text = lines[0];
                if(lines[1] != null && bool.TryParse(lines[1], out var autoUpdate))
                    _autoUpdate = autoUpdate;
                AutoUpdate.IsChecked = _autoUpdate;
                return true;
            }
            return false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (UpdateButton == null)
                return;

            var wowPath = PathBox.Text;
            if (!Directory.Exists(wowPath))
            {
                UpdateButton.IsEnabled = false;
                return;
            }

            UpdateButton.IsEnabled = true;
            _fullAddonFolderPath = Path.Combine(wowPath, AddonFolderPathAppend);
            var elvuiFolderPath = Path.Combine(_fullAddonFolderPath, ElvUiFolderAppend);
            if (Directory.Exists(elvuiFolderPath))
                UpdateButton.Content = Update;
            else
                UpdateButton.Content = Install;
        }

        private void UpdateElvui()
        {
            switch (UpdateButton.Content.ToString())
            {
                case Install:
                    CreateElvUiRepo();
                    MessageWindow.Text = "Installed ElvUI";
                    break;

                case Update:
                    if (!Directory.Exists(Path.Combine(_operationPath, ElvUiFolderAppend)))
                        CreateElvUiRepo();
                    UpdateElvUiRepo();
                    MessageWindow.Text = "Updated ElvUI";
                    break;

                default:
                    return;
            }
            MoveElvuiToAddon();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateElvui();
            }
            catch (Exception ex)
            {
                MessageWindow.Text += ex.Message;
            }
        }

        private void MoveElvuiToAddon()
        {
            if (_fullAddonFolderPath == null)
                return;

            var elvuiRepoPath = Path.Combine(_operationPath, ElvUiFolderAppend);
            var elvuiPath = Path.Combine(elvuiRepoPath, ElvUiFolderAppend);
            CopyFilesRecursively(elvuiPath, Path.Combine(_fullAddonFolderPath, ElvUiFolderAppend));
            var elvuiLibPath = Path.Combine(elvuiRepoPath, ElvUiLibFolderAppend);
            CopyFilesRecursively(elvuiLibPath, Path.Combine(_fullAddonFolderPath, ElvUiLibFolderAppend));
            var elvuiOptionsPath = Path.Combine(elvuiRepoPath, ElvUiOptionsFolderAppend);
            CopyFilesRecursively(elvuiOptionsPath, Path.Combine(_fullAddonFolderPath, ElvUiOptionsFolderAppend));
        }
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }

        private void UpdateElvUiRepo()
        {
            var targetPath = Path.Combine(_operationPath, ElvUiFolderAppend);
            _gitWrapper.Update(targetPath);
        }

        private void CreateElvUiRepo()
        {
            Directory.CreateDirectory(_operationPath);
            _gitWrapper.Clone(_operationPath);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var path = PathBox.Text;
            File.WriteAllLines(_initFilePath, new[] { path, _autoUpdate.ToString() });
        }

        private void AutoUpdate_Checked(object sender, RoutedEventArgs e)
        {
            _autoUpdate = true;
        }
        private void AutoUpdate_Unchecked(object sender, RoutedEventArgs e)
        {
            _autoUpdate = false;
        }
    }
}