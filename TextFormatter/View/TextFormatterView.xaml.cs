using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TextFormatter.ViewModel;

namespace TextFormatter.View
{
    /// <summary>
    /// Interaction logic for TextFormatterView.xaml
    /// </summary>
    public partial class TextFormatterView : UserControl
    {
        /// <summary>
        /// This prevents the code that handles text change events from creating
        /// an infinite recursion when the function itself modifies the text.
        /// </summary>
        private bool suppressTextChangeHandling = false;

        public TextFormatterView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for when the text in the main text box changes. Applies the algorithm to colorize the text.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Event data</param>
        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MainTextBox.Document != null && !suppressTextChangeHandling)
            {
                // Since this function will cause the text to change (hence firing off another TextChangedEvent),
                // we need to suppress the handling of these events while this code is running in order to avoid
                // an infinite recursion.
                suppressTextChangeHandling = true;

                try
                {

                    if (DataContext is TextFormatterViewModel)
                    {
                        // Compute the index of the paragraph containing the caret and the plaintext offset of the caret.
                        TextPointer caretPos = MainTextBox.CaretPosition;
                        Paragraph caretParagraph = caretPos.Paragraph;
                        int plainTextCaretOffset = ComputePlaintextOffset(caretPos);
                        int caretParagraphIndex = -1;
                        BlockCollection blocks = MainTextBox.Document.Blocks;
                        for (int i = 0; i < blocks.Count; ++i)
                            if (blocks.ElementAt<Block>(i) == caretParagraph)
                                caretParagraphIndex = i;

                        // Pull the plaintext content from the RichTextBox
                        var textContent = new List<String>();
                        foreach (Block b in blocks)
                        {
                            if (b is Paragraph)
                            {
                                Paragraph p = (Paragraph)b;
                                // Extract the plain text corresponding to this paragraph
                                TextRange tr = new TextRange(
                                            p.ContentStart,
                                            p.ContentEnd);
                                textContent.Add(tr.Text);
                            }
                            else
                                textContent.Add(""); // Add an empty line for non-paragraph elements
                        }

                        var textFormatterViewModel = (TextFormatterViewModel)DataContext;

                        // Update the view model (ideally we would automatically data-bind to the RichTextBox,
                        // but this does not appear to be supported, so we do it manually here).
                        textFormatterViewModel.HandleTextChanged(textContent, caretParagraphIndex, plainTextCaretOffset);

                        // Perform the other direction of data-binding and update the view
                        UpdateFromViewModel();

                        // Set the caret position back to what it should be
                        int caretOffset = textFormatterViewModel.CaretOffset;
                        if (caretOffset >= 0)
                        {
                            TextPointer newPos = caretParagraph.ContentStart.GetPositionAtOffset(caretOffset);
                            if (null != newPos)
                                MainTextBox.CaretPosition = newPos;
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("MESSAGE: " + exc.Message + Environment.NewLine + "STACK TRACE: " + exc.StackTrace, "Exception Thrown!", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }

                // Now that we're done modifying the content, we can set suppressTextChangeHandling back to false.
                suppressTextChangeHandling = false;
            }
        }

        /// <summary>
        /// Update the view from the view model
        /// </summary>
        private void UpdateFromViewModel()
        {
            if (DataContext is TextFormatterViewModel)
            {
                // Update the View from the ViewModel (ideally we would automatically data-bind to the RichTextBox,
                // but this does not appear to be supported, so we do it manually here).
                var textForDisplay = ((TextFormatterViewModel)DataContext).FormattedText;
                BlockCollection blocks = MainTextBox.Document.Blocks;
                for (int i = 0; i < blocks.Count && i < textForDisplay.Count; ++i)
                {
                    if (blocks.ElementAt<Block>(i) is Paragraph)
                    {
                        // Clear all of the inlines from the paragraph and then add them back from textForDisplay
                        Paragraph p = (Paragraph)blocks.ElementAt<Block>(i);
                        p.Inlines.Clear();
                        foreach (var txtElem in textForDisplay[i])
                            p.Inlines.Add(new Run(txtElem.Item1) { Foreground = txtElem.Item2 ? Brushes.Red : Brushes.Black });
                    }
                }
            }
        }

        /// <summary>
        /// Compute the offset to the caret in terms of actual displayed characters (ignore formatting characters).
        /// This will be -1 if there is no paragraph that scopes the current position.
        /// </summary>
        /// <param name="caretPos">TextPointer to the current caret position</param>
        /// <returns>Offset to the caret in terms of actual displayed characters</returns>
        private static int ComputePlaintextOffset(TextPointer caretPos)
        {
            int plainTextCaretOffset = -1;
            Paragraph paragraph = caretPos.Paragraph;
            if (null != paragraph)
            {
                plainTextCaretOffset = paragraph.ContentStart.GetOffsetToPosition(caretPos);

                // Locate which inline the caret is in
                foreach (Inline il in paragraph.Inlines)
                {
                    // Stop if the caret is in the current Inline object
                    if (caretPos.GetOffsetToPosition(il.ContentEnd) >= 0)
                    {
                        // Subtract only 1, for the start of the current inline.
                        plainTextCaretOffset -= 1;
                        break;
                    }

                    // Each new inline object adds 2 to the caret position (1 for the start of the inline
                    // and 1 more for the end of the inline), so compensate by subtracting 2
                    plainTextCaretOffset -= 2;
                }
            }
            return plainTextCaretOffset;
        }
    }
}
