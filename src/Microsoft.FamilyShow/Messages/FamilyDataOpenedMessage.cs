namespace Microsoft.FamilyShow.Messages;

public class FamilyDataOpenedMessage
{
    public FamilyDataOpenedMessage(string fileName, string fullyQualifiedFilename)
    {
        FileName = fileName;
        FullyQualifiedFilename = fullyQualifiedFilename;
    }

    public string FileName { get; set; }
    public string FullyQualifiedFilename { get; set; }
}