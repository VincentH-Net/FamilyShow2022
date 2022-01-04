using System.Windows.Input;
using Microsoft.FamilyShow.Framework;
using Microsoft.FamilyShow.Properties;
using Microsoft.FamilyShow.Services;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow;

public partial class MainWindowViewModel
{
    public ICommand ChangeSkin { get; }

    public ICommand Close { get; }

    public ICommand Create { get; }

    public ICommand ExportBirth { get; }

    public ICommand ExportGedcom { get; }

    public ICommand ImportGedcom { get; }

    public ICommand Open { get; }

    public ICommand Save { get; }

    public ICommand SaveAs { get; }

    public ICommand SaveXps { get; }

    public ICommand VersionMessageClosed { get; }

    public ICommand VisitVertigoWebSite { get; }

    public ICommand WhatIsGedcom { get; }

    private void ChangeSkinExec(string? obj)
    {
        Connector.ChangeSkin(obj!);
    }

    private void CloseExec()
    {
        var family = App.Family;

        family.Clear();

        Connector.ShowWelcomeScreen();
    }

    private void CreateExec()
    {
        Connector.PromptToSave();

        var family = App.Family;
        var familyCollection = App.FamilyCollection;

        family.Clear();
        familyCollection.FullyQualifiedFilename = null;
        family.OnContentChanged();

        Connector.ShowNewUserControl();
        family.IsDirty = false;
    }

    private void ExportBirthExec()
    {
        var family = App.Family;

        BirthDataService.ExportBirth(family, ConfigurationService.ExportPath);
    }

    private void ExportGedcomExec()
    {
        var family = App.Family;

        var dialog = DialogFactory.Create();
        dialog.InitialDirectory = People.ApplicationFolderPath;
        dialog.Filter.Clear();
        dialog.Filter.Add(new FilterEntry(Resources.GedcomFiles, Resources.GedcomExtension));
        dialog.Filter.Add(new FilterEntry(Resources.AllFiles, Resources.AllExtension));
        dialog.Title = Resources.Export;
        dialog.DefaultExtension = Resources.DefaultGedcomExtension;
        dialog.ShowSave();

        if (!string.IsNullOrEmpty(dialog.FileName))
        {
            var ged = new GedcomExport();
            ged.Export(family, dialog.FileName);
        }
    }

    private void ImportGedcomExec()
    {
        Connector.PromptToSave();

        var familyCollection = App.FamilyCollection;
        var family = App.Family;

        var dialog = DialogFactory.Create();
        dialog.InitialDirectory = People.ApplicationFolderPath;
        dialog.Filter.Clear();
        dialog.Filter.Add(new FilterEntry(Resources.GedcomFiles, Resources.GedcomExtension));
        dialog.Filter.Add(new FilterEntry(Resources.AllFiles, Resources.AllExtension));
        dialog.Title = Resources.Import;
        dialog.ShowOpen();

        if (!string.IsNullOrEmpty(dialog.FileName))
            try
            {
                var ged = new GedcomImport();
                ged.Import(family, dialog.FileName);
                familyCollection.FullyQualifiedFilename = string.Empty;

                // todo ShowDetailsPane();
                family.IsDirty = false;
            }
            catch
            {
                // Could not import the GEDCOM for some reason. Handle
                // all exceptions the same, display message and continue
                /// without importing the GEDCOM file.
                // todo MessageBox.Show(this, Properties.Resources.GedcomFailedMessage, Properties.Resources.GedcomFailed, MessageBoxButton.OK, MessageBoxImage.Information);
            }
    }

    private static void LoadFamily(string fileName)
    {
        var familyCollection = App.FamilyCollection;
        var family = App.Family;

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

    private void OpenExec(string? obj)
    {
        Connector.PromptToSave();

        if (obj != null)
        {
            LoadFamily(obj);
            return;
        }

        var familyCollection = App.FamilyCollection;
        var family = App.Family;

        var dialog = DialogFactory.Create();
        dialog.InitialDirectory = People.ApplicationFolderPath;
        dialog.Filter.Clear();
        dialog.Filter.Add(new FilterEntry(Resources.FamilyFiles, Resources.FamilyExtensions));
        dialog.Filter.Add(new FilterEntry(Resources.FamilyV3Files, Resources.FamilyV3Extension));
        dialog.Filter.Add(new FilterEntry(Resources.FamilyV2Files, Resources.FamilyV2Extension));
        dialog.Filter.Add(new FilterEntry(Resources.AllFiles, Resources.AllExtension));
        dialog.Title = Resources.Open;
        dialog.ShowOpen();

        if (!string.IsNullOrEmpty(dialog.FileName))
        {
            LoadFamily(dialog.FileName);

            Connector.ShowDetailsPane();

            // This will tell other views using this data to update themselves using the new data
            family.OnContentChanged();

            if (familyCollection.FullyQualifiedFilename.EndsWith(Resources.DefaultFamilyExtension))
            {
                ConfigurationService.PushRecentFile(familyCollection.FullyQualifiedFilename);
                Connector.BuildOpenMenu();
                family.IsDirty = false;
            }
        }
    }

    private void SaveAsExec()
    {
        Connector.HideOldVersion();

        var family = App.Family;
        if (family.IsOldVersion && !appSettings.DontShowOldVersionMessage)
            // OldVersionMessageControl.Visibility = Visibility.Visible;
            // ShowOldVersionMessage();
            Connector.ShowOldVersion();
        else
            SaveFamilyData();
    }

    private void SaveExec()
    {
        var familyCollection = App.FamilyCollection;

        familyCollection.Save();

        ConfigurationService.PushRecentFile(familyCollection.FullyQualifiedFilename);
        Connector.BuildOpenMenu();

#if false
if (family.IsOldVersion && !appSettings.DontShowOldVersionMessage)
      {
        //Show message that the file will be saved in the new format
        ShowOldVersionMessage();
      }
      else
      {
        // Prompt to save if the file has not been saved before, otherwise just save to the existing file.
        if (string.IsNullOrEmpty(familyCollection.FullyQualifiedFilename) || family.IsOldVersion)
        {
          CommonDialog dialog = new CommonDialog
          {
            InitialDirectory = People.ApplicationFolderPath
          };
dialog.Filter.Clear();

          dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyV3Files, Properties.Resources.FamilyV3Extension));
          dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
          dialog.Title = Properties.Resources.SaveAs;
          dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
          dialog.ShowSave();

          if (!string.IsNullOrEmpty(dialog.FileName))
          {
            familyCollection.Save(dialog.FileName);
            family.IsOldVersion = false;

            // Remove the file from its current position and add it back to the top/most recent position.
            App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
            App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
            BuildOpenMenu();
          }
        }
        else
        {
          familyCollection.Save();

          // Remove the file from its current position and add it back to the top/most recent position.
          App.RecentFiles.Remove(familyCollection.FullyQualifiedFilename);
          App.RecentFiles.Insert(0, familyCollection.FullyQualifiedFilename);
          BuildOpenMenu();
        }
      }

#endif
    }

    private void SaveFamilyData()
    {
        var familyCollection = App.FamilyCollection;
        var family = App.Family;

        var dialog = DialogFactory.Create();
        dialog.InitialDirectory = People.ApplicationFolderPath;

        dialog.Filter.Clear();
        dialog.Filter.Add(new FilterEntry(Resources.FamilyFiles, Resources.FamilyV3Extension));
        dialog.Filter.Add(new FilterEntry(Resources.AllFiles, Resources.AllExtension));
        dialog.Title = Resources.SaveAs;
        dialog.DefaultExtension = Resources.DefaultFamilyExtension;
        var result = dialog.ShowSave();

        if (!result || string.IsNullOrEmpty(dialog.FileName)) return;

        familyCollection.Save(dialog.FileName);
        family.IsOldVersion = false;

        if (familyCollection.FullyQualifiedFilename.EndsWith(Resources.DefaultFamilyExtension))
        {
            ConfigurationService.PushRecentFile(dialog.FileName);
            Connector.BuildOpenMenu();
        }
    }

    private void SaveXpsExec()
    {
        var dialog = DialogFactory.Create();
        dialog.InitialDirectory = People.ApplicationFolderPath;

        dialog.Filter.Add(new FilterEntry(Resources.XpsFiles, Resources.XpsExtension));
        dialog.Filter.Add(new FilterEntry(Resources.AllFiles, Resources.AllExtension));
        dialog.Title = Resources.Export;
        dialog.DefaultExtension = Resources.DefaultXpsExtension;
        var result = dialog.ShowSave();

        if (!result || string.IsNullOrEmpty(dialog.FileName)) return;

        Connector.ExportDiagram(dialog.FileName);
    }

    private void VersionMessageClosedExec()
    {
        Connector.HideOldVersion();
        SaveFamilyData();
    }

    private static void VisitVerigoWebSiteGo()
    {
        //System.Diagnostics.Process.Start("https://www.linkedin.com/company/vertigo-software");
        //System.Diagnostics.Process.Start("http://www.vertigo.com/familyshow");

        var target = "http://www.vertigo.com/";

        OpenUrl(target);
    }

    private static void WhatIsGedcomExec()
    {
        var target = "http://en.wikipedia.org/wiki/GEDCOM";

        OpenUrl(target);
    }
}