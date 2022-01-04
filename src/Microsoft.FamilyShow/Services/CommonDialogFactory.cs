using Microsoft.FamilyShow.Framework;

namespace Microsoft.FamilyShow.Services;

public class CommonDialogFactory : ICommonDialogFactory
{
    public ICommonDialog Create()
    {
        return new CommonDialog();
    }
}