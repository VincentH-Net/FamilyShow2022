using System.Collections.Generic;
using Microsoft.FamilyShow.Framework;

namespace Microsoft.FamilyShow.Services;

public interface ICommonDialog
{
    List<FilterEntry> Filter { get; }
    string Title { set; }
    string InitialDirectory { set; }
    string DefaultExtension { set; }
    string FileName { get; }

    bool ShowOpen();

    bool ShowSave();
}