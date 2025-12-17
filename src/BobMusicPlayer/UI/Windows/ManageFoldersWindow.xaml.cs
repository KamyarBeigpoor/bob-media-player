using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BobMediaPlayer.Library;
using WinForms = System.Windows.Forms;

namespace BobMediaPlayer
{
    public partial class ManageFoldersWindow : Window
    {
        private LibrarySettings _settings;

        public ManageFoldersWindow()
        {
            InitializeComponent();
            _settings = LibrarySettingsStore.Load();
            LoadFolders();
        }

        private void LoadFolders()
        {
            try
            {
                var folders = _settings.Folders.ToList();
                FoldersList.ItemsSource = null;
                FoldersList.ItemsSource = folders;
                FoldersList.Items.Refresh();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading folders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dlg = new WinForms.FolderBrowserDialog())
                {
                    dlg.Description = "Select a folder to include in your music library";
                    dlg.ShowNewFolderButton = false;
                    if (dlg.ShowDialog() == WinForms.DialogResult.OK)
                    {
                        var path = dlg.SelectedPath;
                        if (!string.IsNullOrWhiteSpace(path) && !_settings.Folders.Contains(path, StringComparer.OrdinalIgnoreCase))
                        {
                            _settings.Folders.Add(path);
                            LoadFolders();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error adding folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = FoldersList.SelectedItems.Cast<string>().ToList();
                if (selected.Count == 0) 
                {
                    System.Windows.MessageBox.Show("Please select folders to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                _settings.Folders = _settings.Folders.Where(f => !selected.Contains(f)).ToList();
                LoadFolders();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error removing folders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.Folders = LibrarySettingsStore.GetDefaultFolders();
            LoadFolders();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Normalize duplicates
            _settings.Folders = _settings.Folders.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            LibrarySettingsStore.Save(_settings);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
