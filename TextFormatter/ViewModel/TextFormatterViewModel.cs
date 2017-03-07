using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextFormatter.Model; 
using System.Collections.ObjectModel;

namespace TextFormatter.ViewModel
{
    public class TextFormatterViewModel
    {
        /// <summary>
        /// Default constructor. Calls LoadFormattedText to initilize the text.
        /// </summary>
        public TextFormatterViewModel()
        {
            LoadFormattedText(); 
        }

        /// <summary>
        /// Represents the text displayed in the rich text box
        /// </summary>
        public TextFormatterModel DisplayedText
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the initial text (single line consisting of just an empty string)
        /// </summary>
        public void LoadFormattedText()
        {
            DisplayedText = new TextFormatterModel { Text = "" };
        }
    }
}
