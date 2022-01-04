using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.FamilyShow.Views;

public partial class MyRichTextBoxControl : UserControl
{
    public MyRichTextBoxControl()
    {
        InitializeComponent();

        foreach (var fontFamily in Fonts.SystemFontFamilies) FontsComboBox.Items.Add(fontFamily.Source);
    }

    public new bool Focus()
    {
        return StoryRichTextBox.Focus();
    }

    public FlowDocument GetDocument()
    {
        return StoryRichTextBox.Document;
    }

    private void FontsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        StoryRichTextBox.Selection.ApplyPropertyValue(FontFamilyProperty, FontsComboBox.SelectedValue);
    }

    private void StoryRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        // Update the toolbar controls based on the current selected text.
        UpdateButtons();
    }

    private void StoryRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateButtons();
    }

    private void UpdateBulletButtons()
    {
        // The bullet information takes a little more work, need
        // to walk the tree and look for a ListItem element.
        var element = StoryRichTextBox.Selection.Start.Parent as TextElement;
        while (element != null)
        {
            if (element is ListItem listItem)
            {
                // Found a bullet item, determine the type of bullet.
                BulletsButton.IsChecked = listItem.List != null && listItem.List.MarkerStyle != TextMarkerStyle.Decimal;
                NumberingButton.IsChecked = listItem.List is { MarkerStyle: TextMarkerStyle.Decimal };
                return;
            }

            element = element.Parent as TextElement;
        }

        // Did not find a bullet item.
        BulletsButton.IsChecked = false;
        NumberingButton.IsChecked = false;
    }

    private void UpdateButtons()
    {
        // Bold button.
        var result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontWeightProperty);
        BoldButton.IsChecked = result is FontWeight weight && weight == FontWeights.Bold;

        // Italic button.
        result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontStyleProperty);
        ItalicButton.IsChecked = result is FontStyle && (FontStyle)result == FontStyles.Italic;

        // Font list.
        result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontFamilyProperty);
        if (result is FontFamily) FontsComboBox.SelectedItem = result.ToString();

        // Align buttons.
        result = StoryRichTextBox.Selection.GetPropertyValue(Block.TextAlignmentProperty);
        AlignLeftButton.IsChecked = result is TextAlignment.Left;
        AlignCenterButton.IsChecked = result is TextAlignment.Center;

        AlignRightButton.IsChecked = result is TextAlignment.Right;
        AlignFullButton.IsChecked = result is TextAlignment.Justify;

        // Underline button.
        result = StoryRichTextBox.Selection.GetPropertyValue(Paragraph.TextDecorationsProperty);
        if (result is TextDecorationCollection decorations)
            UnderlineButton.IsChecked = decorations.Count > 0 && decorations[0].Location == TextDecorationLocation.Underline;
        else
            UnderlineButton.IsChecked = false;

        UpdateBulletButtons();
    }
}