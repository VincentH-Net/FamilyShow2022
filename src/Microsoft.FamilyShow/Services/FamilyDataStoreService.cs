using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.FamilyShow.Properties;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Services;

public class FamilyDataStoreService : ObservableObject, IFamilyDataStoreService
{
    public FamilyDataStoreService(People familyCollection, PeopleCollection family)
    {
        FamilyCollection = familyCollection;
        Family = family;
    }

    public People FamilyCollection { get; }

    public PeopleCollection Family { get; }

    public string FullyQualifiedFilename => FamilyCollection.FullyQualifiedFilename;

    public void LoadFamily(string fileName)
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

        family.OnContentChanged();
    }
}