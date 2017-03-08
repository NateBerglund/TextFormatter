using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFormatter.Model
{
    /// <summary>
    /// Represents a type of text that can occur in our application
    /// </summary>
    public enum TextType
    {
        /// <summary>
        /// Plain text. Needs no special formatting
        /// </summary>
        PlainText,

        /// <summary>
        /// Hash tag.
        /// </summary>
        HashTag,

        /// <summary>
        /// Username.
        /// </summary>
        UserName
    }
}
