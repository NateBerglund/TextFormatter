﻿using System;
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
using System.Text.RegularExpressions;

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

                // Since we can't modify the list while we're iterating through it, we first process it to
                // determine how each line of text needs to be broken up. We keep these in a list of Inline objects
                // for each paragraph.
                BlockCollection blocks = MainTextBox.Document.Blocks;
                List<Tuple<Paragraph, List<Inline>>> insertionsToProcess = new List<Tuple<Paragraph, List<Inline>>>();
                foreach (Block b in blocks)
                {
                    if (b is Paragraph)
                    {
                        Paragraph p = (Paragraph)b;
                        // Extract the plain text corresponding to this paragraph
                        TextRange tr = new TextRange(
                                    p.ContentStart,
                                    p.ContentEnd);
                        string txt = tr.Text;

                        insertionsToProcess.Add(Tuple.Create(p, new List<Inline>()));
                        List<Tuple<string, bool>> parsedTxt = ParseContent(txt);
                        // Replace the run with set of runs, divided into red text and black text
                        for (int i = 0; i < parsedTxt.Count; ++i)
                        {
                            if (parsedTxt[i].Item2)
                                insertionsToProcess[insertionsToProcess.Count - 1].Item2.Add(new Run(parsedTxt[i].Item1) { Foreground = Brushes.Red });
                            else
                                insertionsToProcess[insertionsToProcess.Count - 1].Item2.Add(new Run(parsedTxt[i].Item1) { Foreground = Brushes.Black });
                        }
                    }
                }

                // Do the replacements
                foreach (Tuple<Paragraph, List<Inline>> tpl in insertionsToProcess)
                {
                    Paragraph p = tpl.Item1;
                    p.Inlines.Clear();
                    
                    foreach (Inline toAdd in tpl.Item2)
                    {
                        p.Inlines.Add(toAdd);
                    }
                }

                // Now that we're done modifying the content, we can set suppressTextChangeHandling back to false.
                suppressTextChangeHandling = false;
            }
        }

        /// <summary>
        /// Parse the content in a string into substrings that should be highlighted or not highlighted,
        /// based on the rules defining a hashtag and a username.
        /// </summary>
        /// <param name="content">Content to be parsed</param>
        /// <returns>List of tuples, each containing a string and a boolean (true if we should highlight the string)</returns>
        private List<Tuple<string, bool>> ParseContent(string content)
        {
            // Rules: Define a "token" as a maximal run of consecutive non-whitespace characters. A valid hashtag is a
            // '#' character followed by 1 or more alphanumeric characters that occurs at the end of a token (or is an
            // entire token). Other characters may precede the '#' in the token, so long as they are not alphanumeric.
            // To implement this as a regular expression, we start with a lookbehind assertion that we have either
            // a whitespace character or the beginning of the string/line, followed by 0 or more non-alphanumeric characters,
            // then we match the hashtag itself by looking for a '#' symbol followed by 1 or more alphanumeric characters,
            // and finally end with a lookahead assertion that have either a whitespace character or the end of the string/line.
            // A valid username is an entire token starting with '@' and followed by 1 or more alphanumeric characters.
            // We can construct its regular expression similarly except that we don't need to match 0 or more non-alphanumeric
            // characters in the lookbehind assertion.
            const string hashTagPattern = @"(?<=(\s|^)\W*)#\w+(?=\s|$)";
            const string usernamePattern = @"(?<=\s|^)@\w+(?=\s|$)";

            // Begin by constructing a list of all of the matches found
            MatchCollection hashTagMatches = Regex.Matches(content, hashTagPattern, RegexOptions.IgnoreCase);
            MatchCollection usernameMatches = Regex.Matches(content, usernamePattern, RegexOptions.IgnoreCase);
            var allMatches = new List<Tuple<int, int>>(); // lists the starting and ending indices of all matches
            foreach (Match m in hashTagMatches)
                allMatches.Add(new Tuple<int, int>(m.Index, m.Index + m.Length - 1));
            foreach (Match m in usernameMatches)
                allMatches.Add(new Tuple<int, int>(m.Index, m.Index + m.Length - 1));
            allMatches.Sort((x, y) => x.Item1.CompareTo(y.Item1)); // sort by starting index
            for (int i = 1; i < allMatches.Count; ++i)
                if (allMatches[i - 1].Item2 >= allMatches[i].Item1)
                    throw new Exception("Error in ParseContent: matches overlap!"); // shouldn't ever happen if reg expressions are correct

            // Construct the return value, consisting of highlighted and non-highlighted strings
            var retVal = new List<Tuple<string, bool>>();
            int lastSubstrEnd = -1; // keeps track of the index of the last character of the last substring processed
            foreach(var match in allMatches)
            {
                // Add non-highlighted text to the content
                if (match.Item1 > lastSubstrEnd + 1)
                {
                    retVal.Add(new Tuple<string, bool>(content.Substring(lastSubstrEnd + 1, match.Item1 - 1 - lastSubstrEnd), false));
                    lastSubstrEnd = match.Item1 - 1; // update the position of the last substring processed
                }
                // Add highlighted text to the content
                retVal.Add(new Tuple<string, bool>(content.Substring(match.Item1, match.Item2 - match.Item1 + 1), true));
                lastSubstrEnd = match.Item2; // update the position of the last substring processed
            }
            // Add any remaining non-highlighted text to the content
            if (lastSubstrEnd < content.Length - 1)
            {
                retVal.Add(new Tuple<string, bool>(content.Substring(lastSubstrEnd + 1, content.Length - 1 - lastSubstrEnd), false));
                lastSubstrEnd = content.Length - 1; // update the position of the last substring processed
            }

            return retVal;
        }
    }
}