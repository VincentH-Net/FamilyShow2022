using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.FamilyShow.Properties;

namespace Microsoft.FamilyShow.Views;

public class OldVersionMessageViewModel : ObservableObject
{
    private bool dontShow;

    public OldVersionMessageViewModel()
    {
        Ok = new RelayCommand(OkExec);
    }

    public bool DontShowAgain
    {
        get => dontShow;
        set => SetProperty(ref dontShow, value);
    }

    public Action? GoContinue { get; set; }

    public ICommand Ok { get; }

    private void OkExec()
    {
        GoContinue?.Invoke();

        Settings.Default.DontShowOldVersionMessage = DontShowAgain;
        Settings.Default.Save();
    }
}