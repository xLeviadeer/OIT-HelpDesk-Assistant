using System.Reflection;
using System.Windows;
using System.Collections.Generic;
using System;

/// <summary>
/// Class for ToString formatting
/// </summary>
class StringFormat {

    /// <summary>
    /// Creates a ToString formatted class string
    /// </summary>
    /// <param name="input_class"> The specific class instance being displayed </param>
    /// <param name="flags"> List of BindingFlags to include </param>
    /// <returns> String </returns>
    public static string ToString<T>(T input_class, params BindingFlags[] flags) where T : class {
        List<string> str_list = new List<string>(); // list of strings to be combined

        // add default flags to the flags
        List<BindingFlags> flags_list = flags.ToList();
        flags_list.Add(BindingFlags.Public);
        flags_list.Add(BindingFlags.NonPublic);
        flags_list.Add(BindingFlags.Instance);
        // combine the BindingFlags with an OR statement using a bitwise OR
        BindingFlags combinedFlags = 0;
        foreach (var flag in flags_list)
        {
            combinedFlags |= flag;
        }

        // go through each field and add it to the str_list
        FieldInfo[] fields = input_class.GetType().GetFields(combinedFlags);
        foreach (FieldInfo field in fields) { // iterate through fields
            // Get the value of each field
            object? value = field.GetValue(input_class);
            str_list.Add($"{field.Name}: {value}");
        }
        PropertyInfo[] properties = input_class.GetType().GetProperties(combinedFlags);
        foreach (PropertyInfo property in properties) {
            if (!property.CanRead) continue; // will not print values without a getter
            object? value; // set value to nothing before processing
            var index_params = property.GetIndexParameters(); // get index values if can
            if (index_params.Length != 0) // this means the property is indexed (like a list or array) and needs different processing
            {
                // iterate through the values and put them in a listed string
                string listed_values = "";
                foreach (var ele in index_params)
                {
                    listed_values += $", {ele}";
                }
                value = $"[ {listed_values[2..]} ]"; // set value (slices the first 2 values (will be comma space) out)
            } else // process as a singular (non index) value
            {
                value = property.GetValue(input_class); // set value
            }
            str_list.Add($"{property.Name}: {value}"); // add to final prinout
        }

        // return the combined string
        return String.Join('\n', str_list);
    }

    /// <summary>
    /// Formats a string as name, meaning it has a first capital letter
    /// </summary>
    /// <param name="str"> The input string "name" </param>
    /// <param name="enforce_lowercase"> Whether or not to set all to lowercase except the first or leave as is </param>
    /// <returns> String Name formatted output </returns>
    public static string Name(string str, bool enforce_lowercase=false) {
        if (enforce_lowercase == true) { str = str.ToLower(); }; // sets a string to lowercase if enforce_lowercase is true
        return $"{char.ToUpper(str[0])}{str[1..]}"; // capitalizes the first letter and slices it out
    }

    /// <summary>
    /// Formats a collection (list-likes) into a string
    /// </summary>
    /// <typeparam name="T"> Expected to be the type that the input_class holds </typeparam>
    /// <param name="input_class"> The class to print values of </param>
    /// <returns> String of values listed from the enumerator </returns>
    public static string Collection<T>(IEnumerable<T> input_class) {
        // checks if T is a KeyValue pair and throws an error if it is
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) {
            throw new ArgumentException("The input was a KeyValuePair not a Collection");
        }

        // build string and return
        string str = "";
        foreach (var ele in input_class) {
            str += $", {ele}";
        }
        if (str.Length > 2) { // can only slice if big enough
            return $"[ {str[2..]} ]";
        } else {
            return str;
        }
        
    }

    /// <summary>
    /// Formats a KeyValuePair (dictionary-likes) into a string
    /// </summary>
    /// <typeparam name="K"> Expected to be the type of key </typeparam>
    /// <typeparam name="V"> Expected to be the type of value </typeparam>
    /// <typeparam name="T"> Expected to be the type of value which holds K and V, for example "Dictionary<K,V>" would be T </typeparam>
    /// <param name="input_class"> The class to print values of </param>
    /// <returns> String of values listed from the enumerator </returns>
    public static string KeyValuePair<K, V, T>(T input_class) where T : IEnumerable<KeyValuePair<K, V>> {
        // build string and return
        string str = "";
        foreach (KeyValuePair<K, V> kvp in input_class) {
            str += $", {kvp.Key}:{kvp.Value}";
        }
        return $"[ {str[2..]} ]";
    }
}