using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OIT_HelpDesk_Assistant_v2.SearchPage
{
    internal class CopyTextGrid : Grid
    {
        // --- VARIABLES ---

        public string CopyText { get; private set; } = string.Empty;

        // --- CONSTRUCTOR ---

        public CopyTextGrid(string text) : base()
        {
            CopyText = text;
        }
    }
}
