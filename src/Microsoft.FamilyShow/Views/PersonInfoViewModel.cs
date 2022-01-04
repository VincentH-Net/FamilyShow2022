using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.FamilyShow.Messages;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Views;

public class PersonInfoViewModel : ObservableRecipient
{
    private string? captionText;
    private object? displayPhoto;
    private Person? person;

    private object? selectedPhoto;

    private Visibility storyEditBorderVisibility;

    private Visibility storyViewBorderVisibility;

    private ReadOnlyCollection<string>? tags;

    private Visibility tagsStackPanelVisibility = Visibility.Hidden;

    public PersonInfoViewModel()
    {
        CancelStory = new RelayCommand(CancelStoryExec);
        Close = new RelayCommand(CloseExec);
        EditStory = new RelayCommand(EditStoryExec);
        RemovePhoto = new RelayCommand(RemovePhotoExec);
        SaveStory = new RelayCommand(SaveStoryExec);
        SetPrimary = new RelayCommand(SetPrimaryExec);
    }

    public ICommand CancelStory { get; }

    public string? CaptionText
    {
        get => captionText;
        set => SetProperty(ref captionText, value);
    }

    public ICommand Close { get; }

    public object? DisplayPhoto
    {
        get => displayPhoto;
        set => SetProperty(ref displayPhoto, value);
    }

    public ICommand EditStory { get; }

    public Person? Person
    {
        get => person;
        set => SetProperty(ref person, value);
    }

    public ICommand RemovePhoto { get; }

    public ICommand SaveStory { get; }

    public object? SelectedPhoto
    {
        get => selectedPhoto;
        set
        {
            SetProperty(ref selectedPhoto, value);
            if (selectedPhoto != null)
            {
                // Get the path to the selected photo
                var path = selectedPhoto.ToString();

                // Make sure that the file exists
                var fi = new FileInfo(path);
                if (fi.Exists) SetDisplayPhoto(path);

                //PhotoButtonsDockPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // Clear the display photo
                DisplayPhoto = new BitmapImage();

                // Hide the photos and tags
                //PhotoButtonsDockPanel.Visibility = Visibility.Hidden;
                TagsStackPanelVisibility = Visibility.Hidden;

                // Clear tags and caption
                Tags = null;
                CaptionText = string.Empty;
            }
        }
    }

    public ICommand SetPrimary { get; }

    public Visibility StoryEditBorderVisibility
    {
        get => storyEditBorderVisibility;
        set => SetProperty(ref storyEditBorderVisibility, value);
    }

    public Visibility StoryViewBorderVisibility
    {
        get => storyViewBorderVisibility;
        set => SetProperty(ref storyViewBorderVisibility, value);
    }

    public ReadOnlyCollection<string>? Tags
    {
        get => tags;
        set => SetProperty(ref tags, value);
    }

    public Visibility TagsStackPanelVisibility
    {
        get => tagsStackPanelVisibility;
        set => SetProperty(ref tagsStackPanelVisibility, value);
    }

    public void PhotosDrop(string[] fileNames)
    {
        // Get the files that is supported and add them to the photos for the person
        foreach (var fileName in fileNames)
            if (IsFileSupported(fileName))
            {
                var photo = new Photo(fileName);

                // Make the first photo added the person's avatar
                if (Person.Photos.Count == 0)
                {
                    photo.IsAvatar = true;
                    SelectedPhoto = photo;
                }

                // Associate the photo with the person.
                Person.Photos.Add(photo);

                // Setter for property change notification
                Person.Avatar = string.Empty;
            }
    }

    internal void SetDisplayPhoto(string path)
    {
        DisplayPhoto = new BitmapImage(new Uri(path));

        // Make sure the photo supports meta data before retrieving and displaying it
        if (HasMetaData(path))
        {
            // Extract the photo's metadata
            var metadata = (BitmapMetadata)BitmapFrame.Create(new Uri(path)).Metadata;

            // Display the photo's tags
            //TagsStackPanel.Visibility = Visibility.Visible;
            TagsStackPanelVisibility = Visibility.Visible;
            Tags = metadata.Keywords;

            // Display the photo's comment
            CaptionText = metadata.Title;
        }
        else
        {
            // Clear tags and caption
            TagsStackPanelVisibility = Visibility.Hidden;
            Tags = null;
            CaptionText = string.Empty;
        }
    }

    private void CancelStoryExec()
    {
        StoryEditBorderVisibility = Visibility.Hidden;
        StoryViewBorderVisibility = Visibility.Visible;
    }

    private void CloseExec()
    {
        Messenger.Send(new HidePhotosAndStoriesMessage());
    }

    private void EditStoryExec()
    {
#if false
        LoadStoryText(StoryRichTextBox.Document);

        StoryEditBorder.Visibility = Visibility.Visible;
        StoryViewBorder.Visibility = Visibility.Hidden;

        StoryRichTextBox.Focus();
#endif
    }

    private static bool HasMetaData(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        return string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 ||
               string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0;
    }

    private static bool IsFileSupported(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        return string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 ||
               string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0 ||
               string.Compare(extension, ".png", true, CultureInfo.InvariantCulture) == 0 ||
               string.Compare(extension, ".gif", true, CultureInfo.InvariantCulture) == 0;
    }

    private void RemovePhotoExec()
    {
        if (SelectedPhoto == null) return;

        var photo = (Photo)SelectedPhoto;

        Person!.Photos.Remove(photo);

        // Removed photo is an avatar, set a different avatar photo
        if (photo.IsAvatar && Person!.Photos.Count > 0)
        {
            Person!.Photos[0].IsAvatar = true;
            Person!.Avatar = Person!.Photos[0].FullyQualifiedPath;
        }
        else
            // Setter for property change notification
        {
            Person!.Avatar = "";
        }
    }

    private void SaveStoryExec()
    {
    }

    private void SetPrimaryExec()
    {
        if (Person!.Photos != null && SelectedPhoto != null)
        {
            // Set IsAvatar to false for existing photos
            foreach (var existingPhoto in Person.Photos) existingPhoto.IsAvatar = false;

            var photo = (Photo)SelectedPhoto;
            photo.IsAvatar = true;
            Person.Avatar = photo.FullyQualifiedPath;
        }
    }
}