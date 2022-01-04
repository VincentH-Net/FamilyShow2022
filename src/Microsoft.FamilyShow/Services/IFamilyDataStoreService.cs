using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Services;

public interface IFamilyDataStoreService
{
    People FamilyCollection { get; }
    PeopleCollection Family { get; }
    string FullyQualifiedFilename { get; }
    void LoadFamily(string fileName);
}