using OIT_HelpDesk_Assistant_v2.Phonetics;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static OIT_HelpDesk_Assistant_v2.PhoneticsCreator;
using static System.Collections.Specialized.BitVector32;

namespace OIT_HelpDesk_Assistant_v2
{
    /// <summary>
    /// Phonetics portion of the MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        // --- VARIABLES ---

        /// <summary>
        /// Default value container
        /// </summary>
        private static class PhoneticsDefault {
            /// <summary>
            /// Default string to be prepended to numbers
            /// </summary>
            public const string NumberPrepension = "The number ";

            /// <summary>
            /// Default selection after phonetics has been loaded
            /// </summary>
            public const string TypeComboBox_DefaultName = "HelpDesk";

            /// <summary>
            /// Determines whether or not the set warning (joke and custom) should be displayed
            /// </summary>
            public static bool DisplaySetWarning = true;
        }

        /// <summary>
        /// Stores all object options
        /// </summary>
        private static Dictionary<string, List<PhoneticsObject>> AllPhonetics = new();

        /// <summary>
        /// Class for pointing to phonetics easily between the UI and AllPhonetics list
        /// </summary>
        internal sealed class PhoneticsPointer
        {
            // -- VARIABLES --
            public string Section { get; init; }
            public int Index { get; init; }

            // -- CONSTRUCTOR --
            public PhoneticsPointer(string section, int index)
            {
                Section = section;
                Index = index;
            }
        }

        /// <summary>
        /// Stores a dictionary of pointers of where to find Phonetics
        /// </summary>
        private static List<PhoneticsPointer> PhoneticsPointers = new();

        /// <summary>
        /// Stores the currently selected objects
        /// </summary>
        private static List<PhoneticsPointer> CurrentPointers = new();

        // --- METHODS ---

        /// <summary>
        /// Loads/Refreshes the phonetics lists to both the backend and also the combobox
        /// </summary>
        private void LoadPhonetics()
        {
            // attempts to hold the selection in place if it still exists, but obviously if it update you will have moved places
            int curr_selected_index = Phonetics_TypeComboBox.SelectedIndex;

            // wipe data sets
            AllPhonetics.Clear();
            PhoneticsPointers.Clear();
            Phonetics_TypeComboBox.Items.Clear();

            // load sections before continuing (will clear them too)
            PhoneticsObject.LoadSections();

            // add sections and section headers to AllPhonetics
            int j = 0; // track j to get the section's index
            foreach (PhoneticsSection section in PhoneticsObject.Sections) // for each section in Sections
            {
                if (section.Name.ToLower() == "none") { j += 1; continue; } // don't include anything with the None section
                if (section.Name.ToLower() == "custom") { j += 1; continue; } // skip custom, because we add it later
                AllPhonetics.Add(section.Name, new() { new PhoneticsObject(j) }); // add a new list to AllPhonetics with the section as the key and with a section header in it
                j += 1;
            }
            // add custom (always last)
            AllPhonetics.Add(PhoneticsObject.Sections[1], new() { new PhoneticsObject(1) });

            // sorts phonetics by section
            foreach (string file_path in Directory.GetFiles(Json.CreatePath(PhoneticsObject.ListsPath.ToList()))) // for each file in the directory
            {
                string file_name = Path.GetFileName(file_path); // get just the file name
                if (!file_name.EndsWith(".json")) continue; // don't include files that dont end with json
                string file_name_no_extension = Path.GetFileNameWithoutExtension(file_path); // no extension so it can be appended (as .json) later (name shouldn't have an extension)
                try // try to add to options
                {
                    var curr_phonetics = new PhoneticsObject(file_name_no_extension); // load only
                    if (curr_phonetics.Section.Name.ToLower() == "none") continue; // don't include anything with the None section
                    AllPhonetics[curr_phonetics.Section.Name].Add(curr_phonetics); // add
                }
                catch (FileNotFoundException) { } // error reading, try to continue
                catch (FileEmptyException) { } // file empty, try to continue
            }

            // load names to the combobox
            foreach (string section_name in AllPhonetics.Keys) // for each key in the AllPhonetics dict
            {
                int i = 0; // track relative index (2 things can be at the same index but in different sections)
                if (AllPhonetics[section_name].Count <= 1) continue; // don't include sections which have no content (1 because they will have a section header in them always)
                foreach (PhoneticsObject obj in AllPhonetics[section_name]) // for each list in the section of the dict
                {
                    if (obj.Section.Name.ToLower() == "none") { i += 1; continue; }
                    PhoneticsPointers.Add(new(section_name, i)); // set pointer
                    Phonetics_TypeComboBox.Items.Add(obj.NameWithSectionFormat); // set UI name
                    i += 1;
                }
            }

            // locate the true index (SelectedIndex) of the default phonetic
            int defualt_selection_index = 0; // track defualt_selection_index for indexing
            foreach (PhoneticsPointer pointer in PhoneticsPointers)
            {
                if (AllPhonetics[pointer.Section][pointer.Index].Name == PhoneticsDefault.TypeComboBox_DefaultName)
                {
                    // defualt_selection_index will stay the same
                    break;
                }
                defualt_selection_index += 1;
            }

            // attempt to keep selection and set to default if cant since a bunch of stuff just got loaded
            if ((curr_selected_index < Phonetics_TypeComboBox.Items.Count) // index must be in the range of items
                && (curr_selected_index >= 0)) // index must be greater than zero
            {
                Phonetics_TypeComboBox.SelectedIndex = curr_selected_index;
            } else if (defualt_selection_index < Phonetics_TypeComboBox.Items.Count) // as long as the default is in the length
            {
                Phonetics_TypeComboBox.SelectedIndex = defualt_selection_index;
            } else
            {
                Phonetics_TypeComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Changes the output _Text box
        /// </summary>
        private void SetOutput()
        {
            // clear/do nothing if...
            if (string.IsNullOrEmpty(Phonetics_InputTextBox.Text) // input _Text box is empty
                || (CurrentPointers.Count <= 0)) // no current phonetics
            {
                Phonetics_OuputTextBox.Text = "";
                return;
            }

            var r = new Random();
            string output_string = "";
            // for each letter in the input, find a random Phonetic set and get the corresponding Phonetic
            for (int i = 0; i < Phonetics_InputTextBox.Text.Length; i++)
            {
                // get random
                int random_phonetic_index = r.Next(0, CurrentPointers.Count); // will generate the same number with a range of 0
                PhoneticsPointer random_phonetic_pointer = CurrentPointers[random_phonetic_index];
                PhoneticsObject random_phonetic_object = AllPhonetics[random_phonetic_pointer.Section][random_phonetic_pointer.Index];

                // get char
                char curr_char = Phonetics_InputTextBox.Text[i];
                char curr_char_lower = Char.ToLower(curr_char);
                char curr_char_upper = Char.ToUpper(curr_char);

                // add to output
                if (curr_char == ' ') output_string += ""; // space
                else if (Char.IsDigit(curr_char)) output_string += $"{PhoneticsDefault.NumberPrepension}{curr_char}"; // digits
                else if (!Char.IsLetter(curr_char)) output_string += curr_char; // unknown
                else if (curr_char == curr_char_upper) output_string += $"{curr_char_upper}  -  {random_phonetic_object[curr_char_lower].ToUpper()}"; // uppercase
                else if (curr_char == curr_char_lower) output_string += $"{curr_char_lower}  -  {random_phonetic_object[curr_char_lower].ToLower()}"; // lowercase
                output_string += "\n"; // always new line
            }
            
            // set output
            Phonetics_OuputTextBox.Text = output_string;
        }

        /// <summary>
        /// Updates the current phonetics whenever the current selection has changed
        /// </summary>
        private void UpdateCurrentPhonetics()
        {
            // wipe data
            CurrentPointers.Clear();

            // get current pointer
            if (Phonetics_TypeComboBox.SelectedIndex < 0) return; // don't update if the selection isn't valid
            PhoneticsPointer curr_pointer = PhoneticsPointers[Phonetics_TypeComboBox.SelectedIndex];

            // if the current Phonetic is a pointer
            if (AllPhonetics[curr_pointer.Section][curr_pointer.Index].IsSection == true)
            {
                foreach (PhoneticsPointer pointer in PhoneticsPointers) // for all phonetics pointers
                {
                    if ((curr_pointer.Section == pointer.Section) // same section
                        && (!AllPhonetics[pointer.Section][pointer.Index].IsSection)) // not a section header
                    {
                        CurrentPointers.Add(pointer); // add pointer to current phonetics
                    }
                }
            } else // the current Phonetic isn't a pointer
            {
                CurrentPointers.Add(curr_pointer);
            }

            // display warning message if applicable
            if (PhoneticsDefault.DisplaySetWarning == false) return; // don't display
            int section_index_of_curr_pointer = PhoneticsObject.Sections.FindIndex(s => (s.Name == curr_pointer.Section)); // find index
            if (PhoneticsObject.Sections[section_index_of_curr_pointer].HasWarning == true)
            {
                MessageBox.Show($"{StringFormat.Name(curr_pointer.Section)}: {PhoneticsObject.Sections[section_index_of_curr_pointer].Warning}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Checks the validity and injects a phonetics set
        /// </summary>
        /// <param name="do_modify"> Whether or not to derive from the selected existing phonetic aka modify instead of create </param>
        private async void CreatePhonetics(bool do_modify=false)
        {
            // temp disable the create/modify button until the window is closed
            Phonetics_CreateButton.IsEnabled = false;
            Phonetics_ModifyButton.IsEnabled = false;
            Phonetics_DeleteButton.IsEnabled = false;

            // if modifying get the set to derive from
            PhoneticsObject? phonetic_parent = null;
            if (do_modify == true)
            {
                // get current pointer/obj
                if (Phonetics_TypeComboBox.SelectedIndex < 0) return; // don't update if the selection isn't valid
                PhoneticsPointer curr_pointer = PhoneticsPointers[Phonetics_TypeComboBox.SelectedIndex];
                phonetic_parent = AllPhonetics[curr_pointer.Section][curr_pointer.Index];

                // check if it can't be edited
                if (!IsEditable(phonetic_parent))
                {
                    Phonetics_CreateButton.IsEnabled = true;
                    Phonetics_ModifyButton.IsEnabled = true;
                    return;
                }
            }

            // creates a new creator window, shows it and waits for result
            PhoneticsCreator creator_window = new PhoneticsCreator(phonetic_parent);
            creator_window.Owner = this; // sets the the owner of the creator window to the current window
            creator_window.Loaded += (s, e) => { creator_window.Left += 100; }; // shifts window 100 to the right
            PhoneticsCreatorResult result = await creator_window.Show();
            
            // based on result of creator window
            if (result == PhoneticsCreatorResult.Success)
            { // try to write the phonetic
                bool do_write_file = true;
                if (do_modify == true) // if modifying
                {
                    if (phonetic_parent.Name != creator_window.Phonetic.Name) // if the name was changed, erase the old file
                    {
                        phonetic_parent.Remove(); // will auto update when the file is removed due to the LoadPhonetics() call below
                    }
                } else // if not modifying
                {
                    if (File.Exists(Path.Combine(creator_window.Phonetic.ListPath.ToArray()))) // checks if the file already exists, aka overwriting
                    {
                        MessageBox.Show("A phonetic set with this name already exists! If you would like to overwrite a set, please use \"Modify Set\"", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        do_write_file = false; // stop from writing 
                    }
                }

                // check if the file can be written based on modification and overwritability
                if (do_write_file == true)
                {
                    // try to save phonetic
                    try
                    {
                        creator_window.Phonetic.Store();
                    } catch (IOException)
                    {
                        MessageBox.Show("The phonetics set could not be written\nDid you name the set with invalid characters like '?'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    } catch (Exception err)
                    {
                        MessageBox.Show($"An unknown error occuring when attempting to write the phonetics set: \n{err.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    // display success
                    MessageBox.Show($"Successfully created phonetic set, \"{creator_window.Phonetic.Name}\"", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    // refresh if successful
                    PhoneticsDefault.DisplaySetWarning = false; // doesn't display warning when switching
                    LoadPhonetics();
                    // find the phonetic with a matching name as yours and set the selection to it
                    int i = 0; // track defualt_selection_index so the selected index can be set
                    foreach (PhoneticsPointer pointer in PhoneticsPointers)
                    {
                        if (AllPhonetics[pointer.Section][pointer.Index].Name == creator_window.Phonetic.Name)
                        {
                            
                            Phonetics_TypeComboBox.SelectedIndex = i;
                            break;
                        }
                        i += 1;
                    }
                    PhoneticsDefault.DisplaySetWarning = true; // reset
                }
            } // else do nothing

            // allow to create new again
            Phonetics_CreateButton.IsEnabled = true;
            Phonetics_ModifyButton.IsEnabled = true;
            Phonetics_DeleteButton.IsEnabled = true;
        }

        /// <summary>
        /// Checks if the the set can be edited
        /// </summary>
        /// <returns></returns>
        private bool IsEditable(PhoneticsObject phonetic)
        {
            // check if it's a section
            if (phonetic.IsSection)
            {
                MessageBox.Show("You cannot change this set because it is a section header!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // check if the author of the current set is NOT the same as the current user
            if (phonetic.CurrentUserIsAuthor == false)
            {
                MessageBox.Show("You cannot change this set because you are not the author of it!", "Sorry", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }
            return true; // is editable
        }

        /// <summary>
        /// Checks the author and deleted phonetics
        /// </summary>
        private void DeletePhonetics()
        {
            // get current pointer/obj
            if (Phonetics_TypeComboBox.SelectedIndex < 0) return; // don't update if the selection isn't valid
            PhoneticsPointer curr_pointer = PhoneticsPointers[Phonetics_TypeComboBox.SelectedIndex];
            PhoneticsObject curr_object = AllPhonetics[curr_pointer.Section][curr_pointer.Index];

            // check if it can't be edited
            if (!IsEditable(curr_object)) return;

            // confirm the user wants to delete
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this set?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                AllPhonetics[curr_pointer.Section][curr_pointer.Index].Remove(); // remove file
                LoadPhonetics(); // reload the phonetics
            } // do nothing if no
        }

        // --- EVENT METHODS ---

        private void Phonetics_InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetOutput();
        }

        private void Phonetics_TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // check if the current selection is a section or not and set the change button to on or off
            if (Phonetics_TypeComboBox.SelectedIndex < 0) return; // don't update if the selection isn't valid
            PhoneticsPointer curr_pointer = PhoneticsPointers[Phonetics_TypeComboBox.SelectedIndex];
            if ((AllPhonetics[curr_pointer.Section][curr_pointer.Index].IsSection == true) // is section
                && AllPhonetics[curr_pointer.Section].Count > 2) // has more than 1 item (2 because the header is included)
            {
                Phonetics_ChangeButton.IsEnabled = true;
            } else // not section
            {
                Phonetics_ChangeButton.IsEnabled = false;
            }

            UpdateCurrentPhonetics();
            SetOutput();
        }

        private void Phonetics_ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            SetOutput();
        }

        private void Phonetics_CreateButton_Click(Object sender, RoutedEventArgs e)
        {
            CreatePhonetics();
        }

        private void Phonetics_ModifyButton_Click(object sender, RoutedEventArgs args)
        {
            CreatePhonetics(do_modify:true);
        }

        private void Phonetics_DeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            DeletePhonetics();
        }
    }
}
