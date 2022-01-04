using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.FamilyShow.Messages;
using Microsoft.FamilyShow.Properties;
using Microsoft.FamilyShow.Services;

namespace Microsoft.FamilyShow;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly Settings appSettings = Settings.Default;

    private string? statusMessage;

    public MainWindowViewModel(IUserInterfaceConnector connector, IAppConfiguration appConfiguration, ICommonDialogFactory commonDialogService, IFamilyDataStoreService dataStoreService)
    {
        Connector = connector;
        ConfigurationService = appConfiguration;
        DialogFactory = commonDialogService;
        DataStoreService = dataStoreService;
    
        VisitVertigoWebSite = new RelayCommand(VisitVerigoWebSiteGo);
        ExportGedcom = new RelayCommand(ExportGedcomExec);
        ImportGedcom = new RelayCommand(ImportGedcomExec);
        Save = new RelayCommand(SaveExec);
        SaveAs = new RelayCommand(SaveAsExec);
        SaveXps = new RelayCommand(SaveXpsExec);
        WhatIsGedcom = new RelayCommand(WhatIsGedcomExec);
        ExportBirth = new RelayCommand(ExportBirthExec);
        Open = new RelayCommand<string>(OpenExec);
        Close = new RelayCommand(CloseExec);
        Create = new RelayCommand(CreateExec);
        ChangeSkin = new RelayCommand<string>(ChangeSkinExec);
        VersionMessageClosed = new RelayCommand(VersionMessageClosedExec);

        WeakReferenceMessenger.Default.Register<FamilyDataOpenedMessage>(this, (r, m) =>
        {
            // Handle the message here, with r being the recipient and m being the
            // input message. Using the recipient passed as input makes it so that
            // the lambda expression doesn't capture "this", improving performance.
            //BuildOpenMenu();
            //ShowDetailsPane();
        });

        WeakReferenceMessenger.Default.Register<FamilyDataImportedMessage>(this, (r, m) =>
        {
            //ShowDetailsPane();
        });

        WeakReferenceMessenger.Default.Register<FamilyDataCreateMessage>(this, (r, m) =>
        {
            //ShowNewUserControl();
            //family.IsDirty = false;
        });

        SkinNames = ConfigurationService.Skins.Keys.ToArray();
        StatusMessage = "Welcome to FamilyShow 2022";
    }

    public IAppConfiguration ConfigurationService { get; set; }

    public IUserInterfaceConnector Connector { get; set; }

    public IFamilyDataStoreService DataStoreService { get; set; }

    public ICommonDialogFactory DialogFactory { get; set; }

    public string[]? SkinNames { get; set; }

    public string? StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    public void Configure()
    {
        ConfigurationService.LoadRecentFiles();
    }

    private static void OpenUrl(string url)
    {
        // https://github.com/dotnet/runtime/issues/28005

        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}