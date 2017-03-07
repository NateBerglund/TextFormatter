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
        /// Private member variable that stores the text
        /// </summary>
        private string text;

        /// <summary>
        /// Property that wraps the text member variable. Setter method
        /// both changes the property and fires an event indicating that
        /// it changed (if the new value is not equal to the existing value).
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
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
