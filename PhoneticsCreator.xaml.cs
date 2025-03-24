using OIT_HelpDesk_Assistant_v2.Phonetics;
using OIT_HelpDesk_Assistant_v2.SearchPage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace OIT_HelpDesk_Assistant_v2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PhoneticsCreator : Window
    {
        // --- VARIABLES ---

        public enum PhoneticsCreatorResult 
        {
            Cancelled,
            Success
        }

        /// <summary>
        /// Class for allowing a 2 way binding between information inside of the ItemsControl Sytyle and the backend
        /// </summary>
        public class TextBasedItem : INotifyPropertyChanged
        {
            /// <summary>
            /// private text information
            /// </summary>
            private string _Text { get; set; }

            /// <summary>
            /// Public text information with event update for the set property
            /// </summary>
            public string Text
            {
                get => _Text;
                set
                {
                    if (_Text != value)
                    {
                        _Text = value;
                        OnPropertyChanged(nameof(Text));
                        OnTextChanged();
                    }
                }
            }

            /// <summary>
            /// private text information
            /// </summary>
            private char _Letter { get; set; }

            /// <summary>
            /// Public text information with event update for the set property
            /// </summary>
            public char Letter
            {
                get => _Letter;
                set
                {
                    if (_Letter != value)
                    {
                        _Letter = value;
                        OnPropertyChanged(nameof(Letter));
                        // don't trigger text changed events on textblocks
                    }
                }
            }

            /// <summary>
            /// Declare event for when the text is changed
            /// </summary>
            public event EventHandler<TextBasedItem> TextChanged;

            /// <summary>
            /// Function for when text is changed
            /// </summary>
            private void OnTextChanged()
            {
                TextChanged?.Invoke(this, this);
            }

            /// <summary>
            /// Declare event for property changes
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Function for when property changes
            /// </summary>
            /// <param name="propertyName"> Name of changed property </param>
            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Two-Way-Binded data for the text block labels
        /// </summary>
        public List<ObservableCollection<TextBasedItem>> InputGridOptions { get; init; } = new();

        /// <summary>
        /// Describes which phonetics are correct by textbox
        /// </summary>
        private Dictionary<char, bool> CorrectIndexes { get; set; } = new();

        /// <summary>
        /// Tells the amount of words that are currently correct
        /// </summary>
        private int AmountCorrect { get; set; } = 0;

        /// <summary>
        /// The phonetics set being created
        /// </summary>
        public PhoneticsObject Phonetic { get; private set;  } = new(blank_authored_set:true);

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Sets up the window with the right amount of text boxes
        /// </summary>
        /// <param name="phonetic"> Phonetic to derive off of, null is an empty create window </param>
        public PhoneticsCreator(PhoneticsObject? phonetic=null)
        {
            InitializeComponent();

            // boolean for checking if to derive or not
            bool do_derive = (phonetic != null);

            // set up dictionary of correct indexes
            foreach (char letter in PhoneticsList.Alphabet)
            {
                CorrectIndexes[letter] = false;
            }

            // get the number of divisions based on the amount of columns
            int divisions = PhoneticsCreator_StackPanelGrid.ColumnDefinitions.Count;

            // add the divisions amount to the InputGridOptions list
            for (int i = 0; i < divisions; i++)
            {
                InputGridOptions.Add(new());
            }

            int division_size = (int)Math.Round((float)PhoneticsList.Alphabet.Length / divisions); // get the current division of Alphabet
            // for each collection in the InputGridOptions list
            for (int j = 0; j < InputGridOptions.Count; j++)
            {
                int curr_offset = j * division_size; // get the offset from the division and current index
                // add the correct amount of letters in each row from the Alphabet
                for (int i = 0; i < division_size; i++)
                {
                    int curr_index = curr_offset + i;
                    char curr_letter = PhoneticsList.Alphabet[curr_index];
                    // add text block label
                    InputGridOptions[j].Add(new() {
                        Text = ((do_derive == true) ? phonetic[curr_index] : ""),
                        Letter = Char.ToUpper(curr_letter) // set the letter to the uppercase letter of the alphabet
                    });
                    // simulates changing text to verify correctness (can only happen after CorrectIndexes has been set up
                    if (do_derive == true)
                    { // the whole point of doing this is to ensure that the phonetic's wordlist is set correctly and the correct indexes and such
                        Simulate_InputGrid_TextChanged(InputGridOptions[j][i]);
                    }
                }
            }

            // if deriving, set the name and simulate changing it
            if (do_derive == true)
            {
                PhoneticsCreator_NameTextBox.Text = phonetic.Name;
                // automatically updates because we changed the text
            }

            // Set the DataContext for data binding
            DataContext = this;
        }

        // --- EVENT METHODS ---

        /// <summary>
        /// Changes the state of the Create Button if all text boxes are correct (green) by checking the underlying variables for why they are correct
        /// </summary>
        private void ChangeCreateButtonEnabledState()
        {

            // check if all are correct and set button accordingly
            if ((AmountCorrect == PhoneticsList.Alphabet.Length) // all phonetics have been filled correctly
                && (Phonetic.Name != "default")) // name is filled
            {
                PhoneticsCreator_CreateButton.IsEnabled = true;
            }
            else // not all correct
            {
                PhoneticsCreator_CreateButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Simulates inputting text into the grid text boxes, but doesn't change any TextBox settings
        /// </summary>
        /// <param name="curr_text_item"> The text based item to simulate to/from </param>
        private void Simulate_InputGrid_TextChanged(TextBasedItem curr_text_item)
        {
            // get the current letter
            char curr_letter_lower = Char.ToLower(curr_text_item.Letter);

            // display outline if...
            if ((!string.IsNullOrWhiteSpace(curr_text_item.Text)) // value is not empty
                && (Char.ToLower(curr_text_item.Text[0]) == curr_letter_lower)) // first letter matches current letter
            {
                // only incrememnt if previously was incorrect 
                if (CorrectIndexes[curr_letter_lower] == false) // was incorrect
                {
                    AmountCorrect += 1;
                    CorrectIndexes[curr_letter_lower] = true;
                }

                Phonetic[curr_letter_lower] = curr_text_item.Text; // set text to Phonetic
            }
            else // value is empty or doesn't start with correct character
            {
                // only decrememnt if previously was correct
                if (CorrectIndexes[curr_letter_lower] == true) // was correct
                {
                    AmountCorrect -= 1;
                    CorrectIndexes[curr_letter_lower] = false;
                }

                // erasing text would screw it up so we do nothing
            }

            // enable button or not
            ChangeCreateButtonEnabledState();
        }

        /// <summary>
        /// Handles setting values to the Phonetic object and checking their correctness
        /// </summary>
        /// <param name="sender"> Sender will be a TextBox here which can be translated into a TextBasedItem </param>
        private void PhoneticsCreator_InputGrid_TextChanged(object sender, TextChangedEventArgs e)
        {
            // find current text box from sender
            TextBox? curr_text_box = sender as TextBox;
            if (curr_text_box == null) return; // do nothing if null
            if (!(curr_text_box.DataContext is TextBasedItem)) return; // if the sender isn't a text based item then return

            // get the current letter
            char curr_letter_lower = Char.ToLower(((TextBasedItem)curr_text_box.DataContext).Letter);

            // display outline if...
            if ((!string.IsNullOrWhiteSpace(curr_text_box.Text)) // value is not empty
                && (Char.ToLower(curr_text_box.Text[0]) == curr_letter_lower)) // first letter matches current letter
            {
                // only incrememnt if previously was incorrect 
                if (CorrectIndexes[curr_letter_lower] == false) // was incorrect
                {
                    AmountCorrect += 1;
                    CorrectIndexes[curr_letter_lower] = true;
                }
                curr_text_box.BorderBrush = new SolidColorBrush(Colors.LimeGreen); // green border

                Phonetic[curr_letter_lower] = curr_text_box.Text; // set text to Phonetic
            } else // value is empty or doesn't start with correct character
            {
                // only decrememnt if previously was correct
                if (CorrectIndexes[curr_letter_lower] == true) // was correct
                {
                    AmountCorrect -= 1;
                    CorrectIndexes[curr_letter_lower] = false;
                }
                curr_text_box.BorderBrush = new SolidColorBrush(Colors.Red); // red border

                // erasing text would screw it up so we do nothing
            }

            // enable button or not
            ChangeCreateButtonEnabledState();
        }

        /// <summary>
        /// Checks if the name is anything at all and updates Phonetic
        /// </summary>
        private void PhoneticsCreator_NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // change invalid characters and show a short tooltip
            if (Regex.IsMatch(PhoneticsCreator_NameTextBox.Text, MainWindow.InvalidCharacters)) // check if the characters are in the name
            {
                // replace the characters and reposition the cursor
                int curr_pos = PhoneticsCreator_NameTextBox.CaretIndex;
                PhoneticsCreator_NameTextBox.Text = Regex.Replace(PhoneticsCreator_NameTextBox.Text, MainWindow.InvalidCharacters, "");
                if ((curr_pos - 1) <= PhoneticsCreator_NameTextBox.Text.Length)
                { // if the index - 1 is still in the length
                    PhoneticsCreator_NameTextBox.CaretIndex = (curr_pos - 1);
                } else
                {
                    PhoneticsCreator_NameTextBox.CaretIndex = 0;
                }

                // show tooltip of invalids
                DisplayableDictionary.ShortToolTip(
                    PhoneticsCreator_NameTextBox,
                    $"You cannot use characters of {MainWindow.InvalidCharacters}",
                    3);
            }

            // color changes
            if (!string.IsNullOrWhiteSpace(PhoneticsCreator_NameTextBox.Text) // correct (not empty)
                && (!PhoneticsCreator_NameTextBox.Text.StartsWith(" "))) // correct (doesn't start with space)
            {
                PhoneticsCreator_NameTextBox.BorderBrush = new SolidColorBrush(Colors.LimeGreen);
                Phonetic.Name = PhoneticsCreator_NameTextBox.Text; // set to current
            }
            else // incorrect (nothing)
            {
                PhoneticsCreator_NameTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                Phonetic.Name = "default"; // sets to default if incorrect
            }

            // enable button or not
            ChangeCreateButtonEnabledState();
        }

        /// <summary>
        /// Sets the output result (which closes the window)
        /// </summary>
        private void PhoneticsCreator_CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // set the section
            Phonetic.SectionInt = 1; // set to custom

            // Set the result of the task to indicate success
            _CreateButtonClicked_Task?.SetResult(PhoneticsCreatorResult.Success);
        }

        // --- METHODS ---

        /// <summary>
        /// Function specific variables for tracking the button task
        /// </summary>
        private TaskCompletionSource<PhoneticsCreatorResult> _CreateButtonClicked_Task { get; set; }

        /// <summary>
        /// Show method for this window
        /// </summary>
        /// <remarks>
        /// Hides the original Window.Show() 'base.Show()' method
        /// </remarks>
        /// <returns> PhoneticsCreatorResult result successful or cancelled </returns>
        public new async Task<PhoneticsCreatorResult> Show()
        {
            // create a new task that must be completed and set default value (cancelled)
            _CreateButtonClicked_Task = new TaskCompletionSource<PhoneticsCreatorResult>();

            // show the window, wait for button press and then close it and return the result
            base.Show();
            PhoneticsCreatorResult result = await _CreateButtonClicked_Task.Task;
            this.Close();
            return result;
        }

        // --- OVERRIDES ---

        /// <summary>
        /// Overrides the closed event to ensure that the window is set to 'Cancelled' when it's closed
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_CreateButtonClicked_Task.Task.IsCompleted == false) // if the task hasn't been completed (success)
            { // set to cancelled
                _CreateButtonClicked_Task.SetResult(PhoneticsCreatorResult.Cancelled);
            }
        }

        /// <summary>
        /// Function for keys being pressed
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: // enter
                    if (PhoneticsCreator_CreateButton.IsEnabled == true) PhoneticsCreator_CreateButton_Click(sender, e); // simulate pressing create button if enter pressed
                    break;
            }
        }
    }
}
