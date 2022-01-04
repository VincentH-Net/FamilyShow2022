using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.FamilyShow.Properties;

namespace Microsoft.FamilyShow.Services;

public class AppConfiguration : ObservableObject, IAppConfiguration
{
    private const int NumberOfRecentFilesToTrack = 10;

    private static readonly string RecentFilesFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.Combine(App.ApplicationFolderName, "RecentFiles.xml"));

    private ReadOnlyCollection<string> recentFiles;

    private ReadOnlyDictionary<string, string>? skins;

    public AppConfiguration()
    {
        recentFiles = new ReadOnlyCollection<string>(new List<string>());
    }

    public string ExportPath { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


    public ReadOnlyCollection<string> RecentFiles
    {
        get => recentFiles;
        set => SetProperty(ref recentFiles, value);
    }

    public void LoadRecentFiles()
    {
        var deserializdRecent = new List<string>();
        if (File.Exists(RecentFilesFilePath))
        {
            var ser = new XmlSerializer(typeof(List<string>));
            using (TextReader reader = new StreamReader(RecentFilesFilePath))
            {
                deserializdRecent = (List<string>)ser.Deserialize(reader)!;
            }

            if (deserializdRecent.Count > 0) deserializdRecent = deserializdRecent.Where(File.Exists).Take(NumberOfRecentFilesToTrack).ToList();
        }

        RecentFiles = new ReadOnlyCollection<string>(deserializdRecent);
    }

    public void SaveRecentFiles()
    {
        var ser = new XmlSerializer(typeof(List<string>));
        using TextWriter writer = new StreamWriter(RecentFilesFilePath);
        ser.Serialize(writer, RecentFiles.ToList());
    }

    public ReadOnlyDictionary<string, string> Skins
    {
        get
        {
            if (this.skins != null) return this.skins;
            var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var skins = new Dictionary<string, string>();

            var relDir = Path.Combine(currentDirectory, Resources.Skins);

            foreach (var folder in Directory.GetDirectories(Resources.Skins))
            foreach (var file in Directory.GetFiles(folder))
            {
                var fileInfo = new FileInfo(file);
                if (string.Compare(fileInfo.Extension, Resources.XamlExtension, true, CultureInfo.InvariantCulture) == 0)
                    // Use the first part of the resource file name for the menu item name.
                    skins.Add(fileInfo.Name.Remove(fileInfo.Name.IndexOf(Resources.ResourcesString)), Path.Combine(folder, fileInfo.Name));
            }

            return this.skins = new ReadOnlyDictionary<string, string>(skins);
        }
    }

    public void SaveSkinSetting(string skin)
    {
        var appSettings = Settings.Default;

        appSettings.Skin = skin;
        appSettings.Save();
    }

    public void PushRecentFile(string fullyQualifiedFilename)
    {
        var files = recentFiles.ToList();

        if (recentFiles.Contains(fullyQualifiedFilename)) files.Remove(fullyQualifiedFilename);
        files.Insert(0, fullyQualifiedFilename);

        RecentFiles = new ReadOnlyCollection<string>(files);
    }
}