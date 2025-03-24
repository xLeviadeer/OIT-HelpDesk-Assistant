using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIT_HelpDesk_Assistant_v2.Phonetics
{
    /// <summary>
    /// Section class for easy checking of whether or not a section has a warning and getting it's name
    /// </summary>
    public class PhoneticsSection
    {
        // --- VARIABLES ---

        /// <summary>
        /// Name of the section
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; init; }

        /// <summary>
        /// The warning of the section, can be null
        /// </summary>
        [JsonProperty("warning")]
        public string? Warning { get; init; }

        /// <summary>
        /// If the section has a warning, boolean
        /// </summary>
        [JsonIgnore]
        public bool HasWarning { get => (Warning != null); }

        // --- CONSTRUCTOR ---

        public PhoneticsSection(string name, string warning)
        {
            Name = name;
            Warning = warning;
        }

        public PhoneticsSection()
        {
            Name = "default";
        }

        // --- CASTING ----

        // casting to string
        public static implicit operator string(PhoneticsSection section)
        {
            return section.Name;
        }

        // --- OVERRIDES ---

        public override string ToString()
        {
            return StringFormat.ToString(this);
        }
    }
}
