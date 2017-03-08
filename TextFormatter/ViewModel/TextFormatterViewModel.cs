using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using TextFormatter.Model;

namespace TextFormatter.ViewModel
{
    public class TextFormatterViewModel
    {
        /// <summary>
        /// Default constructor. Calls InitializeText to initilize the text.
        /// </summary>
        public TextFormatterViewModel()
        {
            InitializeText(); 
        }

        /// <summary>
        /// Loads the initial text (single line consisting of just an empty string)
        /// </summary>
        public void InitializeText()
        {
            _textFormatterModel = new TextFormatterModel() { Text = new List<List<TextElementModel>>() };
        }

        /// <summary>
        /// Tracks which paragraph the caret is in (or -1 if no paragraph)
        /// </summary>
        public int CaretParagraphIndex
        {
            get
            {
                return _caretParagraphIndex;
            }
        }

        /// <summary>
        /// Caret offset that should be used to keep the caret in the same place as it was before modifying the content.
        /// </summary>
        public int CaretOffset
        {
            get
            {
                return _adjustedCaretOffset;
            }
        }

        /// <summary>
        /// Updates the ViewModel when a change occurs to the text in the textbox
        /// </summary>
        /// <param name="textContent">Plain text content pulled from the UI</param>
        /// <param name="caretParagraphIndex">Index of the paragraph containing the caret</param>
        /// <param name="plainTextCaretOffset">Plaintext offset of the caret within its paragraph</param>
        public void HandleTextChanged(List<String> textContent, int caretParagraphIndex, int plainTextCaretOffset)
        {
            // Store information pertaining to the location of the caret
            _caretParagraphIndex = caretParagraphIndex;
            _plainTextCaretOffset = plainTextCaretOffset;

            // Recalculate the displayed text
            _textFormatterModel.Text = new List<List<TextElementModel>>();
            foreach (String line in textContent)
            {
                _textFormatterModel.AddLine(line); // Add each line, automatically formatting it
            }

            ComputeAdjustedCaretPos(); // Compute the new location for the caret
        }

        /// <summary>
        /// Represents the formatted text as it appears to the View.
        /// This takes the form of a list of lines, where each line is a list of string-boolean pairs
        /// (boolean is true if we should highlight the string).
        /// </summary>
        public List<List<Tuple<string, bool>>> FormattedText
        {
            get
            {
                var retVal = new List<List<Tuple<string, bool>>>();
                foreach (var line in _textFormatterModel.Text)
                {
                    retVal.Add(new List<Tuple<string, bool>>());
                    foreach (TextElementModel txt in line)
                    {
                        // The rule txt.TextType != TextType.PlainText ensures we highlight anything other than plaintext
                        retVal[retVal.Count - 1].Add(new Tuple<string, bool>(txt.FullText, txt.TextType != TextType.PlainText));
                    }
                }
                return retVal;
            }
        }

        /// <summary>
        /// Data model for the text displayed in the rich text box.
        /// </summary>
        private TextFormatterModel _textFormatterModel;

        /// <summary>
        /// Represents the offset the caret would have if the content of the RichTextBox were
        /// plain text (no special formatting). Equal to -1 if the caret is not inside a Paragraph object.
        /// </summary>
        private int _plainTextCaretOffset;

        /// <summary>
        /// Represents the actual offset the caret should have, given the formatting of the current paragraph.
        /// Value is unspecified if the caret is not inside a Paragraph object.
        /// </summary>
        private int _adjustedCaretOffset;

        /// <summary>
        /// Private member behind the CaretParagraphIndex property
        /// </summary>
        private int _caretParagraphIndex;

        /// <summary>
        /// Function to compute the adjusted caret position after the content has changed.
        /// This can be retrieved through the property "CaretOffset". Note that if the caret
        /// is not in any Paragraph object, the value of CaretOffset will be -1.
        /// </summary>
        private void ComputeAdjustedCaretPos()
        {
            _adjustedCaretOffset = -1; // default value

            // Return immediately if the caret is not in any paragraph
            if (_caretParagraphIndex < 0)
                return;

            // Initialize to the plain text offset
            _adjustedCaretOffset = _plainTextCaretOffset;

            // Locate which inline the caret is in
            var textElemsForParagraph = _textFormatterModel.Text[_caretParagraphIndex];
            int currentOffset = 0; // Current offset (measured as a plaintext offset)
            foreach (TextElementModel tem in textElemsForParagraph)
            {
                // Stop if the caret will be in the current text element
                if (currentOffset + tem.FullText.Length >= _plainTextCaretOffset)
                {
                    // Add only 1, for the start of the current text element.
                    _adjustedCaretOffset += 1;
                    break;
                }

                // Each new inline object adds 2 to the caret position (1 for the start of the text element
                // and 1 more for the end of the text element)
                _adjustedCaretOffset += 2;

                // Increment current offset to add the length of this text element
                currentOffset += tem.FullText.Length;
            }
        }
    }
}
