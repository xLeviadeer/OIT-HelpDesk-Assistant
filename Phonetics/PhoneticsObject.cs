using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace OIT_HelpDesk_Assistant_v2.Phonetics
{
    /// <summary>
    /// Exception class for when an object cannot be deleted
    /// </summary>
    public class CannotDeleteException : Exception
    {
        public CannotDeleteException() { }

        public CannotDeleteException(string message) : base(message) { }

        public CannotDeleteException(string message,  Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Phonetics object class extends a phonetics list and must also include a name and section
    /// </summary>
    public class PhoneticsObject : PhoneticsList
    {
        // --- VARIABLES ---

        /// <summary>
        /// The max length a name can be
        /// </summary>
        private const int NameMaxLength = 30;

        /// <summary>
        /// list of sections and their warnings
        /// </summary>
        public static List<PhoneticsSection> Sections { get; private set; } = new();

        /// <summary>
        /// Internal Sections index of the section
        /// </summary>
        [JsonProperty("section")]
        public int SectionInt { get; set; } = 0;

        /// <summary>
        /// Public section callable
        /// </summary>
        [JsonIgnore]
        public PhoneticsSection Section
        {
            get => Sections[SectionInt];
        }

        /// <summary>
        /// Get the section as a string
        /// </summary>
        [JsonIgnore]
        public string SectionName { get => Section.Name; }

        /// <summary>
        /// String to precurse section heads
        /// </summary>
        [JsonIgnore]
        private const string SectionPrecursor = " --- ";

        /// <summary>
        /// String to postcurse section heads
        /// </summary>
        [JsonIgnore]
        private const string SectionPostcursor = " Section --- ";

        /// <summary>
        /// Value to determine if the object is a section head
        /// </summary>
        [JsonIgnore]
        public bool IsSection { get; private set; } = false;

        /// <summary>
        /// Private, raw name of the object
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Name of the object, including the section naming scheme
        /// </summary>
        [JsonIgnore]
        public string NameWithSectionFormat
        {
            get => (IsSection) ? $"{SectionPrecursor}{StringFormat.Name(Section.Name)}{SectionPostcursor}" : Name;
            private set { Name = value; }
        }

        [JsonIgnore]
        public static List<string> Path { get => new() { "Phonetics" }; }

        /// <summary>
        /// Default path to the directory of phonetics lists
        /// </summary>
        [JsonIgnore]
        public static List<string> ListsPath { get => Path.Append("Lists").ToList(); }

        /// <summary>
        /// Default path to this specific phonetics list
        /// </summary>
        [JsonIgnore]
        public List<string> ListPath { get => ListsPath.Append($"{Name}.json").ToList(); }

        /// <summary>
        /// Pointer value for locating after sorting into the ComboBox.
        /// Pointer value can be null and must be tested for
        /// </summary>
        [JsonIgnore] // value is never saved as sorting is relative to the session
        public int? Pointer { get; set; } = null;

        /// <summary>
        /// Author of this object
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; private set; }

        /// <summary>
        /// Checks if the current user is the author of this object
        /// </summary>
        [JsonIgnore]
        public bool CurrentUserIsAuthor { get => (Environment.UserName == Author); }

        // --- Constructor ---

        /// <summary>
        /// Creates a new phonetics object without trying to load
        /// </summary>
        /// <param name="name"> The name of the object </param>
        /// <param name="section"> The section of the object, must be a Sections enum value </param>
        /// <param name="list"> The list of words in this object, this is optional (will set to default if left untouched) </param>
        public PhoneticsObject(string name, int section, List<string>? list=null)
        {
            // Create new
            ValidateAndSet(name, section, Environment.UserName, list); // it's assumed the current user is the author if the object doesn't already exist
        }

        /// <summary>
        /// Strictly tries to load a PhoneticsObject rather than creating a new one. Will kill the program if it fails to find Phonetics
        /// </summary>
        /// <param name="name"> The name of the object to load </param>
        public PhoneticsObject(string name)
        {
            Name = name; // name must always be set before loading
            Load(); // validates in load
        }

        /// <summary>
        /// Creates a new phonetics object SECTION HEADER
        /// </summary>
        /// <remarks>
        /// This creates a section header with no details except the section that it is
        /// </remarks>
        /// <param name="section"> The section this is a head for </param>
        public PhoneticsObject(int section) : base()
        {
            // sections use the default wordlist set in base()
            Name = "default"; 
            IsSection = true;
            Author = "System";

            // validate only section
            ValidateAndSet(section: section);
        }

        /// <summary>
        /// Default set for the PhoneticsObject class with a blank author or not
        /// </summary>
        /// <remarks>
        /// Sets values to their defaults. None, default and <letter>-default
        /// </remarks>
        /// <paramref name="blank_authored_set"/> Determines if the set will be created with an author or not. True = with author </param>
        public PhoneticsObject(bool blank_authored_set=false)
        {
            Name = "default";
            SectionInt = 0; // none
            Author = (blank_authored_set == true) ? Environment.UserName : "System";
            // nothing to validate
        }

        /// <summary>
        /// Default set for the PhoneticsObject class. should not use. JSON Serialization only
        /// </summary>
        /// <remarks>
        /// Sets values to their defaults. None, default and <letter>-default
        /// </remarks>
        public PhoneticsObject()
        {
            Name = "default";
            SectionInt = 0; // none
            Author = "System";
            // nothing to validate
        }

        /// <summary>
        /// Loads the sections
        /// </summary>
        public static void LoadSections()
        {
            // clear sections
            Sections.Clear();

            // read attempt
            List<PhoneticsSection> loaded_sections;
            List<string> path = Path.Append("Sections.json").ToList();
            try
            {
                loaded_sections = Json.Read<List<PhoneticsSection>>(path);
            }
            catch (FileNotFoundException)
            { // if file not found then critical error
                MessageBox.Show($"{StringFormat.Collection(path)} could not be read, click OK to exit the program");
                Application.Current.Shutdown();
                return;
            }
            catch (JsonReaderException)
            {
                MessageBox.Show($"{StringFormat.Collection(path)} is not a correctly formatted JSON file, click OK to exit the program");
                Application.Current.Shutdown();
                return;
            }
            catch (FileEmptyException)
            {
                MessageBox.Show($"{StringFormat.Collection(path)} has no items, click OK to exit the program");
                Application.Current.Shutdown();
                return;
            }

            // add hard-coded sections to the sections variable
            Sections.Add(new PhoneticsSection() { Name = "None", Warning = null });
            Sections.Add(new PhoneticsSection() { Name = "Custom", Warning = "This section is made by other users and should be used carefully!" });
            // add dynamic sections to the sections variable
            foreach (PhoneticsSection section in loaded_sections)
            {
                // validate section name isn't null
                if (section.Name == null)
                {
                    MessageBox.Show($"Section names cannot be null (name: {section.Name ?? "null"} warning: {section.Warning ?? "null"})");
                    Application.Current.Shutdown();
                    return;
                } 
                // unique name check
                foreach (PhoneticsSection section_check in Sections)
                {
                    if (section.Name == section_check.Name)
                    {
                        MessageBox.Show($"Two sections cannot have the same name: {section.Name}");
                        Application.Current.Shutdown();
                        return;
                    }
                }

                // add section
                Sections.Add(section);
            }
        }

        // --- METHODS ---

        /// <summary>
        /// Loads the current object
        /// </summary>
        private void Load()
        {
            // read attempt
            PhoneticsObject loaded_object;
            try
            {
                loaded_object = Json.Read<PhoneticsObject>(ListPath);
            }
            catch (FileNotFoundException)
            { // if file not found then critical error
                MessageBox.Show($"Phonetics List could not be read, click OK to exit the program ({NameWithSectionFormat})");
                Application.Current.Shutdown();
                return;
            } catch (JsonReaderException)
            {
                MessageBox.Show($"{Name} is not a correctly formatted JSON file, click OK to exit the program");
                Application.Current.Shutdown();
                return;
            } catch (FileEmptyException)
            {
                MessageBox.Show($"{Name} has no items, click OK to exit the program");
                Application.Current.Shutdown();
                return;
            }

            // validate all
            ValidateAndSet(loaded_object.Name, loaded_object.SectionInt, loaded_object.Author, loaded_object.WordList.ToList());
        }

        /// <summary>
        /// Stores the current object
        /// </summary>
        public void Store()
        {
            Json.Write(ListPath, this);
        }

        /// <summary>
        /// Deletes this object
        /// </summary>
        /// <exception cref="CannotDeleteException"> Thrown if the class is a section </exception>
        public void Remove()
        {
            if (IsSection == true) throw new CannotDeleteException($"{Name}: this is a section; it cannot be deleted");
            Json.Delete(ListPath);
        }

        /// <summary>
        /// Validates that all values are to-spec and sets onces that are to their corresponding class properties
        /// </summary>
        /// <param name="name"> Prospective name </param>
        /// <param name="section"> Prospective section </param>
        /// <param name="author"> Prospective author </param>
        /// <param name="word_list"> Prospective word list </param>
        private void ValidateAndSet(string? name=null, int? section=null, string? author=null, List<string>? word_list=null)
        {
            // name
            if (name != null)
            {
                if (name.Length > NameMaxLength) // length check
                {
                    Name = name[..NameMaxLength]; // slice down
                }
                else
                {
                    Name = name; // name should be already set but not with checks
                }
            }

            // section
            if (section != null)
            {
                ((Action)(() => // anonymous, instantly instantiated function (AIIF) block for returning when errors are found without breaking the whole function
                {
                    if (section >= Sections.Count) return; // key out of range (above)
                    if (section < 0) return; // key out of range (below)
                    SectionInt = (int)section;
                }))();
            }

            // author
            if (author != null)
            {
                Author = author;
            }

            // word list
            if (word_list != null)
            {
                try
                {
                    SetWordList(word_list);
                }
                catch (ArgumentException) // too long or short
                {
                    SetDefaultWordList();
                }
            }
        }

        // --- OVERRIDES ---

        public override string ToString()
        {
            return StringFormat.ToString(this);
        }

    }
}
