using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace TextFormatter.Model
{
    /// <summary>
    /// Class that represents formatted text
    /// </summary>
    public class TextFormatterModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Private member variable that stores the text as a list of lists.
        /// Each sublist represents a line of text that may be broken into
        /// multiple components.
        /// </summary>
        private List<List<TextElementModel>> text;

        /// <summary>
        /// Property that wraps the text member variable. Setter method
        /// both changes the property and fires an event indicating that
        /// it changed.
        /// </summary>
        public List<List<TextElementModel>> Text
        {
            get
            {
                return text;
            }

            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        /// <summary>
        /// Add a line of text to the end of the text, automatically parsing it into
        /// plaintext, hashtags and usernames.
        /// </summary>
        /// <param name="line">Plaintext line to add</param>
        public void AddLine(String line)
        {
            text.Add(ParseContent(line));
            OnPropertyChanged("Text");
        }

        /// <summary>
        /// Parse the content in a string into substrings that correspond to different types of text,
        /// such as plaintext, hashtags and usernames.
        /// </summary>
        /// <param name="content">Content to be parsed</param>
        /// <exception cref="Exception">
        /// Can be thrown if there is an internal error in the logic that causes matches for hashtags and usernames
        /// to overlap. If this exception is thrown it most likely means there is an error in the source code of this function.
        /// </exception>
        /// <returns>List of tuples, each containing a string and a boolean (true if we should highlight the string)</returns>
        private List<TextElementModel> ParseContent(String content)
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
            var allMatches = new List<Tuple<int, int, TextType>>(); // lists the starting and ending indices of all matches, as well as their type
            foreach (Match m in hashTagMatches)
                allMatches.Add(new Tuple<int, int, TextType>(m.Index, m.Index + m.Length - 1, TextType.HashTag));
            foreach (Match m in usernameMatches)
                allMatches.Add(new Tuple<int, int, TextType>(m.Index, m.Index + m.Length - 1, TextType.UserName));
            allMatches.Sort((x, y) => x.Item1.CompareTo(y.Item1)); // sort by starting index
            for (int i = 1; i < allMatches.Count; ++i)
                if (allMatches[i - 1].Item2 >= allMatches[i].Item1)
                    throw new Exception("Error in ParseContent: matches overlap!"); // shouldn't ever happen if reg expressions are correct

            // Construct the return value, consisting of highlighted and non-highlighted strings
            var retVal = new List<TextElementModel>();
            int lastSubstrEnd = -1; // keeps track of the index of the last character of the last substring processed
            foreach (var match in allMatches)
            {
                // Add non-highlighted text to the content
                if (match.Item1 > lastSubstrEnd + 1)
                {
                    retVal.Add(new TextElementModel(TextType.PlainText, content.Substring(lastSubstrEnd + 1, match.Item1 - 1 - lastSubstrEnd)));
                    lastSubstrEnd = match.Item1 - 1; // update the position of the last substring processed
                }
                // Add highlighted text to the content
                retVal.Add(new TextElementModel(match.Item3, content.Substring(match.Item1 , match.Item2 - match.Item1 + 1)));
                lastSubstrEnd = match.Item2; // update the position of the last substring processed
            }
            // Add any remaining non-highlighted text to the content
            if (lastSubstrEnd < content.Length - 1)
            {
                retVal.Add(new TextElementModel(TextType.PlainText, content.Substring(lastSubstrEnd + 1, content.Length - 1 - lastSubstrEnd)));
                lastSubstrEnd = content.Length - 1; // update the position of the last substring processed
            }

            return retVal;
        }

        /// <summary>
        /// Event that occurs when a property value changes,
        /// required in order to implement the INotifyPropertyChanged Interface.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event, with event arguments indicating
        /// the name of the property that changed.
        /// </summary>
        /// <param name="property">Name of the property that changed</param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
