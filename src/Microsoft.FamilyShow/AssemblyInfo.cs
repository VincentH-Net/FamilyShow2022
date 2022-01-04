using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://familyshow.com/fs", "Microsoft.FamilyShow")]
[assembly: XmlnsDefinition("http://familyshow.com/fs", "Microsoft.FamilyShow.Controls")]
[assembly: XmlnsDefinition("http://familyshow.com/fs", "Microsoft.FamilyShow.Controls.Diagram")]
[assembly: XmlnsDefinition("http://familyshow.com/fs", "Microsoft.FamilyShow.Controls.FamilyData")]
[assembly: XmlnsDefinition("http://familyshow.com/fs", "Microsoft.FamilyShow.Framework")]
[assembly: XmlnsDefinition("http://familyshow.com/fs", "Microsoft.FamilyShow.Views")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page,
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page,
    // app, or any theme specific resource dictionaries)
)]