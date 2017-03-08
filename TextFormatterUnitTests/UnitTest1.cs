using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TextFormatter.ViewModel;

namespace TextFormatterUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Construct a TextFormatterViewModel
            TextFormatterViewModel textFormatterViewModel = new TextFormatterViewModel();

            // Add an example line of text
            List<String> textContent = new List<String>();
            textContent.Add("@you #hello there #welcome to @ our");
            int caretParagraphIndex = 0;
            int plainTextCaretOffset = 28;
            textFormatterViewModel.HandleTextChanged(textContent, caretParagraphIndex, plainTextCaretOffset);
            var FormattedText = textFormatterViewModel.FormattedText;
            int CaretOffset = textFormatterViewModel.CaretOffset;

            // There should be only 1 line in the result
            Assert.AreEqual(1, FormattedText.Count);

            // There should be 6 pieces
            Assert.AreEqual(6, FormattedText[0].Count);

            // First piece
            Assert.AreEqual("@you", FormattedText[0][0].Item1);
            Assert.AreEqual(true, FormattedText[0][0].Item2); // highlighted

            // Second piece
            Assert.AreEqual(" ", FormattedText[0][1].Item1);
            Assert.AreEqual(false, FormattedText[0][1].Item2); // not highlighted

            // Third piece
            Assert.AreEqual("#hello", FormattedText[0][2].Item1);
            Assert.AreEqual(true, FormattedText[0][2].Item2); // highlighted

            // Fourth piece
            Assert.AreEqual(" there ", FormattedText[0][3].Item1);
            Assert.AreEqual(false, FormattedText[0][3].Item2); // not highlighted

            // Fifth piece
            Assert.AreEqual("#welcome", FormattedText[0][4].Item1);
            Assert.AreEqual(true, FormattedText[0][4].Item2); // highlighted

            // Sixth piece
            Assert.AreEqual(" to @ our", FormattedText[0][5].Item1);
            Assert.AreEqual(false, FormattedText[0][5].Item2); // not highlighted

            // Adjusted caret position should be 39
            Assert.AreEqual(39, CaretOffset);
        }
    }
}
