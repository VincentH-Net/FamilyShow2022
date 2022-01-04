using System;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Xps.Packaging;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FamilyShow.Framework;
using Microsoft.FamilyShow.Messages;
using Microsoft.FamilyShow.Services;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow;

public partial class MainWindow : Window, IUserInterfaceConnector
{
    private readonly PeopleCollection family = App.Family;

    public MainWindow()
    {
        UserInterfaceConnector = this; // TODO get rid of this

        ViewModel = App.Current.Services.GetService<MainWindowViewModel>()!;
        ViewModel.Configure();

        InitializeComponent();

        BuildOpenMenu();

        ShowWelcomeScreen();

        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<FamilyDataOpenedMessage>(this, (r, m) =>
        {
            BuildOpenMenu();
            ShowDetailsPane();
        });

        messenger.Register<FamilyDataImportedMessage>(this, (r, m) => { ShowDetailsPane(); });

        messenger.Register<FamilyDataCreateMessage>(this, (r, m) =>
        {
            ShowNewUserControl();
            family.IsDirty = false;
        });

        messenger.Register<NewUserCompletedMessage>(this, (r, m) =>
        {
            HideNewUserControl();
            ShowDetailsPane();
        });

        messenger.Register<ShowPhotosAndStoriesMessage>(this, (r, m) =>
        {
            ((Storyboard)Resources["ShowPersonInfo"]).Begin(this);

            // PersonInfoControl.DataContext = family.Current;
            PersonInfoControl.ViewModel.Person = family.Current;
        });

        messenger.Register<HidePhotosAndStoriesMessage>(this, (r, m) => { ((Storyboard)Resources["HidePersonInfo"]).Begin(this); });

        Closing += (sender, args) => { ViewModel.ConfigurationService.SaveRecentFiles(); };

        ViewModel.ConfigurationService.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(AppConfiguration.RecentFiles)) BuildOpenMenu();
        };

        family.ContentChanged += (sender, args) => { messenger.Send(new UpdateStatusMessage("Family data changed " + family.Count)); };

        messenger.Register<UpdateStatusMessage>(this, (r, args) => { ((MainWindow)r).ViewModel.StatusMessage = args.Value; });
    }

    public static IUserInterfaceConnector? UserInterfaceConnector { get; set; }

    public MainWindowViewModel ViewModel
    {
        get => (MainWindowViewModel)DataContext;
        set => DataContext = value;
    }

    public void ShowFamilyDataControl()
    {
        FamilyDataControl.Refresh();

        ((Storyboard)Resources["ShowFamilyData"]).Begin(this);
    }

    public void HideFamilyDataControl()
    {
        // Uses an animation to hide the Family Data Control
        if (FamilyDataControl.IsVisible) ((Storyboard)Resources["HideFamilyData"]).Begin(this);
    }

    public void ShowOldVersion()
    {
        OldVersionMessageControl.Visibility = Visibility.Visible;
    }

    public void HideOldVersion()
    {
        OldVersionMessageControl.Visibility = Visibility.Hidden;
    }

    public void ChangeSkin(string skin)
    {
        var skins = ViewModel.ConfigurationService.Skins;
        var skinPath = skins[skin];

        var rd = new ResourceDictionary();
        rd.MergedDictionaries.Add(Application.LoadComponent(new Uri(skinPath, UriKind.Relative)) as ResourceDictionary);
        Application.Current.Resources = rd;

        ViewModel.ConfigurationService.SaveSkinSetting(skinPath);

        family.OnContentChanged();
        // PersonInfoControl.OnSkinChanged();
        WeakReferenceMessenger.Default.Send(new SkinChangedMessage());
    }

    public void ShowDetailsPane()
    {
        // Add the cloned column to layer 0:
        if (!DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
            DiagramPane.ColumnDefinitions.Add(column1CloneForLayer0);

        if (family.Current != null) DetailsControl.DataContext = family.Current;

        DetailsPane.Visibility = Visibility.Visible;
        DetailsControl.SetDefaultFocus();

        HideNewUserControl();
        HideWelcomeScreen();

        MainMenu.IsEnabled = true;
    }

    public void ShowNewUserControl()
    {
        HideFamilyDataControl();
        HideDetailsPane();
        DiagramControl.Visibility = Visibility.Collapsed;
        WelcomeUserControl.Visibility = Visibility.Collapsed;

        if (PersonInfoControl.Visibility == Visibility.Visible) ((Storyboard)Resources["HidePersonInfo"]).Begin(this);

        NewUserControl.Visibility = Visibility.Visible;
        NewUserControl.ClearInputFields();
        NewUserControl.SetDefaultFocus();

        // Delete to clear existing files and re-Create the necessary directories
        var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            App.ApplicationFolderName);
        tempFolder = Path.Combine(tempFolder, FamilyShowLib.App.AppDataFolderName);
        People.RecreateDirectory(tempFolder);

        var photoFolder = Path.Combine(tempFolder, Photo.Const.PhotosFolderName);
        People.RecreateDirectory(photoFolder);

        var storyFolder = Path.Combine(tempFolder, Story.Const.StoriesFolderName);
        People.RecreateDirectory(storyFolder);
    }

    public void ShowWelcomeScreen()
    {
        HideDetailsPane();
        HideNewUserControl();
        DiagramControl.Visibility = Visibility.Hidden;

        WelcomeUserControl.Visibility = Visibility.Visible;
    }

    public void PromptToSave()
    {
        if (!family.IsDirty) return;

        var result = MessageBox.Show(Properties.Resources.NotSavedMessage, Properties.Resources.NotSaved, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var dialog = ViewModel.DialogFactory.Create();
        dialog.InitialDirectory = People.ApplicationFolderPath;

        dialog.Filter.Add(new FilterEntry(Properties.Resources.FamilyFiles, Properties.Resources.FamilyV3Extension));
        dialog.Filter.Add(new FilterEntry(Properties.Resources.AllFiles, Properties.Resources.AllExtension));
        dialog.Title = Properties.Resources.SaveAs;
        dialog.DefaultExtension = Properties.Resources.DefaultFamilyExtension;
        dialog.ShowSave();

        var familyCollection = App.FamilyCollection;

        if (!string.IsNullOrEmpty(dialog.FileName))
        {
            familyCollection.Save(dialog.FileName);

            ViewModel.ConfigurationService.PushRecentFile(familyCollection.FullyQualifiedFilename);

            BuildOpenMenu();
        }
    }

    public void ExportDiagram(string fileName)
    {
        var package = Package.Open(fileName, FileMode.Create);
        var xpsDoc = new XpsDocument(package);
        var xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);

        // Hide the zoom control before the diagram is saved
        DiagramControl.ZoomSliderPanel.Visibility = Visibility.Hidden;

        // Since DiagramBorder derives from FrameworkElement, the XpsDocument writer knows
        // how to output it's contents. The border is used instead of the DiagramControl
        // so that the diagram background is output as well as the digram control itself.
        xpsWriter.Write(DiagramBorder);
        xpsDoc.Close();
        package.Close();

        // Show the zoom control again
        DiagramControl.ZoomSliderPanel.Visibility = Visibility.Visible;
    }

    public void BuildOpenMenu()
    {
        OpenMenu.Items.Clear();

        // MenuItem for opening files
        var openMenuItem = new MenuItem
        {
            Header = "Open...",
            Command = ViewModel.Open,
            InputGestureText = "Ctrl+O"
        };

        OpenMenu.Items.Add(openMenuItem);

        // Add the recent files to the menu as menu items
        if (ViewModel.ConfigurationService.RecentFiles.Count > 0)
        {
            // Separator between the open menu and the recent files
            OpenMenu.Items.Add(new Separator());

            foreach (var file in ViewModel.ConfigurationService.RecentFiles)
            {
                var item = new MenuItem
                {
                    Header = Path.GetFileName(file),
                    CommandParameter = file,
                    Command = ViewModel.Open
                };

                // todo item.Click += new RoutedEventHandler(OpenRecentFile_Click);
                OpenMenu.Items.Add(item);
            }
        }

        var closeMenuItem = new MenuItem
        {
            Header = "Close",
            Command = ViewModel.Close
        };

        OpenMenu.Items.Add(closeMenuItem);
    }

    private void DetailsControl_PersonInfoClick(object sender, RoutedEventArgs e)
    {
        // Uses an animation to show the Person Info Control
        ((Storyboard)Resources["ShowPersonInfo"]).Begin(this);

        PersonInfoControl.DataContext = family.Current;
    }

    private void HideDetailsPane()
    {
        DetailsPane.Visibility = Visibility.Collapsed;

        // Remove the cloned columns from layers 0
        if (DiagramPane.ColumnDefinitions.Contains(column1CloneForLayer0))
            DiagramPane.ColumnDefinitions.Remove(column1CloneForLayer0);

        DiagramControl.Visibility = Visibility.Collapsed;

        MainMenu.IsEnabled = false;
        // ShowWelcomeScreen();
    }

    private void HideFamilyData_StoryboardCompleted(object sender, EventArgs e)
    {
        DetailsControl.SetDefaultFocus();
    }

    private void HideNewUserControl()
    {
        NewUserControl.Visibility = Visibility.Hidden;
        DiagramControl.Visibility = Visibility.Visible;

        if (family.Current != null) DetailsControl.DataContext = family.Current;
    }

    private void HidePersonInfo_StoryboardCompleted(object sender, EventArgs e)
    {
        DetailsControl.SetDefaultFocus();
    }

    private void HideWelcomeScreen()
    {
        WelcomeUserControl.Visibility = Visibility.Hidden;
    }

    private void PersonInfoControl_CloseButtonClick(object sender, RoutedEventArgs e)
    {
        // Uses an animation to hide the Person Info Control
        ((Storyboard)Resources["HidePersonInfo"]).Begin(this);
    }

    private void ShowFamilyData_StoryboardCompleted(object sender, EventArgs e)
    {
        FamilyDataControl.SetDefaultFocus();
    }

    private void ShowPersonInfo_StoryboardCompleted(object sender, EventArgs e)
    {
        PersonInfoControl.SetDefaultFocus();
    }
}