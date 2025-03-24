using Newtonsoft.Json;
using OIT_HelpDesk_Assistant_v2.Lib;
using OIT_HelpDesk_Assistant_v2.Phonetics;
using OIT_HelpDesk_Assistant_v2.SearchPage;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace OIT_HelpDesk_Assistant_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // --- VARIABLES ---

        public const string InvalidCharacters = @"[\\/:*?""|<>]";

        // --- WINDOW SETTINGS ---

        /// <summary>
        /// Function that runs once everything has been loaded and sized already from the XAML
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.HideMinimizeAndMaximizeButtons();

            // assigns the width after the width of the sub-items have been determined
            AssignWidth();
        }

        /// <summary>
        /// Sets the width/minwidth based on the tab items and the size of the refresh button
        /// </summary>
        private void AssignWidth()
        {
            // tab items width
            double width = 0;
            foreach (TabItem tab in AllTabs.Items)
            {
                width += tab.ActualWidth;
            }
            width += 15; // required space so tabs don't stack

            // refresh button width
            width += RefreshColumn.ActualWidth;

            // check width is not too high
            if (width > MaxWidth)
            {
                MessageBox.Show("There are too many tabs! Try removing some before adding more tabs.", "Error");
                Application.Current.Shutdown();
                return;
            }

            // set width
            this.Width = width;
            this.MinWidth = width;
        }

        // --- CONSTRUCTOR ---

        public MainWindow()
        {
            InitializeComponent();
            Load();
        }

        /// <summary>
        /// Loads the tabs from the Tabs/Tabs.json location and loads phonetics, aka loads the main window's contents 
        /// </summary>
        private void Load()
        {
            // -- LOAD PHONETICS --

            LoadPhonetics();

            // -- LOAD TABS --

            // for all files in the static directory
            string tabs_folder_path = "Static";
            foreach (string file_name_full in Directory.GetFiles(tabs_folder_path, "*.json"))
            {
                // get name without json
                string file_name = Path.GetFileNameWithoutExtension(file_name_full);

                // check for invalid characters, though this should be impossible since they are already file names
                if (Regex.IsMatch(file_name, InvalidCharacters)) // check if the name contains invalid characters
                {
                    MessageBox.Show($"The following name is invalid: {file_name_full}, names cannot include the following characters: {InvalidCharacters}");
                    Application.Current.Shutdown();
                    return;
                }

                // try to read to create a search page tab item from this file
                TabItem tabItem = new()
                {
                    Header = file_name,
                    Content = new SearchPageGrid()
                    {
                        Name = file_name.Replace(" ", ""), // removes all spaces
                        Path = file_name_full,
                        GridBorderThickness = new Thickness(1),
                        ColumnBreakLineThickness = 1.0,
                        MaxColumnAmount = 2,
                    }
                };

                // add to tab_name control
                AllTabs.Items.Add(tabItem);
            }
        }

        // --- EVENT METHODS ---

        /// <summary>
        /// Checks what tab_name is currently selected and refreshed the related items
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var selected_tab = (AllTabs.SelectedItem as TabItem); // get tab_name
            if (selected_tab != null)
            {
                switch (selected_tab.Header.ToString()) // switch for the name of the tab_name
                {
                    case "Phonetics":
                        // temporarily disables warning messages
                        PhoneticsDefault.DisplaySetWarning = false;
                        LoadPhonetics();
                        PhoneticsDefault.DisplaySetWarning = true;
                        break;
                    default: // catches for search page grids
                        // check if it's not null and a search page grid
                        if ((AllTabs.SelectedItem != null) && (selected_tab.Content is SearchPageGrid curr_page))
                        {
                            curr_page.Load(); // loads it again
                            curr_page.Display();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Function specific variable for tracking if enter is currently pressed
        /// </summary>
        private bool EnterPressed = false;

        /// <summary>
        /// What to do when keys are pressed
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var selected_tab = (AllTabs.SelectedItem as TabItem); // get tab_name
            if (selected_tab != null)
            {
                // function for easy color shift
                void ShiftColorToAndCopy(SearchPageGrid tab_user_control)
                {
                    // enter pressed
                    if (EnterPressed == true) return; // do nothing
                    EnterPressed = true;

                    // check if anything has been searched
                    if (string.IsNullOrWhiteSpace(tab_user_control.SearchTextBox.Text)) return; // do nothing

                    // check if any items
                    var first_item = tab_user_control.ItemsGrid.Children[0];
                    if (first_item == null) return; // do nothing

                    // cast as copytextgrid
                    if (first_item is CopyTextGrid grid)
                    {
                        // get the background brush
                        SolidColorBrush? background_brush = grid.Background as SolidColorBrush;
                        if (background_brush == null) return; // do nothing
                        // change color
                        Color color = DisplayableDictionary.ShiftBackgroundColor(background_brush.Color, true);
                        // apply color change
                        grid.Background = new SolidColorBrush(color);

                        // set to clipboard
                        Utility.CopyToClipboardWPF(grid, grid.CopyText);
                    }
                }
                switch (selected_tab.Header.ToString()) // switch for the name of the tab_name
                {
                    case "Phonetics":
                        switch (e.Key)
                        {
                            case Key.Enter:
                                if (Phonetics_ChangeButton.IsEnabled == true) Phonetics_ChangeButton_Click(sender, e); // simulate pressing change button if enter pressed
                                break;
                        }
                        break;
                    default: // catches all other index
                        switch (e.Key)
                        {
                            case Key.Enter: // copies to clipboard and color
                                // gets the current item
                                TabItem curr_tab_item = (TabItem)AllTabs.SelectedItem;

                                // check if it's not null and a search page grid
                                if ((AllTabs.SelectedItem != null) && (curr_tab_item.Content is SearchPageGrid curr_page))
                                {
                                    ShiftColorToAndCopy(curr_page);
                                }
                                break;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// What to do when keys are released
        /// </summary>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            var selected_tab = (AllTabs.SelectedItem as TabItem); // get tab_name
            if (selected_tab != null)
            {
                // function for easy color shift
                void ShiftColorBack(SearchPageGrid tab_user_control)
                {
                    // enter pressed
                    if (EnterPressed == false) return; // do nothing
                    EnterPressed = false;

                    // check if anything has been searched
                    if (string.IsNullOrWhiteSpace(tab_user_control.SearchTextBox.Text)) return; // do nothing

                    // check if any items
                    var first_item = tab_user_control.ItemsGrid.Children[0];
                    if (first_item == null) return; // do nothing

                    // cast as copytextgrid
                    if (first_item is CopyTextGrid grid)
                    {
                        // get the background brush
                        SolidColorBrush? background_brush = grid.Background as SolidColorBrush;
                        if (background_brush == null) return; // do nothing
                                                              // change color
                        Color color = DisplayableDictionary.ShiftBackgroundColor(background_brush.Color, false);
                        // apply color change
                        grid.Background = new SolidColorBrush(color);
                    }
                }
                // no switch because we dont include phonetics
                switch (e.Key)
                {
                    case Key.Enter: // copies to clipboard and color
                                    // gets the current item
                        TabItem curr_tab_item = (TabItem)AllTabs.SelectedItem;

                        // check if it's not null and a search page grid
                        if ((AllTabs.SelectedItem != null) && (curr_tab_item.Content is SearchPageGrid curr_page))
                        {
                            ShiftColorBack(curr_page);
                        }
                        break;
                }
            }
        }
    }
}