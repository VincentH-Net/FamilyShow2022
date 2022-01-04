namespace Microsoft.FamilyShow.Framework;

public class StringPair
{
    public StringPair(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; set; }
    public string Value { get; set; }
}