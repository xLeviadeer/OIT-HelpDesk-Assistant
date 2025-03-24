using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Text.Json.Serialization;
using static OIT_HelpDesk_Assistant_v2.SearchPageGrid;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Reflection;
using System.Windows.Threading;
using System.Diagnostics.Metrics;
using System.Buffers;
using OIT_HelpDesk_Assistant_v2.Lib;

namespace OIT_HelpDesk_Assistant_v2.SearchPage
{
    /// <summary>
    /// Displayable dictionary; regular dictionary but with ability to output display boxes of information as a grid
    /// </summary>
    public sealed class DisplayableDictionary : Dictionary<string, SearchValue>
    {
        // --- VARIABLES ---

        /// <summary>
        /// Public color selection helper
        /// </summary>
        public enum Colors
        {
            GrayVeryDark,
            GrayDark,
            GrayLight,
            GrayVeryLight
        }

        [JsonIgnore]
        private readonly Dictionary<Colors, Color> ColorLink = new(); // initialized in constructor

        [JsonIgnore]
        public DisplayableDictionaryDefaults Defaults = new();
        public class DisplayableDictionaryDefaults
        {
            /// <summary>
            /// Default width of the grid total is auto
            /// </summary>
            public double Width = Double.NaN;

            /// <summary>
            /// Default height of the grid total is auto
            /// </summary>
            public double Height = Double.NaN;

            /// <summary>
            /// Darker gray very
            /// </summary>
            public Color GrayVeryDark = Color.FromRgb(0xBB, 0xBB, 0xBB);

            /// <summary>
            /// Darker gray
            /// </summary>
            public Color GrayDark = Color.FromRgb(0xCC, 0xCC, 0xCC);

            /// <summary>
            /// Lighter gray
            /// </summary>
            public Color GrayLight = Color.FromRgb(0xDD, 0xDD, 0xDD);

            /// <summary>
            /// Lighter gray very
            /// </summary>
            public Color GrayVeryLight = Color.FromRgb(0xEE, 0xEE, 0xEE);

            /// <summary>
            /// The click to copy tooltip text
            /// </summary>
            public string ClickToCopyToolTip = "[Left Click] Click to copy";
        }

        /// <summary>
        /// The amount of color that is shifted when clicking and hovering the mouse
        /// </summary>
        public static (int Red, int Blue, int Green) DefaultColorShift = (0, 30, 15);

        /// <summary>
        /// The amount of lightness shift downwards when clicking and hovering the mouse
        /// </summary>
        public static int DefaultDownShift = 20;

        // --- CONSTRUCTOR ---

        public DisplayableDictionary()
        {
            // link colors
            ColorLink.Add(Colors.GrayDark, Defaults.GrayDark);
            ColorLink.Add(Colors.GrayLight, Defaults.GrayLight);
            ColorLink.Add(Colors.GrayVeryDark, Defaults.GrayVeryDark);
            ColorLink.Add(Colors.GrayVeryLight, Defaults.GrayVeryLight);
        }

        // --- EVENT METHODS ---

        /// <summary>
        /// Copies the value to the users clipboard
        /// </summary>
        private void DisplayableDictionary_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            // interpret sender as grid
            CopyTextGrid? grid = sender as CopyTextGrid;
            if (grid == null) return; // do nothing

            // short tooltip and color
            ShortToolTip(grid, "Copied!", 1);
            ShortColorChange(grid, 1);

            // set to clipboard
            Utility.CopyToClipboardWPF(grid, grid.CopyText);
        }

        /// <summary>
        /// Checks to make the sure the change doesn't exceed 255
        /// </summary>
        /// <param name="direction"> Positive or negative </param>
        /// <param name="num1"> The initial color </param>
        /// <param name="num2"> The change in color </param>
        /// <returns> A color int </returns>
        private static int CheckColorChange(bool direction, int num1, int num2)
        {
            if (direction == true)
            {
                int num = (num1 + num2) - DefaultDownShift;
                return (num <= 255) ? num : 255;
            }
            else
            {
                int num = (num1 - num2) + DefaultDownShift;
                return (num >= 0) ? num : 255;
            }
        }

        /// <summary>
        /// Shifts the background color of the grid either positively or negatively
        /// </summary>
        /// <param name="direction"> The direction of shift. Positive is brighter </param>
        public static Color ShiftBackgroundColor(Color color, bool direction)
        {
            // get the color and change it
            if (direction == true)
            {
                color = Color.FromArgb(
                color.A,
                (byte)CheckColorChange(true, color.R, DefaultColorShift.Red),
                (byte)CheckColorChange(true, color.G, DefaultColorShift.Green),
                (byte)CheckColorChange(true, color.B, DefaultColorShift.Blue));
            } else
            {
                color = Color.FromArgb(
                color.A,
                (byte)CheckColorChange(false, color.R, DefaultColorShift.Red),
                (byte)CheckColorChange(false, color.G, DefaultColorShift.Green),
                (byte)CheckColorChange(false, color.B, DefaultColorShift.Blue));
            }

            return color;
        }

        /// <summary>
        /// Handles color change for mouse hovering on the grid
        /// </summary>
        private void DisplayableDictionary_MouseEnter(object sender, MouseEventArgs e)
        {
            // interpret sender as grid
            Grid? grid = sender as Grid;
            if (grid == null) return; // do nothing

            // get the background brush
            SolidColorBrush? background_brush = grid.Background as SolidColorBrush;
            if (background_brush == null) return; // do nothing

            // change color
            Color color = ShiftBackgroundColor(background_brush.Color, true);

            // apply color change
            grid.Background = new SolidColorBrush(color);
        }

        /// <summary>
        /// Handles color change for mouse hovering on the grid
        /// </summary>
        private void DisplayableDictionary_MouseLeave(object sender, MouseEventArgs e)
        {
            // interpret sender as grid
            Grid? grid = sender as Grid;
            if (grid == null) return; // do nothing

            // get the background brush
            SolidColorBrush? background_brush = grid.Background as SolidColorBrush;
            if (background_brush == null) return; // do nothing

            // change color
            Color color = ShiftBackgroundColor(background_brush.Color, false);

            // apply color change
            grid.Background = new SolidColorBrush(color);
        }

        // --- METHODS ---

        /// <summary>
        /// Changes the color for the amount of seconds
        /// </summary>
        /// <param name="grid"> The grid which is changing color </param>
        /// <param name="time_in_seconds"> The time the color displays for </param>
        private static void ShortColorChange(Grid grid, int time_in_seconds)
        {
            // get the background brush
            SolidColorBrush? background_brush = grid.Background as SolidColorBrush;
            if (background_brush == null) return; // do nothing
            // change color
            int red = CheckColorChange(true, background_brush.Color.R, DefaultColorShift.Red);
            int green = CheckColorChange(true, background_brush.Color.G, DefaultColorShift.Green);
            int blue = CheckColorChange(true, background_brush.Color.B, DefaultColorShift.Blue);
            int red_change = red - background_brush.Color.R;
            int green_change = green - background_brush.Color.G;
            int blue_change = blue - background_brush.Color.B;
            Color color = Color.FromArgb(
                background_brush.Color.A,
                (byte)red,
                (byte)green,
                (byte)blue);
            // apply color change
            grid.Background = new SolidColorBrush(color);

            // timer for changing tooltip back
            DispatcherTimer color_timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(time_in_seconds) // amount of time
            };
            color_timer.Tick += (s, args) => // event handler anonymous function (completed)
            {
                // get the background brush
                SolidColorBrush? background_brush = grid.Background as SolidColorBrush;
                if (background_brush == null) return; // do nothing
                // change color
                color = Color.FromArgb(
                    background_brush.Color.A,
                    (byte)(background_brush.Color.R - red_change),
                    (byte)(background_brush.Color.G - green_change),
                    (byte)(background_brush.Color.B - blue_change));
                // apply color change
                grid.Background = new SolidColorBrush(color);

                color_timer.Stop(); // stop the timer
            };
            color_timer.Start(); // start the timer
        }

        /// <summary>
        /// Creates a tooltip that shows for the amount of seconds
        /// </summary>
        /// <param name="placement_target"> What object this tooltip comes from </param>
        /// <param name="content"> What the tooltip says </param>
        /// <param name="time_in_seconds"> The time the tooltip displays for </param>
        public static void ShortToolTip(UIElement placement_target, string content, int time_in_seconds)
        {
            // create a new tooltip
            ToolTip tooltip = new ToolTip
            {
                Content = content,
                PlacementTarget = placement_target, // set the target for the tooltip (this is how the tooltip is linked to the grid)
                IsOpen = true // show the tooltip immediately
            };

            // timer for changing tooltip back
            DispatcherTimer tooltip_timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(time_in_seconds) // amount of time
            };
            tooltip_timer.Tick += (s, args) => // event handler anonymous function (completed)
            {
                tooltip.IsOpen = false; // hide the tooltip
                tooltip_timer.Stop(); // stop the timer
            };
            tooltip_timer.Start(); // start the timer
        }

        /// <summary>
        /// Gets a grid containing the data associated with the index
        /// </summary>
        /// <param name="index"> The index to get the grid for </param>
        /// <param name="margin"> The double amount of margin for the items in this grid </param>
        /// <returns> A grid that displays the key-value information </returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when index is out of bounds </exception>
        public Grid GetGrid(int index, Colors color, double margin)
        {
            if ((0 > index) || (index > Count)) throw new ArgumentOutOfRangeException($"Index was not within the range of the dictionary: \"{index}\"");

            return GetGrid(this.Keys.ToList()[index], color, margin); // runs other function on the key
        }

        /// <summary>
        /// Gets a grid containing the data associated with the key
        /// </summary>
        /// <param name="key"> The string key to get the grid for </param>
        /// <param name="color"> Tells what color to display. Displays a dark grey if left empty </param>
        /// <param name="margin"> The double amount of margin for the items in this grid. If left empty, 0 margin </param>
        /// <returns> A grid that displays the key-value information </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the key is not in the dictionary </exception>
        public Grid GetGrid(string key, Colors color=Colors.GrayDark, double margin=0)
        {
            // check if doesn't contain key
            if (!this.ContainsKey(key)) throw new KeyNotFoundException($"Key could not be found in DisplayableDictionary: \"{key}\"");

            // create new grid
            var return_grid = new CopyTextGrid(this[key].Value); // puts the value as the copy text
            return_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) }); // relative 1
            return_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // relative 2

            // click to copy physics
            return_grid.MouseLeftButtonDown += DisplayableDictionary_MouseLeftDown;
            return_grid.MouseEnter += DisplayableDictionary_MouseEnter;
            return_grid.MouseLeave += DisplayableDictionary_MouseLeave;
            return_grid.ToolTip = new ToolTip() { Content = Defaults.ClickToCopyToolTip };

            // create text boxes
            var key_text = new TextBlock();
            var value_text = new TextBlock();
            key_text.Text = key;
            value_text.Text = this[key].Value;
            // positioning
            key_text.Margin = new Thickness(margin);
            value_text.Margin = new Thickness(margin);
            key_text.VerticalAlignment = VerticalAlignment.Center;
            value_text.VerticalAlignment = VerticalAlignment.Center;
            key_text.HorizontalAlignment = HorizontalAlignment.Left;
            value_text.HorizontalAlignment = HorizontalAlignment.Left;

            // column color
            return_grid.Background = new SolidColorBrush(ColorLink[color]);
            // sizing
            return_grid.Height = Defaults.Height;
            return_grid.Width = Defaults.Width;
            // text column
            Grid.SetColumn(key_text, 0);
            Grid.SetColumn(value_text, 1);
            // add as children
            return_grid.Children.Add(key_text);
            return_grid.Children.Add(value_text);

            return return_grid;
        }

        // --- TO/FROM ---

        /// <summary>
        /// Converts this class into a regular Dictionary
        /// </summary>
        /// <returns> Dictionary<string, string> </returns>
        public Dictionary<string, SearchValue> ToDictionary()
        {
            return new Dictionary<string, SearchValue>(this);
        }

        /// <summary>
        /// Converts dictionary to displayable dictionary
        /// </summary>
        /// <param name="dict"> The dictionary to convert from </param>
        /// <returns> Displayable dictionary </returns>
        public static DisplayableDictionary FromDictionary(Dictionary<string, SearchValue> dict)
        {
            var casted_dict = new DisplayableDictionary();
            foreach (string key in dict.Keys.ToList())
            {
                casted_dict[key] = dict[key];
            }
            return casted_dict;
        }
    }
}
