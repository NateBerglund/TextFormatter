using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TextFormatter.Model
{
    /// <summary>
    /// Represents a text element that can occur within the main text box. This can be a string of plaintext,
    /// a hashtag, or a username. Instances of this type are immutable and validation is done inside of the
    /// constructor to guarantee instances can only be valid hashtags or usernames (in the case
    /// of plaintext, the only restriction is that the underlying string be non-empty)
    /// </summary>
    public class TextElementModel
    {
        /// <summary>
        /// Type of text. Can be PlainText, HashTag or UserName.
        /// </summary>
        private readonly TextType _textType;

        /// <summary>
        /// Content of the text (not including the initial &#64; symbol for a username or &#35; symbol for a hashtag)
        /// </summary>
        private readonly String _text;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="textType">Type of text. Can be PlainText, HashTag or UserName.</param>
        /// <param name="text">Content of the text (allowed to either include the initial &#64; symbol for a username or &#35; symbol for a hashtag or not include it)</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the user attempts to construct an invalid hashtag or username, or tries to construct a TextElementModel
        /// with an empty string, which is not allowed.
        /// </exception>
        public TextElementModel(TextType textType, String text)
        {
            // Validate
            if (text.Length == 0)
                throw new ArgumentException("TextElementModel cannot be an empty string!", "text");
            // Remove the prefix, if necessary
            if (textType == TextType.HashTag && text[0] == '#')
                text = text.Substring(1); // Remove first character
            if (textType == TextType.UserName && text[0] == '@')
                text = text.Substring(1); // Remove first character
            if (textType == TextType.HashTag || textType == TextType.UserName)
            {
                MatchCollection matches = Regex.Matches(text, @"\w+", RegexOptions.IgnoreCase);
                if (matches.Count != 1)
                    throw new ArgumentException(
                        (textType == TextType.HashTag ? "Hashtag" : "UserName") + "cannot contain non-alphanumeric characters!", "text"
                        );
                foreach (Match m in matches) // At this point there will be only one match
                    if (m.Index != 0 || m.Length != text.Length) // it must match the whole string
                        throw new ArgumentException(
                            (textType == TextType.HashTag ? "Hashtag" : "UserName") + "cannot contain non-alphanumeric characters!", "text"
                            );
            }

            // Set member variables
            _textType = textType;
            _text = text;
        }

        /// <summary>
        /// Text type. Can be PlainText, HashTag or UserName.
        /// </summary>
        public TextType TextType
        {
            get
            {
                return _textType;
            }
        }

        /// <summary>
        /// Text content, not including the starting symbol to define a hashtag or username.
        /// </summary>
        public String Text
        {
            get
            {
                return _text;
            }
        }

        /// <summary>
        /// Get the full text content including the starting symbol to define a hashtag or username.
        /// </summary>
        public String FullText
        {
            get
            {
                switch (_textType)
                {
                    case TextType.PlainText:
                        return _text;
                    case TextType.HashTag:
                        return "#" + _text;
                    case TextType.UserName:
                        return "@" + _text;
                    default:
                        return _text;
                }
            }
        }

        /// <summary>
        /// Helper function to return the number of prefix characters that are used for a given text type.
        /// </summary>
        /// <param name="textType">TextType (PlainText, HashTag or UserName)</param>
        /// <returns>Number of characters in the prefix of this text type</returns>
        public static int PrefixLength(TextType textType)
        {
            switch (textType)
            {
                case TextType.PlainText:
                    return 0;
                case TextType.HashTag:
                    return 1;
                case TextType.UserName:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
