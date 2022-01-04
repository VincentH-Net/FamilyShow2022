using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FamilyShow.Messages;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow.Views;

public partial class PersonInfoView
{
    public PersonInfoView()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetService<PersonInfoViewModel>()!;

        WeakReferenceMessenger.Default.Register<SkinChangedMessage>(this, (r, m) =>
        {
            var tempQualifier = (PersonInfoView)r;
            var textRange = new TextRange(tempQualifier.StoryViewer.Document.ContentStart, tempQualifier.StoryViewer.Document.ContentEnd);
            textRange.Select(tempQualifier.StoryViewer.Document.ContentStart, tempQualifier.StoryViewer.Document.ContentEnd);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, tempQualifier.FindResource("FlowDocumentFontColor"));
        });
    }

    public PersonInfoViewModel ViewModel
    {
        get => (PersonInfoViewModel)DataContext;
        set => DataContext = value;
    }

    public void SetDefaultFocus()
    {
        CloseButton.Focus();
    }

    private void EditStoryButton_Click(object sender, RoutedEventArgs e)
    {
        LoadStoryText(ViewModel.Person!, RichTextBoxControl.GetDocument());

        ViewModel.StoryEditBorderVisibility = Visibility.Visible;
        ViewModel.StoryViewBorderVisibility = Visibility.Hidden;

        RichTextBoxControl.Focus();
    }

    private void LoadStoryText(Person person, FlowDocument flowDocument)
    {
        // Ignore null cases
        if (flowDocument == null || flowDocument.Blocks == null || DataContext == null) return;

        // Clear out any existing text in the viewer 
        flowDocument.Blocks.Clear();

        // Load the story into the story viewer
        if (person is { Story: { } })
        {
            var textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
            person.Story.Load(textRange);
        }
        else
        {
            // This person doesn't have a story.
            // Load the default story text
            var textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd)
            {
                Text = Properties.Resources.DefaultStory
            };

            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, Properties.Resources.StoryFontFamily);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, Properties.Resources.StoryFontSize);
        }
    }

    private void PhotosListBox_Drop(object sender, DragEventArgs e)
    {
        // Retrieve the dropped files
        var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

        ViewModel.PhotosDrop(fileNames);

        // Mark the event as handled, so the control's native Drop handler is not called.
        e.Handled = true;
    }

    private void SaveStoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Person != null)
        {
            // Pass in a TextRange object to save the story
            var doc = RichTextBoxControl.GetDocument();
            var textRange = new TextRange(doc.ContentStart, doc.ContentEnd);
            ViewModel.Person.Story = new Story();
            var storyFileName = new StringBuilder(ViewModel.Person.Name).Append(".rtf").ToString();
            ViewModel.Person.Story.Save(textRange, storyFileName);

            // Display the rich text in the viewer
            LoadStoryText(ViewModel.Person, StoryViewer.Document);

            // Display all text in constrast color to the StoryViewer background.
            var textRange2 = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
            textRange2.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
            textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));
        }

        // Switch to view mode
        ViewModel.StoryEditBorderVisibility = Visibility.Hidden;
        ViewModel.StoryViewBorderVisibility = Visibility.Visible;

        // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
        StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
        StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Page;
    }

    private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility != Visibility.Visible) return;

        // Show the story, hide the editor
        ViewModel.StoryViewBorderVisibility = Visibility.Visible;
        ViewModel.StoryEditBorderVisibility = Visibility.Hidden;

        // Load the person story into the viewer
        LoadStoryText(ViewModel.Person, StoryViewer.Document);

        // Display all text in constrast color to the StoryViewer background.
        var textRange2 = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
        textRange2.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
        textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));

        // Hide the photo tags and photo edit buttons if there is no main photo.
        if (DisplayPhoto.Source == null)
        {
            TagsStackPanel.Visibility = Visibility.Hidden;
            PhotoButtonsDockPanel.Visibility = Visibility.Hidden;
        }

        // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
        StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
        StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Page;
    }
}