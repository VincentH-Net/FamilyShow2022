using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.FamilyShow.Views;

public partial class AddPersonView
{
    public AddPersonView()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetService<AddPersonViewModel>()!;

        SetDefaultFocus();
    }

    internal AddPersonViewModel ViewModel
    {
        get => (AddPersonViewModel)DataContext;
        private init => DataContext = value;
    }

    public void ClearInputFields()
    {
        ViewModel.ClearInputFields();
    }

    public void SetDefaultFocus()
    {
        FirstNameInputTextBox.Focus();
    }

    private void AvatarPhoto_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop, true) is string[] { Length: > 0 } fileNames)
        {
            ViewModel.AvatarPhotoPath = fileNames[0];
            ViewModel.AvatarPhotoSource = new BitmapImage(new Uri(ViewModel.AvatarPhotoPath));
        }

        // Mark the event as handled, so the control's native Drop handler is not called.
        e.Handled = true;
    }
}