using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIT_HelpDesk_Assistant_v2.SearchPage
{
    /// <summary>
    /// Class contains a name and it's aliases for searching and displaying through a displayable dictionary
    /// </summary>
    public class SearchValue
    {
        // --- VARIABLES ---

        /// <summary>
        /// The display name of the item
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; } = "";

        /// <summary>
        /// A list of aliases that can be searched to find this item
        /// </summary>
        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; } = [""];

        // --- CONSTRUCTOR ---

        public SearchValue() { }
    }
}
