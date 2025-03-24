using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace OIT_HelpDesk_Assistant_v2.Phonetics
{
    /// <summary>
    /// Phonetics list class. Creates an immutable list of values that must correspond to the alphabet
    /// </summary>
    public class PhoneticsList
    {
        // --- VARIABLES ---

        /// <summary>
        /// Default alphabet values as a string
        /// </summary>
        [JsonIgnore]
        public const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// The size requirement for PhoneticsLists
        /// </summary>
        [JsonIgnore]
        private int FixedSize { get => Alphabet.Count(); }

        /// <summary>
        /// The actual wordlist, internal
        /// </summary>
        [JsonProperty("word_list")]
        private List<string> _WordList { get; set; } = new();

        /// <summary>
        /// Gets a value from the list by index
        /// </summary>
        /// <param name="index"> The index of the list to access </param>
        /// <returns> The word associated with the index value </returns>
        [JsonIgnore]
        public string this[int index]
        {
            get => _WordList[index];
        }

        /// <summary>
        /// Gets a value from the list by letter
        /// </summary>
        /// <param name="letter"> The letter of the list to access </param>
        /// <returns> The word associated with the letter value </returns>
        /// <exception cref="ArgumentException"> Throws exception when the letter is not in the alphabet </exception>
        [JsonIgnore]
        public string this[char letter]
        {
            get { 
                foreach (string word in _WordList) // checks the letter is a possible word in the list
                {
                    if (Char.ToLower(word[0]) == Char.ToLower(letter)) return word;
                }
                throw new ArgumentException($"The provided letter is not alphabetic: \"{letter}\"");
            }
            set
            {
                if (_WordList.Count <= 0) SetDefaultWordList(); // if the wordlist is empty, set it to default
                for (int i = 0; i < Alphabet.Length; i++)
                {
                    if (Char.ToLower(Alphabet[i]) == Char.ToLower(letter)) // checks the letter is in the Alphabet
                    {
                        _WordList[i] = value; // set the value
                        return; // breaks to make sure no error is thrown
                    }
                }
                throw new ArgumentException($"Could not set Phonetic, the provided letter is not alphabetic: \"{letter}\"");
            }
        }

        /// <summary>
        /// Public immutable wordlist
        /// </summary>
        [JsonIgnore]
        public ImmutableList<string> WordList
        {
            get => _WordList.ToImmutableList();
        }

        /// <summary>
        /// The length of the PhoneticsList
        /// </summary>
        [JsonIgnore]
        public int Count => _WordList.Count;

        /// <summary>
        /// If the PhoneticsList is readonly
        /// </summary>
        [JsonIgnore]
        public bool IsReadOnly => true;

        // --- CONSTRUCTOR ---

        /// <summary>
        /// PhoneticsList class, a list which much be length 26 where each value must start with the next letter in the alphabet
        /// </summary>
        /// <param name="list"> Value must be a list of length 26 or values dont start with the right letter </param>
        /// <exception cref="ArgumentException"> Thrown when the list is not a length of 26 </exception>
        public PhoneticsList(List<string> list)
        {
            SetWordList(list);
        }

        /// <summary>
        /// Default set for the PhoneticsList class. Should not be used.
        /// </summary>
        public PhoneticsList() { }

        // --- METHODS ---

        /// <summary>
        /// Logic for (mainly) the constructor to set the word list according to the 26 character requirement. This can also be used in the derived class to set the wordlist.
        /// </summary>
        /// <param name="word_list"> The list of words </param>
        /// <exception cref="ArgumentException"> Thrown when the list is not a length of 26 </exception>
        protected void SetWordList(List<string> word_list)
        {
            // clear first
            _WordList.Clear();

            // check length
            if (word_list.Count != FixedSize) throw new ArgumentException($"PhoneticsList can only have a length of {FixedSize}");
            // check alphabetic values
            int i = 0;
            foreach (string word in word_list)
            {
                if (Char.ToLower(word[0]) != Alphabet[i]) throw new ArgumentException($"PhoneticsList invalid, alphabetics incorrect at word \"{word}\"");
                i += 1;
            }
            // else set the values
            _WordList = word_list;
        }

        /// <summary>
        /// Logic for (mainly) the constructor to set a default word list. The can also be used in the derived class to set the wordlist.
        /// </summary>
        protected void SetDefaultWordList()
        {
            // clear first
            _WordList.Clear();

            // convert alphabet to list of strings
            List<string> letters = new();
            foreach (char ele in Alphabet)
            {
                letters.Add($"{ele}_default");
            }
            _WordList = letters;
        }

        // --- OVERRIDES ---

        public override string ToString() => StringFormat.Collection(_WordList) ?? "";
    }
}
