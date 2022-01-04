using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.FamilyShow.Services;

public interface IAppConfiguration : INotifyPropertyChanged
{
    ReadOnlyCollection<string> RecentFiles { get; }
    ReadOnlyDictionary<string, string> Skins { get; }
    string ExportPath { get; }
    void LoadRecentFiles();
    void PushRecentFile(string filename);
    void SaveRecentFiles();
    void SaveSkinSetting(string skin);
}
