using System.IO;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json;

/// <summary>
/// Class exception for when attempting to modify a file that doesn't end with .json
/// </summary>
sealed class JsonExtensionException : Exception
{
    public JsonExtensionException() : base() { }

    public JsonExtensionException(string message) : base(message) { }

    public JsonExtensionException(string message,  Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Class exception for when a file has no contents
/// </summary>
sealed class FileEmptyException : Exception
{
    public FileEmptyException() : base() { }

    public FileEmptyException(string message) : base(message) { }

    public FileEmptyException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Class for handling Json writing, reading, storing and overwriting
/// </summary>
sealed class Json { 
    private static string CWD = Directory.GetCurrentDirectory();

    /// <summary>
    /// Takes a path input and prepends it with the CWD and turns it into a path
    /// </summary>
    /// <param name="path"> The path as a list without a CWD </param>
    /// <returns> String path </returns>
    public static string CreatePath(List<string> path) {
        return Path.Combine(path.Prepend(CWD).ToArray());
    }

    /// <summary>
    /// Verifies the path ends with .json
    /// </summary>
    /// <param name="path"> Path as a list </param>
    /// <exception cref="JsonExtensionException"></exception>
    private static void VerifyPathIsJson(List<string> path) {
        string last_variable_in_path = path[path.Count - 1];
        VerifyPathIsJson(last_variable_in_path); // run string version
    }

    /// <summary>
    /// Verifies the path ends with .json
    /// </summary>
    /// <param name="path_string"> Path as a string</param>
    /// <exception cref="JsonExtensionException"></exception>
    private static void VerifyPathIsJson(string path_string) {
        if (!path_string.EndsWith(".json")) throw new JsonExtensionException($"The following path does not end with a .json file (path ending) \"{path_string}\"");
    }

    /// <summary>
    /// Verifies the path is valid in the os as not a blank string and a root path
    /// </summary>
    /// <param name="path_string"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void VerifyValidPath(string path_string) {
        // check if it's not a possible path
        if (string.IsNullOrWhiteSpace(path_string) || !Path.IsPathRooted(path_string)) { 
            throw new ArgumentException($"The following path is invalid: \"{path_string}\"");
        }
    }

    /// <summary>
    /// Runs all verficiation methods on the path string
    /// </summary>
    /// <param name="path_string"> The path to verify as a string </param>
    /// <returns> The path with the CWD prepended </returns>
    private static string PathVerification(string path_string) {
        // make sure the path ends with .json
        VerifyPathIsJson(path_string);
        // prepends the CWD and combines it into a path
        path_string = Path.Combine(CWD, path_string);
        // check if the path is a valid path that can be written/read from/to
        VerifyValidPath(path_string);

        return path_string;
    }

    /// <summary>
    /// Writes an input class as a json file to the specific location
    /// </summary>
    /// <param name="path_string"> Path to write json data to. Path is automatically appended to the current working directory </param>
    /// <param name="content"> Class as content to be written to the Json file. </param>
    /// <param name="create_directories"> Whether or not non-existent directories can be written </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public static void Write<T>(string path_string, T content, bool create_directories=true) where T : class {
        path_string = PathVerification(path_string);

        // check directory (and write)
        string path_directory = Path.GetDirectoryName(path_string)!; // exclamation mark (!) forgives the possibility of the value being null
        if (!Directory.Exists(path_directory)) { // if directory doesnt exist
            if (create_directories == true) { // if directories can be created
                Directory.CreateDirectory(path_directory);
            } else {
                throw new DirectoryNotFoundException($"The following path is not an existing directory: \"{path_directory}\"");
            }
        }

        // write file
        var serializer_settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }; // ignores self referencing loops
        string json_string = JsonConvert.SerializeObject(new List<T> { content }, serializer_settings);
        try
        {
            using (StreamWriter file = new StreamWriter(path_string))
            {
                file.Write(json_string);
            }
        }
        catch (IOException) { throw; }
        
    }

    /// <summary>
    /// Writes an input class as a json file to the specific location
    /// </summary>
    /// <param name="path"> Path to write json data to. Path is automatically appended to the current working directory </param>
    /// <param name="content"> Class as content to be written to the Json file. </param>
    /// <param name="create_directories"> Whether or not non-existent directories can be written </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static void Write<T>(List<string> path, T content, bool create_directories=true) where T : class {
        // run string version
        Write(Path.Combine(path.ToArray()), content, create_directories);
    }

    /// <summary>
    /// Reads a whole json file
    /// </summary>
    /// <param name="path_string"> Path to read Json data from. Path is automatically appended to the current working directory </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static T Read<T>(string path_string) where T : class {
        path_string = PathVerification(path_string);

        // check if the file exists or not to read
        if (!File.Exists(path_string)) {
            throw new FileNotFoundException($"The following path is not an existing file: {path_string}");
        }

        // read file
        using (StreamReader file = new StreamReader(path_string)) {
            string jsonString = file.ReadToEnd();
            List<T>? contents = JsonConvert.DeserializeObject<List<T>>(jsonString); // can throw errors, but I let it because I want the program to stop if one of them occurs
            if (contents == null) throw new FileEmptyException($"The following file read as null entirely: {path_string}"); // throw error for null here
            if (contents == null) throw new FileEmptyException($"The following file has no contents: {path_string}"); // throw error if file is empty
            if (contents.Count == 0) throw new FileEmptyException($"The following JSON file has no contents: {path_string}"); // throw error if list is empty
            return contents[0];
        }
    }

    /// <summary>
    /// Reads a whole json file
    /// </summary>
    /// <param name="path"> Path to read Json data from. Path is automatically appended to the current working directory </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static T Read<T>(List<string> path) where T : class {
        // run string version
        return Read<T>(Path.Combine(path.ToArray()));
    }

    /// <summary>
    /// Gets a value out of a class Json rather than reading the whole class Json
    /// </summary>
    /// <param name="path_string"> Path to read Json data from. Path is automatically appended to the current working directory </param>
    /// <param name="target"> The name of the variable (property/field) to target </param>
    /// <returns> Returns the value of either the field or property that was found </returns>
    /// <exception cref="ArgumentException"></exception>
    public static object? ReadFor<T>(string path_string, string target) where T : class {
        T json = Read<T>(path_string); // read the json object

        // check for a property
        PropertyInfo? propertyInfo = typeof(T).GetProperty(target);
        if (propertyInfo == null) {
            // check for a field
            FieldInfo? fieldInfo = typeof(T).GetField(target);
            if (fieldInfo == null) { // runs if both field and property cannot be found
                throw new ArgumentException($"Could not find a property with the name: \"{target}\" of type: \"{typeof(T)}\"");
            }
            return fieldInfo.GetValue(json); // return field
        }
        return propertyInfo.GetValue(json); // return property
    }

    /// <summary>
    /// Gets a value out of a class Json rather than reading the whole class Json
    /// </summary>
    /// <param name="path"> Path to read Json data from. Path is automatically appended to the current working directory </param>
    /// <param name="target"> The name of the variable (property/field) to target </param>
    /// <returns> Returns the value of either the field or property that was found </returns>
    /// <exception cref="ArgumentException"></exception>
    public static object? ReadFor<T>(List<string> path, string target) where T : class {
        // use string version
        return ReadFor<T>(Path.Combine(path.ToArray()), target);
    }

    /// <summary>
    /// Deletes a Json file
    /// </summary>
    /// <remarks>
    /// The file must end with .json for it to be deleted this way
    /// </remarks>
    /// <param name="path_string"> The path to the file </param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void Delete(string path_string) {
        path_string = PathVerification(path_string);

        // check if the file exists or not to delete
        if (!File.Exists(path_string)) {
            throw new FileNotFoundException($"The following path is not an existing file: {path_string}");
        } // if exists, delete
        File.Delete(path_string);
    }

    /// <summary>
    /// Deletes a Json file
    /// </summary>
    /// <remarks>
    /// The file must end with .json for it to be deleted this way
    /// </remarks>
    /// <param name="path"> The path to the file </param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void Delete(List<string> path)
    {
        // run string version
        Delete(Path.Combine(path.ToArray()));
    }
}