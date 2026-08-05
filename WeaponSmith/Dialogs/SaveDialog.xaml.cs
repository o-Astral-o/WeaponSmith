using System.Windows;

namespace WeaponSmith.Dialogs;

public enum UnsavedChangesResult
{
    Save,
    Discard,
    Cancel,
}

public partial class SaveDialog : Window
{
    public UnsavedChangesResult Result { get; private set; } = UnsavedChangesResult.Cancel;

    public SaveDialog(string fileName)
    {
        InitializeComponent();
        MessageTextBlock.Text = $"Do you want to save changes to \"{fileName}\"?";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Result = UnsavedChangesResult.Save;
        DialogResult = true;
    }

    private void DontSave_Click(object sender, RoutedEventArgs e)
    {
        Result = UnsavedChangesResult.Discard;
        DialogResult = true;
    }
}
