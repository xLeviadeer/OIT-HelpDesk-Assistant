using Newtonsoft.Json;
using OIT_HelpDesk_Assistant_v2.Phonetics;
using OIT_HelpDesk_Assistant_v2.SearchPage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static OIT_HelpDesk_Assistant_v2.SearchPage.DisplayableDictionary;
using static System.Collections.Specialized.BitVector32;

namespace OIT_HelpDesk_Assistant_v2
{
    /// <summary>
    /// Interaction logic for SearchPageGrid.xaml
    /// </summary>
    public partial class SearchPageGrid : UserControl
    {
        // --- VARIABLES ---

        /// <summary>
        /// Default value container
        /// </summary>
        private static class SearchPageGridDefaults
        {
            /// <summary>
            /// The default height of items in the grid
            /// </summary>
            public const double ItemHeight = 25.0;

            /// <summary>
            /// The default space between rows
            /// </summary>
            public const double RowSpacing = 1.0;

            /// <summary>
            /// The max amount of characters a name can have
            /// </summary>
            [Obsolete("Names are not currently length validated")]
            public const int MaxNameLength = 40;

            /// <summary>
            /// The max amount of characters a value can have
            /// </summary>
            [Obsolete("Values are not currently length validated")]
            public const int MaxValueLength = 16;
        }

        [JsonIgnore]
        public static readonly DependencyProperty ColumnBreakLineThicknessProperty =
            DependencyProperty.Register("ColumnBreakLineThickness", typeof(double), typeof(SearchPageGrid), new PropertyMetadata(0.0, OnColumnBreakLineThicknessChanged));

        [JsonIgnore]
        public double ColumnBreakLineThickness
        {
            get { return (double)GetValue(ColumnBreakLineThicknessProperty); }
            set { SetValue(ColumnBreakLineThicknessProperty, value); }
        }

        /// <summary>
        /// Register the BorderThickness property
        /// </summary>
        [JsonIgnore]
        public static readonly DependencyProperty GridBorderThicknessProperty =
            DependencyProperty.Register("GridBorderThickness", typeof(Thickness), typeof(SearchPageGrid), new PropertyMetadata(new Thickness(0), OnItemsBorderThicknessChanged));

        [JsonIgnore]
        public Thickness GridBorderThickness
        {
            get { return (Thickness)GetValue(GridBorderThicknessProperty); }
            set { SetValue(GridBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Register the MaxColumnAmount property
        /// </summary>
        [JsonIgnore]
        public static readonly DependencyProperty MaxColumnAmountProperty =
            // sets name (MaxColumnAmount), type (int), default value (1), and the function that runs when the value is changed (OnMaxColumnAmountChanged)
            DependencyProperty.Register("MaxColumnAmount", typeof(int), typeof(SearchPageGrid), new PropertyMetadata(1, OnMaxColumnAmountChanged));

        /// <summary>
        /// CLR wrapper for the MaxColumnAmount property
        /// </summary>
        [JsonIgnore]
        public int MaxColumnAmount
        {
            get { return (int)GetValue(MaxColumnAmountProperty); }
            set { SetValue(MaxColumnAmountProperty, value); }
        }

        /// <summary>
        /// Register the Path property
        /// </summary>
        [JsonIgnore]
        public static readonly DependencyProperty PathProperty =
            // sets name (MaxColumnAmount), type (int), default value (1), and the function that runs when the value is changed (OnMaxColumnAmountChanged)
            DependencyProperty.Register("Path", typeof(string), typeof(SearchPageGrid), new PropertyMetadata("", OnPathChanged));

        /// <summary>
        /// CLR wrapper for the Path property
        /// </summary>
        [JsonIgnore]
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        [JsonProperty("data")]
        public DisplayableDictionary DisplayData { get; private set; } = new();

        // --- CONSTRUCTOR ---

        public SearchPageGrid()
        {
            InitializeComponent();
            Display();
        }

        // --- EVENT METHODS ---

        /// <summary>
        /// Event function runs when the MaxColumnAmount changes (is set initially)
        /// </summary>
        private static void OnMaxColumnAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchPageGrid search_page_grid)
            {
                search_page_grid.Display();
            }
        }

        /// <summary>
        /// Event function runs when the Path changes (is set initially)
        /// </summary>
        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is SearchPageGrid search_page_grid)
            {
                search_page_grid.Load();
                search_page_grid.Display();
            }
        }

        /// <summary>
        /// Event function runs when the ItemsBorderThickness changes (is set initially)
        /// </summary>
        private static void OnItemsBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchPageGrid search_page_grid)
            {
                search_page_grid.Display();
            }
        }

        /// <summary>
        /// Event function runs when the ColumnBreakLineThickness changes (is set initially)
        /// </summary>
        private static void OnColumnBreakLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchPageGrid search_page_grid)
            {
                search_page_grid.Display();
            }
        }

        /// <summary>
        /// When typing in the search box, change the display
        /// </summary>
        public void SearchPageGrid_SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            // quick access variables for search text
            string search_text = SearchTextBox.Text;

            if (search_text.Length <= 0) // if not searching
            {
                Display();
            } else // if searching
            {
                // search the data to find aliases and name matches
                DisplayableDictionary search_data = new();
                foreach (string key in DisplayData.Keys.ToList())
                {
                    // if value contains
                    if (key.ToLower().Contains(search_text.ToLower()))
                    {
                        search_data[key] = DisplayData[key];
                        continue;
                    }

                    // if aliases contians
                    foreach (string alias in DisplayData[key].Aliases.ToList())
                    {
                        if (alias.ToLower().Contains(search_text.ToLower()))
                        {
                            search_data[key] = DisplayData[key];
                            continue;
                        }
                    }

                    // if value contains
                    if (DisplayData[key].Value.ToLower().Contains(search_text.ToLower()))
                    {
                        search_data[key] = DisplayData[key];
                        continue;
                    }
                }

                // display custom data, only 1 column
                Display(search_data, force_single_column:true); 
            }
        }

        // --- METHODS ---

        /// <summary>
        /// Loads the current object
        /// </summary>
        /// <remarks>
        /// This loads the data directly, not the search page grid as a whole
        /// </remarks>
        public void Load()
        {
            DisplayableDictionary loaded_obj;
            // try to load
            try
            {
                loaded_obj = Json.Read<DisplayableDictionary>(Path);
            }
            catch (FileNotFoundException)
            { // if file not found then critical error
                MessageBox.Show($"{Path} could not be read, click OK to exit the program");
                Application.Current.Shutdown();
                return;
            } catch (JsonException ex) when (ex is JsonReaderException || ex is JsonSerializationException)
            {
                MessageBox.Show($"{Path} is not a correctly formatted JSON file, click OK to exit the program");
                Application.Current.Shutdown();
                return;
            } catch (FileEmptyException)
            {
                DisplayNoItemsFoundIf(true);
                return;
            }

            DisplayData = loaded_obj;
        }

        /// <summary>
        /// Stores the current object
        /// </summary>
        /// <remarks>
        /// This loads the data directly, not the search page grid as a whole
        /// </remarks>
        public void Store()
        {
            Json.Write(Path, this.DisplayData);
        }

        /// <summary>
        /// Deleted the current object
        /// </summary>
        /// <remarks>
        /// This loads the data directly, not the search page grid as a whole
        /// </remarks>
        public void Remove()
        {
            Json.Delete(Path);
        }

        /// <summary>
        /// Sets the ItemsGrid to display "No Items Found" if the condition is true
        /// </summary>
        /// <returns> The value of the input boolean </returns>
        /// <param name="condition"> The condition that must be met for items to be NOT displayed </param>
        private bool DisplayNoItemsFoundIf(bool condition)
        {
            if (condition == true)
            {
                BorderGrid.Visibility = Visibility.Collapsed;
                ItemsGrid.Children.Add(new Label() { Content = "No items found" });
                return true;
            }
            BorderGrid.Visibility = Visibility.Visible;
            return false;
        }

        /// <summary>
        /// Displays the passed content to the search page
        /// </summary>
        /// <param name="force_single_column"> Forces interface to only display a single column </param>
        /// <param name="display_data"> What data to display. If left empty ues the DisplayData </param>
        public void Display(DisplayableDictionary? display_data=null, bool force_single_column=false)
        {
            // clear the currently displayed items (and border)
            ItemsGrid.Children.Clear();

            // if the displaydata is empty, display no items found
            display_data ??= DisplayData;
            if (DisplayNoItemsFoundIf(display_data.Count == 0)) return;

            // sort the DisplayData alphabetically (and set it)
            display_data = DisplayableDictionary.FromDictionary(
                display_data.ToDictionary()
                .OrderBy(kvp => kvp.Key) // sort by keys
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) // convert it back to a dictionary
                );

            // for the max amount of columns, add them
            ItemsGrid.ColumnDefinitions.Clear();
            for (int j = 0; (j < MaxColumnAmount) && (j < display_data.Count); j++) // && statement lets the items fill more space if there isn't enough of them
            {
                // adds only 1 column
                if (force_single_column == true)
                {
                    ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    break;
                }

                // add column
                ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // for all data, add items to the grid
            ItemsGrid.RowDefinitions.Clear();
            int last_row = -1; // tracks the last row
            DisplayableDictionary.Colors color = DisplayableDictionary.Colors.GrayDark; // tracks the color
            int i = 0;
            while (i < display_data.Count) // uses a while loop so that i is scoped outside of the loop
            {
                // find column and row to set to
                int row = (int)Math.Floor((double)i / ItemsGrid.ColumnDefinitions.Count);
                int column = i % ItemsGrid.ColumnDefinitions.Count;

                // alternate colors (only if the row has changed)
                if (last_row != row)
                {
                    // create grid column
                    ItemsGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(SearchPageGridDefaults.ItemHeight, GridUnitType.Pixel) }); // creates a new row of the default hieght

                    // swaps the color to the opposite (sets color)
                    if (color == DisplayableDictionary.Colors.GrayDark)
                    {
                        color = DisplayableDictionary.Colors.GrayLight;
                    }
                    else if (color == DisplayableDictionary.Colors.GrayLight)
                    {
                        color = DisplayableDictionary.Colors.GrayDark;
                    }
                }
                last_row = row; // sets last row

                // create sub-grid and add spacing
                Grid curr_grid = display_data.GetGrid(i, color, (SearchPageGridDefaults.ItemHeight / 7)); // get a grid with margin of default height / 7
                double row_space_margin = (double)SearchPageGridDefaults.RowSpacing / 2;
                curr_grid.Margin = new Thickness(0, row_space_margin, 0, row_space_margin); // add space between items (divides by 2 to add even space between each)
                
                // set to column and row
                Grid.SetRow(curr_grid, row);
                Grid.SetColumn(curr_grid, column);
                ItemsGrid.Children.Add(curr_grid); // add to grid
                
                // incremement i
                i += 1;
            }

            // if there are empty slots at the end of the grid
            if ((i % ItemsGrid.ColumnDefinitions.Count) != 0)
            {
                // the remainder of the grid columns as filler
                while ((i % ItemsGrid.ColumnDefinitions.Count) != 0)
                {
                    // find column and row to set to
                    int row = (int)Math.Floor((double)i / ItemsGrid.ColumnDefinitions.Count);
                    int column = i % ItemsGrid.ColumnDefinitions.Count;

                    // set to grid
                    var blank_grid = new Grid() { Background = new SolidColorBrush(display_data.Defaults.GrayVeryLight) };
                    Grid.SetRow(blank_grid, row);
                    Grid.SetColumn(blank_grid, column);
                    ItemsGrid.Children.Add(blank_grid);

                    // increment
                    i += 1;
                }
            }

            // add line between columns
            for (int j = 0; (j <= MaxColumnAmount) && (j <= display_data.Count); j++) // && statement lets the items fill more space if there isn't enough of them
            {
                var line = new Line()
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = 1,
                    StrokeThickness = ColumnBreakLineThickness,
                    Stroke = Brushes.Black,
                    Stretch = Stretch.Fill,
                };
                Grid.SetRow(line, 0);
                Grid.SetRowSpan(line, ItemsGrid.RowDefinitions.Count);
                Grid.SetColumn(line, j);
                ItemsGrid.Children.Add(line);
            }

            // adjust the size of the border and grid (to the amount of rows)
            double height = SearchPageGridDefaults.ItemHeight * Math.Ceiling((double)display_data.Count / ItemsGrid.ColumnDefinitions.Count);
            BorderGrid.Height = height;
        }
    }
}
