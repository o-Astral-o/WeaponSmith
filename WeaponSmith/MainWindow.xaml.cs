using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using WeaponSmith.Dialogs;
using WeaponSmith.Util;

namespace WeaponSmith;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<WeaponFileEntry> _allEntries = new();
    private ObservableCollection<WeaponFileEntry> _allItems = new();
    private ObservableCollection<WeaponFileEntry> _generalItems = new();
    private ObservableCollection<WeaponFileEntry> _damageItems = new();
    private ObservableCollection<WeaponFileEntry> _modelsItems = new();
    private ObservableCollection<WeaponFileEntry> _animsItems = new();
    private ObservableCollection<WeaponFileEntry> _stateTimerItems = new();
    private ObservableCollection<WeaponFileEntry> _soundsItems = new();
    private ObservableCollection<WeaponFileEntry> _fxItems = new();
    private ObservableCollection<WeaponFileEntry> _uiItems = new();
    private ObservableCollection<WeaponFileEntry> _miscItems = new();
    private string? _currentFilePath;
    private string? _currentFileHeader;
    private bool _isDirty;
    private readonly Config _config;
    private const int MaxRecentFiles = 5;
    private static readonly string AppDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WeaponSmith"
    );
    private static readonly string ConfigPath = Path.Combine(
        AppDataPath,
        "config.cfg"
    );
    
    public MainWindow()
    {
        InitializeComponent();
        AllDataGrid.ItemsSource = _allItems;
        GeneralDataGrid.ItemsSource = _generalItems;
        DamageDataGrid.ItemsSource = _damageItems;
        ModelsDataGrid.ItemsSource = _modelsItems;
        AnimsDataGrid.ItemsSource = _animsItems;
        StateTimerDataGrid.ItemsSource = _stateTimerItems;
        SoundsDataGrid.ItemsSource = _soundsItems;
        FXDataGrid.ItemsSource = _fxItems;
        UIDataGrid.ItemsSource = _uiItems;
        MiscDataGrid.ItemsSource = _miscItems;
        _config = Config.Load(ConfigPath);
        if (!File.Exists(ConfigPath))
        {
            try
            {
                _config.Save(ConfigPath);
            }
            catch
            {
                // Silently fail if we can't write the default config
            }
        }
        UpdateRecentFilesMenu();
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                LoadWeaponFile(files[0]);
            }
        }
    }

    private void LoadWeaponFile(string path)
    {
        if (!ConfirmUnsavedChanges())
        {
            return;
        }

        try
        {
            var weaponFile = new WeaponFile(path, _config.FileHeaders);
            _allEntries = weaponFile.Entries;
            _currentFilePath = path;
            _currentFileHeader = weaponFile.Header;
            _isDirty = false;
            foreach (var entry in _allEntries)
            {
                entry.PropertyChanged += Entry_PropertyChanged;
            }
            SearchTextBox.Text = string.Empty;
            UpdateDataGrids();
            SaveMenuItem.IsEnabled = true;
            SaveAsMenuItem.IsEnabled = true;
            AddToRecentFiles(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Entry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WeaponFileEntry.Key) or nameof(WeaponFileEntry.Value))
        {
            _isDirty = true;
        }
    }

    /// <summary>
    /// If there are unsaved changes, asks the user whether to save them.
    /// Returns true if the caller should proceed (saved or discarded), false to cancel.
    /// </summary>
    private bool ConfirmUnsavedChanges()
    {
        if (!_isDirty || string.IsNullOrEmpty(_currentFilePath))
        {
            return true;
        }

        var dialog = new SaveDialog(Path.GetFileName(_currentFilePath)) { Owner = this };
        dialog.ShowDialog();

        switch (dialog.Result)
        {
            case UnsavedChangesResult.Save:
                try
                {
                    SaveFile(_currentFilePath);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            case UnsavedChangesResult.Discard:
                return true;
            default:
                return false;
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (!ConfirmUnsavedChanges())
        {
            e.Cancel = true;
        }
    }

    private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateDataGrids();
    }

    private void UpdateDataGrids()
    {
        var searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;
        var filteredEntries = string.IsNullOrWhiteSpace(searchText)
            ? _allEntries
            : _allEntries.Where(entry => entry.Key.ToLower().Contains(searchText)).ToList();

        _allItems.Clear();
        _generalItems.Clear();
        _damageItems.Clear();
        _modelsItems.Clear();
        _animsItems.Clear();
        _stateTimerItems.Clear();
        _soundsItems.Clear();
        _fxItems.Clear();
        _uiItems.Clear();
        _miscItems.Clear();

        foreach (var entry in filteredEntries)
        {
            _allItems.Add(entry);
            
            switch (entry.Category)
            {
                case EntryCategory.General:
                    _generalItems.Add(entry);
                    break;
                case EntryCategory.Damage:
                    _damageItems.Add(entry);
                    break;
                case EntryCategory.Model:
                    _modelsItems.Add(entry);
                    break;
                case EntryCategory.Anim:
                    _animsItems.Add(entry);
                    break;
                case EntryCategory.StateTimers:
                    _stateTimerItems.Add(entry);
                    break;
                case EntryCategory.Sound:
                    _soundsItems.Add(entry);
                    break;
                case EntryCategory.FX:
                    _fxItems.Add(entry);
                    break;
                case EntryCategory.UI:
                    _uiItems.Add(entry);
                    break;
                case EntryCategory.Misc:
                    _miscItems.Add(entry);
                    break;
            }
        }
    }
    
    private void Open_MenuItemClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "All Files (*.*)|*.*",
            Title = "Open Weapon File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            LoadWeaponFile(openFileDialog.FileName);
        }
    }

    private void Save_MenuItemClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            MessageBox.Show("No file loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        
        try
        {
            SaveFile(_currentFilePath);
            // MessageBox.Show("File saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            // MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveAs_MenuItemClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            MessageBox.Show("No file loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = System.IO.Path.GetFileName(_currentFilePath),
            InitialDirectory = System.IO.Path.GetDirectoryName(_currentFilePath),
            Filter = "All Files (*.*)|*.*"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                SaveFile(saveFileDialog.FileName);
                // MessageBox.Show("File saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SaveFile(string path)
    {
        if (string.IsNullOrEmpty(_currentFileHeader))
        {
            throw new Exception("Unknown file type");
        }

        var content = _currentFileHeader;
        foreach (var entry in _allEntries)
        {
            content += $"\\{entry.Key}\\{entry.Value}";
        }

        File.WriteAllText(path, content);
        _isDirty = false;
    }

    private void AddToRecentFiles(string filePath)
    {
        try
        {
            // Remove if already exists (to avoid duplicates)
            _config.RecentFiles.Remove(filePath);

            // Add to the top
            _config.RecentFiles.Insert(0, filePath);

            // Keep only the most recent 5
            if (_config.RecentFiles.Count > MaxRecentFiles)
            {
                _config.RecentFiles.RemoveRange(MaxRecentFiles, _config.RecentFiles.Count - MaxRecentFiles);
            }

            _config.Save(ConfigPath);
            UpdateRecentFilesMenu();
        }
        catch
        {
            // Silently fail if we can't save recent files
        }
    }

    private void UpdateRecentFilesMenu()
    {
        OpenRecentMenuItem.Items.Clear();

        var recentFiles = _config.RecentFiles.Where(File.Exists).ToList();

        if (recentFiles.Count == 0)
        {
            var noFilesItem = new System.Windows.Controls.MenuItem
            {
                Header = "No recent files",
                IsEnabled = false
            };
            OpenRecentMenuItem.Items.Add(noFilesItem);
            OpenRecentMenuItem.IsEnabled = false;
        }
        else
        {
            OpenRecentMenuItem.IsEnabled = true;
            
            foreach (var filePath in recentFiles)
            {
                var fileName = Path.GetFileName(filePath);
                var menuItem = new System.Windows.Controls.MenuItem
                {
                    Header = fileName,
                    ToolTip = filePath,
                    Tag = filePath
                };
                
                menuItem.Click += RecentFile_MenuItemClick;
                OpenRecentMenuItem.Items.Add(menuItem);
            }
        }
    }

    private void RecentFile_MenuItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem menuItem && menuItem.Tag is string filePath)
        {
            if (File.Exists(filePath))
            {
                LoadWeaponFile(filePath);
            }
            else
            {
                MessageBox.Show("File no longer exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Remove from recent files
                _config.RecentFiles.Remove(filePath);
                _config.Save(ConfigPath);
                UpdateRecentFilesMenu();
            }
        }
    }

    private void AlwaysOnTopCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
    {
        Topmost = AlwaysOnTopCheckBox.IsChecked == true;
    }
}