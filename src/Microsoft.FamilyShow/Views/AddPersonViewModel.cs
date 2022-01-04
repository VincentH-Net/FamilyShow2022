using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.FamilyShow.Framework;
using Microsoft.FamilyShow.Messages;
using Microsoft.FamilyShow.Properties;
using Microsoft.FamilyShow.Services;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Views;

public class AddPersonViewModel : ObservableRecipient
{
    private readonly PeopleCollection family = App.Family;

    private BitmapImage? avatarPhotoImage;
    private string? avatarPhotoPath;
    private string? birthplace;
    private bool birthSexIsFemale;
    private bool birthSexIsMale;
    private string? dateOfBirth;
    private string? firstName;
    private string? surname;

    public AddPersonViewModel(ICommonDialogFactory dialogFactory)
    {
        DialogFactory = dialogFactory;
        Add = new RelayCommand(AddExec);
        Close = new RelayCommand(CloseExec);
        AvatarPhotoMouseDown = new RelayCommand(AvatarPhotoMouseDownExec);
    }

    public ICommand Add { get; }

    public ICommand AvatarPhotoMouseDown { get; }

    public string? AvatarPhotoPath
    {
        get => avatarPhotoPath;
        set => SetProperty(ref avatarPhotoPath, value);
    }

    public BitmapImage? AvatarPhotoSource
    {
        get => avatarPhotoImage;
        set => SetProperty(ref avatarPhotoImage, value);
    }

    public string? Birthplace
    {
        get => birthplace;
        set => SetProperty(ref birthplace, value);
    }

    public bool BirthSexIsFemale
    {
        get => birthSexIsFemale;
        set => SetProperty(ref birthSexIsFemale, value);
    }

    public bool BirthSexIsMale
    {
        get => birthSexIsMale;
        set => SetProperty(ref birthSexIsMale, value);
    }

    public ICommand Close { get; }

    public string? DateOfBirth
    {
        get => dateOfBirth;
        set => SetProperty(ref dateOfBirth, value);
    }

    public string? FirstName
    {
        get => firstName;
        set => SetProperty(ref firstName, value);
    }

    public string? Surname
    {
        get => surname;
        set => SetProperty(ref surname, value);
    }

    private ICommonDialogFactory DialogFactory { get; }

    public void ClearInputFields()
    {
        FirstName = string.Empty;
        Surname = string.Empty;
        DateOfBirth = string.Empty;
        Birthplace = string.Empty;
        BirthSexIsMale = true;
        BirthSexIsFemale = false;
        AvatarPhotoSource = null;
        AvatarPhotoPath = null;
    }

    private void AddExec()
    {
        var newPerson = new Person(FirstName, Surname)
        {
            Gender = BirthSexIsMale ? Gender.Male : Gender.Female,
            BirthPlace = Birthplace,
            IsLiving = true
        };

        var birthdate = DateOfBirth.FriendlyStringToDate();
        if (birthdate != DateTime.MinValue) newPerson.BirthDate = birthdate;

        if (!string.IsNullOrEmpty(AvatarPhotoPath))
        {
            var photo = new Photo(AvatarPhotoPath)
            {
                IsAvatar = true
            };

            newPerson.Photos.Add(photo);
        }

        SendMessage(newPerson);
    }

    private void AvatarPhotoMouseDownExec()
    {
        var dialog = DialogFactory.Create();
        dialog.InitialDirectory = Environment.SpecialFolder.MyPictures.ToString();
        dialog.Filter.Add(new FilterEntry(Resources.JpegFiles, Resources.JpegExtension));
        dialog.Filter.Add(new FilterEntry(Resources.PngFiles, Resources.PngExtension));
        dialog.Title = Resources.Open;
        dialog.ShowOpen();

        if (string.IsNullOrEmpty(dialog.FileName)) return;

        AvatarPhotoPath = dialog.FileName;
        AvatarPhotoSource = new BitmapImage(new Uri(AvatarPhotoPath));
    }

    private void CloseExec()
    {
        SendMessage(new Person());
    }

    private void SendMessage(Person person)
    {
        family.Add(person);
        family.Current = person;

        family.IsDirty = true; // todo why was this false?
        family.OnContentChanged();
        Messenger.Send(new NewUserCompletedMessage());
    }
}