using OIT_HelpDesk_Assistant_v2.SearchPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OIT_HelpDesk_Assistant_v2.Lib
{
    internal class Utility
    {
        /// <summary>
        /// Attempts to copy text to the clipboard
        /// </summary>
        /// <param name="text"> The text to copy </param>
        /// <param name="amount_of_attempts"> The amount of times it can retry copying. Default is 5 </param>
        /// <returns> Boolean, true if complete </returns>
        public static bool CopyToClipboard(string text, int amount_of_attempts=5)
        {
            for (int i = 0; i < amount_of_attempts; i++)
            {
                try
                {
                    Clipboard.SetText(text);
                    return true; // completed successfully
                } catch (System.Runtime.InteropServices.COMException)
                {
                    Thread.Sleep(100); // 100ms before retrying again
                }
            }
            return false; // didn't complete
        }

        /// <summary>
        /// Attempts to copy text to the clipboard and displays fail or completed tooltip
        /// </summary>
        /// <remarks>
        /// This function is only suitable for WPF since it uses error handling for WPF
        /// </remarks>
        /// <param name="text"> The text to copy </param>
        /// <returns> Boolean, true if complete </returns>
        public static void CopyToClipboardWPF(UIElement placement_target, string text)
        {
            if (CopyToClipboard(text, 3)) {
                DisplayableDictionary.ShortToolTip(placement_target, "Copied!", 1);
            } else
            {
                DisplayableDictionary.ShortToolTip(placement_target, "Failed to Copy! try again", 12);
            }
            
        }
    }
}
