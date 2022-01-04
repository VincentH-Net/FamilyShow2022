using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FamilyShow.Framework;
using Microsoft.FamilyShow.Messages;
using Microsoft.FamilyShow.Properties;
using Microsoft.FamilyShow.Services;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Views;

public class WelcomeViewModel : ObservableRecipient
{
    private List<StringPair> recentFiles = new();

    public WelcomeViewModel(IAppConfiguration configuration, ICommonDialogFactory dialogFactory)
    {
        AppConfiguration = configuration;
        DialogFactory = dialogFactory;

        Create = new RelayCommand(CreateExec);
        Open = new RelayCommand(OpenExec);
        Import = new RelayCommand(ImportExec);
        OpenRecent = new RelayCommand<string>(OpenRecentExec!);

        var version = Assembly.GetExecutingAssembly().GetName().Version!;
        Version = string.Format(CultureInfo.CurrentCulture, $"{version.Major}.{version.Minor}.{version.Build}");

        AppConfiguration = App.Current.Services.GetService<IAppConfiguration>()!;

        var files = AppConfiguration.RecentFiles.Select(file => new StringPair(Path.GetFileName(file), file)).ToList();

        RecentFiles = files;
    }

    public IAppConfiguration AppConfiguration { get; set; }

    public ICommand Create { get; }

    public ICommonDialogFactory DialogFactory { get; set; }

    public ICommand Import { get; }

    public ICommand Open { get; }

    public ICommand OpenRecent { get; }

    public List<StringPair> RecentFiles
    {
        get => recentFiles;
        set => SetProperty(ref recentFiles, value);
    }

    public string Version { get; }

    private void CreateExec()
    {
        var family = App.Family;
        var familyCollection = App.FamilyCollection;
        family.Clear();
        familyCollection.FullyQualifiedFilename = null;
        family.OnContentChanged();

        Messenger.Send(new FamilyDataCreateMessage());
    }

    private void ImportExec()
    {
        var dialog = DialogFactory.Create();

        dialog.InitialDirectory = People.ApplicationFolderPath;

        dialog.Filter.Add(new FilterEntry(Resources.GedcomFiles, Resources.GedcomExtension));
        dialog.Filter.Add(new FilterEntry(Resources.AllFiles, Resources.AllExtension));
        dialog.Title = Resources.Import;
        var result = dialog.ShowOpen();

        if (!result) return;

        if (string.IsNullOrEmpty(dialog.FileName))
            return;

        var family = App.Family;
        var familyCollection = App.FamilyCollection;

        var ged = new GedcomImport();
        ged.Import(family, dialog.FileName);
        familyCollection.FullyQualifiedFilename = string.Empty;

        Messenger.Send(new FamilyDataImportedMessage(dialog.FileName, familyCollection.FullyQualifiedFilename));

        // FamilyDataImportedMessage

        // ShowDetailsPane();


#if false
PromptToSave();

      CommonDialog dialog = new CommonDialog
      {
        InitialDirectory = People.ApplicationFolderPath
      };

      dialog.Filter.Add(new FilterEntry(Properties.Resources.GedcomFiles, Properties.Resources.GedcomExtension));
      dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
      dialog.Title = Properties.Resources.Import;
      dialog.ShowOpen();

      if (!string.IsNullOrEmpty(dialog.FileName))
      {
        try
        {
          GedcomImport ged = new GedcomImport();
          ged.Import(family, dialog.FileName);
          familyCollection.FullyQualifiedFilename = string.Empty;

          ShowDetailsPane();
          family.IsDirty = false;
        }
        catch
        {
          // Could not import the GEDCOM for some reason. Handle
          // all exceptions the same, display message and continue
          /// without importing the GEDCOM file.
          MessageBox.Show(this, Properties.Resources.GedcomFailedMessage,
              Properties.Resources.GedcomFailed, MessageBoxButton.OK,
              MessageBoxImage.Information);
        }
#endif
    }

    private static void LoadFamily(string fileName)
    {
        var family = App.Family;
        var familyCollection = App.FamilyCollection;
        familyCollection.FullyQualifiedFilename = fileName;

        // Load the selected family file based on the file extension
        if (fileName.EndsWith(Resources.DefaultFamilyExtension))
        {
            familyCollection.LoadOPC();
        }
        else
        {
            family.IsOldVersion = true;
            familyCollection.LoadVersion2();
        }
    }

    private void OpenExec()
    {
        var dialog = DialogFactory.Create();
        dialog.InitialDirectory = People.ApplicationFolderPath;

        dialog.Filter.Add(new FilterEntry(Resources.FamilyFiles, Resources.FamilyExtensions));
        dialog.Filter.Add(new FilterEntry(Resources.FamilyV3Files, Resources.FamilyV3Extension));
        dialog.Filter.Add(new FilterEntry(Resources.FamilyV2Files, Resources.FamilyV2Extension));
        dialog.Filter.Add(new FilterEntry(Resources.AllFiles, Resources.AllExtension));
        dialog.Title = Resources.Open;
        var result = dialog.ShowOpen();

        if (result == false)
            return;

        if (string.IsNullOrEmpty(dialog.FileName)) // generate a warning
            return;

        var family = App.Family;
        var familyCollection = App.FamilyCollection;

        LoadFamily(dialog.FileName);

        family.OnContentChanged();

        if (familyCollection.FullyQualifiedFilename.EndsWith(Resources.DefaultFamilyExtension))
        {
            AppConfiguration.PushRecentFile(familyCollection.FullyQualifiedFilename);
            //BuildOpenMenu(); // this is handled in the familydataopenedmessage
            family.IsDirty = false;
        }

        Messenger.Send(new FamilyDataOpenedMessage(dialog.FileName, familyCollection.FullyQualifiedFilename));
    }

    private void OpenRecentExec(string fileName)
    {
        var family = App.Family;
        var familyCollection = App.FamilyCollection;

        LoadFamily(fileName);

        family.OnContentChanged();

        if (familyCollection.FullyQualifiedFilename.EndsWith(Resources.DefaultFamilyExtension))
        {
            //BuildOpenMenu();
            AppConfiguration.PushRecentFile(familyCollection.FullyQualifiedFilename);
            family.IsDirty = false;
        }

        Messenger.Send(new FamilyDataOpenedMessage(fileName, familyCollection.FullyQualifiedFilename));
    }
}