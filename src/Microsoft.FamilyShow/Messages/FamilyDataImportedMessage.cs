namespace Microsoft.FamilyShow.Messages;

public class FamilyDataImportedMessage
{
    public FamilyDataImportedMessage(string fileName, string fullyQualifiedFilename)
    {
        FileName = fileName;
        FullyQualifiedFilename = fullyQualifiedFilename;
    }

    public string FileName { get; set; }
    public string FullyQualifiedFilename { get; set; }
}