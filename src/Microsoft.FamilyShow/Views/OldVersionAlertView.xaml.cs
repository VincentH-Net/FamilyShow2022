using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FamilyShow.Properties;

namespace Microsoft.FamilyShow.Views;

public partial class OldVersionAlertView
{
    public static readonly DependencyProperty MessageClosedProperty = DependencyProperty.Register("MessageClosed", typeof(ICommand), typeof(OldVersionAlertView));

    public OldVersionAlertView()
    {
        InitializeComponent();

        DataContext = App.Current.Services.GetService<OldVersionMessageViewModel>();
        ViewModel.DontShowAgain = Settings.Default.DontShowOldVersionMessage;
        ViewModel.GoContinue = delegate { MessageClosed.Execute(this); };
    }

    public ICommand MessageClosed
    {
        get => (ICommand)GetValue(MessageClosedProperty);
        set => SetValue(MessageClosedProperty, value);
    }

    public OldVersionMessageViewModel ViewModel
    {
        get => (OldVersionMessageViewModel)DataContext;
        set => DataContext = value;
    }
}