using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.FamilyShow.Views;

public partial class WelcomeView
{
    public WelcomeView()
    {
        InitializeComponent();

        DataContext = App.Current.Services.GetService<WelcomeViewModel>();
    }
}
