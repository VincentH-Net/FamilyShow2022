using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FamilyShow.Framework;
using Microsoft.FamilyShow.Properties;
using Microsoft.FamilyShow.Services;
using Microsoft.FamilyShow.Views;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow;

public partial class App : Application
{
    // The name of the application folder.  This folder is used to save the files 
    // for this application such as the photos, stories and family data.
    internal const string ApplicationFolderName = "Family.Show";

    // The main list of family members that is shared for the entire application.
    // The FamilyCollection and Family fields are accessed from the same thread,
    // so suppressing the CA2211 code analysis warning.
    public static People FamilyCollection = new();
    public static PeopleCollection Family = FamilyCollection.PeopleCollection;

    public App()
    {
        Services = ConfigureServices();
    }

    public new static App Current => (App)Application.Current;

    public IServiceProvider Services { get; }

    /// <summary>
    ///     Return the animation duration. The duration is extended
    ///     if special keys are currently pressed (for demo purposes)
    ///     otherwise the specified duration is returned.
    /// </summary>
    public static TimeSpan GetAnimationDuration(double milliseconds)
    {
        return TimeSpan.FromMilliseconds(Keyboard.IsKeyDown(Key.F12) ? milliseconds * 5 : milliseconds);
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SampleFileGenerator.InstallSampleFiles(ApplicationFolderName);

        var appSettings = Settings.Default;

        if (!string.IsNullOrEmpty(appSettings.Skin))
            try
            {
                var resourceDictionary = new ResourceDictionary();
                resourceDictionary.MergedDictionaries.Add(LoadComponent(new Uri(appSettings.Skin, UriKind.Relative)) as ResourceDictionary);
                Current.Resources = resourceDictionary;
            }
            catch
            {
            }
        
        var mainWindow = new MainWindow();

        var userInterfaceConnector = Services.GetService<IUserInterfaceConnector>();
        ((userInterfaceConnector as UserInterfaceConnector)!).RegisterMainWindow(mainWindow);

        mainWindow.Show();
    }

    private static IServiceProvider ConfigureServices()
    {
        // https://docs.microsoft.com/en-us/windows/communitytoolkit/mvvm/ioc
        var services = new ServiceCollection();

        services.AddLogging(configure => configure.AddConsole());

        var dataStore = new FamilyDataStoreService(FamilyCollection, Family);
        services.AddSingleton<IFamilyDataStoreService>(dataStore);
        services.AddSingleton<IAppConfiguration, AppConfiguration>();
        services.AddTransient<ICommonDialog, CommonDialog>();
        services.AddSingleton<ICommonDialogFactory, CommonDialogFactory>();
        services.AddSingleton<IUserInterfaceConnector, UserInterfaceConnector>();

        services.AddTransient<AddPersonViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<OldVersionMessageViewModel>();
        services.AddTransient<PersonInfoViewModel>();
        services.AddTransient<WelcomeViewModel>();

        return services.BuildServiceProvider();
    }
}